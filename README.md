# Hashids-fs
A hashids implementation in F#.
It allows the encoding of one or more numbers into a single YouTube-like random string.
Please visit http://hashids.org for implementations in other languages.

![alt text](https://ci.appveyor.com/api/projects/status/vxj71sajea928rl4?svg=true "Build status")

## Usage in F&#35;

### Open the Hashids namespace
```fsharp
open Hashids
```
### Create a configuration
A HashidConfiguration stores the salt, minimum id length, alphabet, and separators.
```fsharp
let config = 
    HashidConfiguration.create 
        { Salt = "this is my salt"
          MinimumHashLength = 0
          Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"
          Separators = "cfhistuCFHISTU" }
```
Alternatively, the values from the defaultOptions record can be modified.
```fsharp
let config = 
    HashidConfiguration.create 
        { HashidConfiguration.defaultOptions with Salt = "this is my salt" }
```
If the only thing that needs changing is the salt, use `withSalt`.
```fsharp
let config = HashidConfiguration.withSalt "this is my salt"
```
### Create an encode and decode function
Create curried versions of Hashid.encode64 and Hashid.decode64 with a baked in configuration.
```fsharp
let encode = Hashid.encode64 config
let decode = Hashid.decode64 config
```
### Encode and decode numbers
```fsharp
let id = encode [| 73L; 88L |]
let numbers = decode id
```
The resulting id will be `rlVfvd`.
## Usage in C&#35;
Using Hashids in C# is very similar to F# except for the currying.
For a more object oriented approach please try Hashids.net (https://github.com/ullmark/hashids.net).
```csharp
var config = HashidConfiguration.Create("this is my salt");
var id = Hashid.Encode(config, new int[] { 1, 2, 3 });
var numbers = Hashid.Decode(config, id);
```
## Installation
Install the package with NuGet.

    Install-Package hashids-fs


