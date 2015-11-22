module TestAlphabet

open System
open NUnit
open NUnit.Framework
open FsUnit
open Hashids
open HashidConfiguration

[<Test>]
let ``Validate fails when alphabet too short``() = 
    (fun () -> HashidConfiguration.validateAlphabet "abcd") |> should throw typeof<Exception>

[<Test>]
let ``Validate fails when alphabet not diverse enough``() = 
    (fun () -> HashidConfiguration.validateAlphabet "aaaaaaa") |> should throw typeof<Exception>

[<Test>]
let ``Validate passes when alphabet meets minimal standards``() = HashidConfiguration.validateAlphabet "abcde"

[<Test>]
let ``Validate is case sensitive``() = HashidConfiguration.validateAlphabet "abcdA"
