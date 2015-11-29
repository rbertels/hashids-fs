namespace Hashids

/// The options that can be used to call HashidConfiguration.create.
type HashidConfigurationOptions = 
    { /// The unique salt that will be used to hash ids.
      Salt : string
      /// The minimum length of the generated ids.
      MinimumLength : int
      /// The alphabet used to generate the ids.
      /// There should be at least 4 unique characters in the alphabet that aren't separators.
      Alphabet : string
      /// The separator characters.
      Separators : string }

/// Allows the creation and validation of Hashid configurations.
module HashidConfiguration = 
    // The minimum required length of the alphabet.
    [<Literal>]
    let private MinimumAlphabetLength = 4
    
    // The separator ratio used to resize hashes.
    [<Literal>]
    let private SeparatorRatio = 3.5
    
    // The guard denominator.
    [<Literal>]
    let private GuardDenominator = 12.0
    
    /// The default options used to create a HashidConfiguration.
    /// It is recommended to override at least the salt:
    /// { HashidConfiguration.defaultOptions with Salt = "this is my salt" }.
    [<CompiledName("DefaultOptions")>]
    let defaultOptions = 
        { Salt = ""
          MinimumLength = 0
          Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"
          Separators = "cfhistuCFHISTU" }
    
    /// The configuration used for generating ids.
    type HashidConfiguration = 
        { /// The alphabet used to generate the ids.
          Alphabet : string
          /// The separators used to generate the ids.
          Separators : array<char> 
          /// The salt used to generate the ids.
          Salt : string
          /// The minimum length of the ids.
          MinimumLength : int
          /// The guards used to generate the ids.
          Guards : array<char> }
    
    /// Validates if an alphabet meets the requirements.
    /// Throws an exception if the alphabet is invalid.
    [<CompiledName("ValidateAlphabet")>]
    let validateAlphabet (alphabet : seq<char>) = 
        match alphabet
              |> Seq.distinct
              |> Seq.truncate (MinimumAlphabetLength + 1)
              |> Seq.length with
        | 5 -> ()
        | _ -> failwith "Not enough unique alphabet characters"


    // Makes sure the separators were valid and removes them from the alphabet.
    let private filterOptions (options : HashidConfigurationOptions) = 
        let filteredSeparators = 
            options.Separators
            |> Seq.filter (fun c -> options.Alphabet.IndexOf(c) > 0)
            |> toString
        
        let filteredAlphabet = 
            options.Alphabet
            |> Seq.filter(fun c -> filteredSeparators.IndexOf(c) < 0)
            |> toString
        
        { options with Separators = filteredSeparators
                       Alphabet = filteredAlphabet }

    // Shuffles the separators.
    let private shuffleSeparators (options : HashidConfigurationOptions) = 
        let shuffledSeparators = consistentShuffle options.Separators options.Salt
        { options with Separators = shuffledSeparators }

    // Resizes the separators.
    let private resizeOptions (options : HashidConfigurationOptions) = 
        if (options.Separators.Length = 0 || (float options.Alphabet.Length / float options.Separators.Length) > 3.5) then 
            let seplen = 
                match int (ceil <| float options.Alphabet.Length / SeparatorRatio) with
                | 1 -> 2
                | x -> x
            if (seplen > options.Separators.Length) then 
                let difference = seplen - options.Separators.Length
                let extendedSeparators = options.Separators + options.Alphabet.[0..difference - 1]
                let reducedAlphabet = options.Alphabet.[difference..]
                { options with Separators = extendedSeparators
                               Alphabet = reducedAlphabet }
            else { options with Separators = options.Separators.[0..seplen - 1] }
        else options

    // Validates and shuffles the alphabet.
    let private shuffleAlphabet (options : HashidConfigurationOptions) = 
        validateAlphabet options.Alphabet
        let shuffledAlphabet = consistentShuffle options.Alphabet options.Salt
        { options with Alphabet = shuffledAlphabet }

    // Creates the guard characters.
    let private constructGuards (options : HashidConfigurationOptions) = 
        let guardCount = int (ceil <| float options.Alphabet.Length / GuardDenominator)
        if (guardCount < 3) then 
            let guards = options.Separators.[0..guardCount - 1]
            let separators = options.Separators.[guardCount..]
            { Alphabet = options.Alphabet
              MinimumLength = options.MinimumLength
              Separators = separators.ToCharArray()
              Guards = guards.ToCharArray()
              Salt = options.Salt }
        else 
            let guards = options.Alphabet.[0..guardCount - 1]
            let alphabet = options.Alphabet.[guardCount..]
            { Alphabet = alphabet
              MinimumLength = options.MinimumLength
              Separators = options.Separators.ToCharArray()
              Guards = guards.ToCharArray()
              Salt = options.Salt }
    
    /// Validates the provided HashidConfigurationOptions and builds a new HashidConfiguration.
    [<CompiledName("Create")>]
    let create options = 
        options 
        |> (filterOptions
            >> shuffleSeparators
            >> resizeOptions
            >> shuffleAlphabet
            >> constructGuards)
  
    /// Validates the provided options and builds a new HashidConfiguration.
    [<CompiledName("Create")>]
    let withOptions (salt : string) (minimumLength : int) (alphabet: string) (separators : string) =
        create { Salt = salt; MinimumLength = minimumLength; Alphabet = alphabet; Separators = separators }

    /// Builds a new Hashid configuration based on a salt.
    [<CompiledName("Create")>]
    let withSalt salt = 
        create { defaultOptions with Salt = salt }