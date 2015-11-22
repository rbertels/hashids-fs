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
          Hash : string }

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
    
    // Unhashes a number.
    let private unhashNumber (hash : string) (alphabet : string) = 
        hash
        |> Seq.fold (fun acc (character : char) -> 
               let pos = alphabet.IndexOf(character) |> int64
               (fst (acc) + pos * pown (length alphabet) (hash.Length - snd (acc) - 1), (snd (acc) + 1))) (0L, 0)
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
                  Hash = acc.Hash + last }
            if (nextAccumulator.Index < numbers.LongLength) then 
                let v = pickItem config.Separators (number % (int64 (last.[0]) + acc.Index))
                { nextAccumulator with Hash = nextAccumulator.Hash + string v }
            else nextAccumulator

        numbers |> Seq.fold (hasher) { Alphabet = config.Alphabet
                                       Index = 0L
                                       Hash = initialValue }
    
    // Ensures that a given hash is equal to or greater than the minimal configured length.
    let private ensureMinimalLength (config : HashidConfiguration) (seed : int64) (alphabet : string) (hash : string) = 
        let guardCharacter index = pickItem config.Guards (hash.[index] |> int64 |> (+) seed) |> string
        
        let rec growHash amount alpha value = 
            let shuffledAlphabet = consistentShuffle alpha alpha
            let halfLength = shuffledAlphabet.Length / 2
            let preIndex = max (shuffledAlphabet.Length - (ceil (float amount / 2.0) |> int)) halfLength
            let postIndex = min (floor (float amount / 2.0) |> int) halfLength
            let prefix = shuffledAlphabet.[preIndex..]
            let postfix = shuffledAlphabet.[0..postIndex - 1]
            let resized = prefix + value + postfix
            if resized.Length < config.MinimumHashLength then 
                growHash (config.MinimumHashLength - resized.Length) shuffledAlphabet resized
            else resized
        match max (config.MinimumHashLength - hash.Length) 0 with
        | 0 -> hash
        | 1 -> 
            let prefix = guardCharacter 0
            prefix + hash
        | 2 -> 
            let prefix = guardCharacter 0
            let postfix = guardCharacter 1
            prefix + hash + postfix
        | x -> 
            let prefix = guardCharacter 0
            let postfix = guardCharacter 1
            growHash (x - 2) alphabet (prefix + hash + postfix)
    
    // Removes the padding introduced to meet the minimal length requirements.
    let private shrinkHash (config : HashidConfiguration) (hash : string) = 
        let parts = hash.Split(config.Guards)
        match parts.Length with
        | 1 -> parts.[0]
        | 2 | 3 -> parts.[1]
        | _ -> failwith "Illegal hash"

    /// Validates the input numbers.
    let validateInput (numbers : array<int64>) =
        numbers |> Array.forall((<) 0L) && numbers.Length > 0
    
    /// Encodes 64 bit numbers to a hash. 
    [<CompiledName("Encode")>]
    let encode64 (config : HashidConfiguration) (numbers : int64 []) = 
        match validateInput numbers with
        | false -> String.Empty
        | true -> 
            let numberHash = 
                numbers
                |> Seq.mapi (fun index number -> number % (int64 index + 100L))
                |> Seq.sum
            let lottery = pickCharacter config.Alphabet numberHash //|> string
            let encoded = encodeNumbers config lottery numbers
            ensureMinimalLength config numberHash encoded.Alphabet encoded.Hash
            
    /// Decodes 64 bit numbers from a hash.
    [<CompiledName("Decode")>]
    let decode64 (config : HashidConfiguration) (hash : string) = 
        let hashCore = shrinkHash config hash
        match hashCore |> Seq.tryHead with
        | Some '\000' | None -> [||]
        | Some lottery ->
            let prefix = string lottery + config.Salt
            hashCore.Substring(1).Split(config.Separators)
            |> Array.scan (fun (acc : UnhashAccumulator) hash -> 
                   let buffer = prefix + acc.Alphabet
                   let alphabet = consistentShuffle acc.Alphabet buffer.[0..acc.Alphabet.Length - 1]
                   { Alphabet = alphabet
                     Number = unhashNumber hash alphabet }) { Alphabet = config.Alphabet
                                                              Number = 0L }
            |> Array.skip 1
            |> Array.map (fun acc -> acc.Number)
    
    /// Encodes 32 bit numbers to a hash. 
    [<CompiledName("Encode")>]
    let encode (config : HashidConfiguration) (numbers : int []) = 
        numbers |> Array.map int64 |> encode64 config

    /// Decodes 32 bit numbers from a hash.
    [<CompiledName("Decode")>]
    let decode (config : HashidConfiguration) (hash : string) = 
        decode64 config hash |> Array.map int