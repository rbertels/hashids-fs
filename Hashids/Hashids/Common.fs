namespace Hashids

// Common functions shared by the Hashid and HashidConfiguration modules.
[<AutoOpen>]
module internal Common = 
    // Gets the length of a string as an int64.
    let inline length (text : string) = text.Length |> int64
    
    /// Swaps the items at index i1 and index i2 inside a given array.
    let swapItems i1 i2 (items : array<_>) = 
        let tempItems = Array.copy items
        let e1 = tempItems.[i1]
        let e2 = tempItems.[i2]
        tempItems.[i1] <- e2
        tempItems.[i2] <- e1
        tempItems
    
    /// Converts a sequence of chars to a string.
    let toString (characters : seq<char>) = 
        characters
        |> Seq.toArray
        |> System.String
    
    /// Performs a consistent shuffle algoritm.
    let consistentShuffle (value : string) (salt : string) = 
        let saltLength = salt.Length
        let rec shuffle index v p alphabet = 
            let v' = v % saltLength
            let n = int32 (salt.[v'])
            let p' = p + n
            let j = (n + v' + p') % index
            let shuffled = swapItems index j alphabet
            match index with
            | 1 -> shuffled
            | _ -> shuffle (index - 1) (v' + 1) p' shuffled
        match salt with
        | "" -> value
        | _ -> shuffle (value.Length - 1) 0 0 (value.ToCharArray()) |> System.String
    
    /// Gets a single character of a string given an index.
    let inline pickCharacter (text : string) (index : int64) = 
        text.Substring(text |> length |> (%) index |> int32, 1)

    /// Gets a single item of an array given an index.
    let inline pickItem (items : array<_>) (index : int64) = 
        items.[index % items.LongLength |> int32]
