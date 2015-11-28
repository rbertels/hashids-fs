namespace Hashids

/// Allows the encoding and decoding of hashids.
[<RequireQualifiedAccess>]
module Hashid = 
    open System
    open HashidConfiguration

    // Accumulator used to hash numbers.
    type private HashAccumulator = 
        { Alphabet : string
          Index : int64
          Id : string }

    // Accumulator used to unhash numbers.
    type private UnhashAccumulator = 
        { Alphabet : string
          Number : int64 }

    // Recursively hashes a number until 0 is reached.
    let rec private hashNumber (number : int64) (alphabet : string) (prevHash : string) : string = 
        let toInsert = pickCharacter alphabet number
        let next = alphabet |> length |> (/) number
        let hash = toInsert + prevHash
        match next with
        | 0L -> hash
        | x -> hashNumber x alphabet hash
    
    // Unhashes a part of an id.
    let private unhashNumber (idpart : string) (alphabet : string) = 
        idpart
        |> Seq.fold (fun acc (character : char) -> 
               let pos = alphabet.IndexOf(character) |> int64
               (fst (acc) + pos * pown (length alphabet) (idpart.Length - snd (acc) - 1), (snd (acc) + 1))) (0L, 0)
        |> fst 
    
    // Encodes all provided numbers.
    let private encodeNumbers (config : HashidConfiguration) (initialValue : string) (numbers : int64 []) = 
        let hasher (acc : HashAccumulator) number =
            let buffer = initialValue + config.Salt + acc.Alphabet
            let tempalpha = consistentShuffle acc.Alphabet buffer.[0..acc.Alphabet.Length]
            let last = hashNumber number tempalpha "" 
                       
            let nextAccumulator = 
                { Alphabet = tempalpha
                  Index = acc.Index + 1L
                  Id = acc.Id + last }
            if (nextAccumulator.Index < numbers.LongLength) then 
                let v = pickItem config.Separators (number % (int64 (last.[0]) + acc.Index))
                { nextAccumulator with Id = nextAccumulator.Id + string v }
            else nextAccumulator

        numbers |> Seq.fold (hasher) { Alphabet = config.Alphabet
                                       Index = 0L
                                       Id = initialValue }
    
    // Ensures that a given id is equal to or greater than the minimal configured length.
    let private ensureMinimalLength (config : HashidConfiguration) (seed : int64) (alphabet : string) (id : string) = 
        let guardCharacter index = pickItem config.Guards (id.[index] |> int64 |> (+) seed) |> string
        
        let rec growId amount alpha value = 
            let shuffledAlphabet = consistentShuffle alpha alpha
            let halfLength = shuffledAlphabet.Length / 2
            let preIndex = max (shuffledAlphabet.Length - (ceil (float amount / 2.0) |> int)) halfLength
            let postIndex = min (floor (float amount / 2.0) |> int) halfLength
            let prefix = shuffledAlphabet.[preIndex..]
            let postfix = shuffledAlphabet.[0..postIndex - 1]
            let resized = prefix + value + postfix
            if resized.Length < config.MinimumLength then 
                growId (config.MinimumLength - resized.Length) shuffledAlphabet resized
            else resized
        match max (config.MinimumLength - id.Length) 0 with
        | 0 -> id
        | 1 -> 
            let prefix = guardCharacter 0
            prefix + id
        | 2 -> 
            let prefix = guardCharacter 0
            let postfix = guardCharacter 1
            prefix + id + postfix
        | x -> 
            let prefix = guardCharacter 0
            let postfix = guardCharacter 1
            growId (x - 2) alphabet (prefix + id + postfix)
    
    // Removes the padding introduced to meet the minimal length requirements.
    let private shrinkId (config : HashidConfiguration) (id : string) = 
        let parts = id.Split(config.Guards)
        match parts.Length with
        | 1 -> parts.[0]
        | 2 | 3 -> parts.[1]
        | _ -> failwith "Illegal id"

    /// Validates the input numbers.
    [<CompiledName("Validate")>]
    let validateInput (numbers : array<int64>) =
        numbers |> Array.forall((<) 0L) && numbers.Length > 0
    
    /// Encodes 64 bit numbers to an id. 
    [<CompiledName("Encode")>]
    let encode64 (config : HashidConfiguration) ([<ParamArray>] numbers : int64 []) = 
        match validateInput numbers with
        | false -> String.Empty
        | true -> 
            let numberHash = 
                numbers
                |> Seq.mapi (fun index number -> number % (int64 index + 100L))
                |> Seq.sum
            let lottery = pickCharacter config.Alphabet numberHash 
            let encoded = encodeNumbers config lottery numbers
            ensureMinimalLength config numberHash encoded.Alphabet encoded.Id

    /// Decodes 64 bit numbers from an id.
    [<CompiledName("Decode64")>]
    let decode64 (config : HashidConfiguration) (id : string) = 
        let core = shrinkId config id
        match core |> Seq.toList with
        | [] -> [||]
        | '\000' :: tail -> [||]
        | lottery :: tail ->
            let prefix = string lottery + config.Salt
            core.Substring(1).Split(config.Separators)
            |> Array.scan (fun (acc : UnhashAccumulator) idpart -> 
                   let buffer = prefix + acc.Alphabet
                   let alphabet = consistentShuffle acc.Alphabet buffer.[0..acc.Alphabet.Length - 1]
                   { Alphabet = alphabet
                     Number = unhashNumber idpart alphabet }) { Alphabet = config.Alphabet
                                                                Number = 0L }
            |> Array.map (fun acc -> acc.Number)
            |> fun a -> a.[1..]

    /// Encodes 32 bit numbers to an id. 
    [<CompiledName("Encode")>]
    let encode (config : HashidConfiguration) ([<ParamArray>] numbers : int []) = 
        numbers |> Array.map int64 |> encode64 config

    /// Decodes 32 bit numbers from an id.
    [<CompiledName("Decode")>]
    let decode (config : HashidConfiguration) (id : string) = 
        decode64 config id |> Array.map int