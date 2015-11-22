# Hashids-fs
A hashids implementation in F#.
It allows the encoding of one or more numbers into a single YouTube-like hash.
Please visit http://hashids.org for implementations in other languages.

## Usage

### Open the Hashids namespace
```fsharp
open Hashids
```
### Create a configuration
A HashidConfiguration stores the salt, minimum hash length, alphabet, and separators.
```fsharp
let config = 
    HashidConfiguration.create 
        { Salt = "zupdog"
          MinimumHashLength = 0
          Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"
          Separators = "cfhistuCFHISTU" }
```
Alternatively, the values from the defaultOptions record can be modified.
```fsharp
let config = 
    HashidConfiguration.create 
        { HashidConfiguration.defaultOptions with Salt = "zupdog" }
```
### Create an encode and decode function
Create curried versions of Hashid.encode64 and Hashid.decode64 with a baked in configuration.
```fsharp
let encode = Hashid.encode64 config
let decode = Hashid.decode64 config
```
### Encode and decode numbers
```fsharp
let hash = encode [| 73L; 88L |]
let numbers = decode hash
```
The resulting hash will be `rlVfvd`.

## Building
Hashids-fs uses FAKE for building. (http://fsharp.github.io/FAKE/)
Simply run `build.bat` and look for the files in the `build` folder.