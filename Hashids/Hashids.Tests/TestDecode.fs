module TestDecode

open System
open NUnit
open NUnit.Framework
open FsUnit
open Hashids

[<TestCase([||], "")>]
[<TestCase([| 1 |], "NV")>]
[<TestCase([| 22 |], "K4")>]
[<TestCase([| 333 |], "OqM")>]
[<TestCase([| 9999 |], "kQVg")>]
[<TestCase([| 123000 |], "58LzD")>]
[<TestCase([| 456000000 |], "5gn6mQP")>]
[<TestCase([| 987654321 |], "oyjYvry")>]
[<TestCase([| 1; 2; 3 |], "laHquq")>]
[<TestCase([| 2; 4; 6 |], "44uotN")>]
[<TestCase([| 99; 25 |], "97Jun")>]
[<TestCase([| 1337; 42; 314 |], "7xKhrUxm")>]
[<TestCase([| 683; 94108; 123; 5 |], "aBMswoO2UB3Sj")>]
[<TestCase([| 547; 31; 241271; 311; 31397; 1129; 71129 |], "3RoSDhelEyhxRsyWpCx5t1ZK")>]
[<TestCase([| 21979508; 35563591; 57543099; 93106690; 150649789 |], "p2xkL3CK33JjcrrZ8vsw4YRZueZX9k")>]
let ``Decode returns correct numbers`` numbers hash = 
    let config = HashidConfiguration.create { HashidConfiguration.defaultOptions with Salt = "this is my salt" }
    Hashid.decode config hash |> should equal numbers
