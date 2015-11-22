module TestEncode

open System
open NUnit
open NUnit.Framework
open FsUnit
open Hashids

[<Test>]
let ``Encode passes with salt``() = 
    let config = HashidConfiguration.create { HashidConfiguration.defaultOptions with Salt = "zupdog" }
    Hashid.encode config [| 1337; 1447 |] |> should equal "2OjfbL"

[<TestCase(7, "K2OjfbL")>]
[<TestCase(8, "K2OjfbLA")>]
[<TestCase(17, "m2xvQK2OjfbLAnroX")>]
[<TestCase(18, "m2xvQK2OjfbLAnroXq")>]
[<TestCase(37, "jbVPakOyMLm2xvQK2OjfbLAnroXq8dRGgwE63")>]
[<TestCase(38, "jbVPakOyMLm2xvQK2OjfbLAnroXq8dRGgwE63W")>]
[<TestCase(157, 
           "g3L2P1dDN2d40Z5GQm3gW8nEYzVrjq7YNV7DeEa0pnGdWO5RjkLX8lezJZD1jbVPakOyMLm2xvQK2OjfbLAnroXq8dRGgwE63W0p47YN5M2wPx3J14rqQovZyglb6mzJoRDXx1pPvyMbeOla6LNwkE7xO5m4W")>]
[<TestCase(158, 
           "g3L2P1dDN2d40Z5GQm3gW8nEYzVrjq7YNV7DeEa0pnGdWO5RjkLX8lezJZD1jbVPakOyMLm2xvQK2OjfbLAnroXq8dRGgwE63W0p47YN5M2wPx3J14rqQovZyglb6mzJoRDXx1pPvyMbeOla6LNwkE7xO5m4W8")>]
let ``Encode resizes hash accordingly`` size result = 
    let config = 
        HashidConfiguration.create { HashidConfiguration.defaultOptions with Salt = "zupdog"
                                                                             MinimumHashLength = size }
    Hashid.encode config [| 1337; 1447 |] |> should equal result

[<TestCase([| -1 |], "")>]
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
let ``Encode returns correct hash`` numbers result = 
    let config = HashidConfiguration.create { HashidConfiguration.defaultOptions with Salt = "this is my salt" }
    Hashid.encode config numbers |> should equal result

[<TestCase([| 9223372036854775807L |], "8RP4M3dGN0ZdR")>]
let ``Encode64 returns correct hash`` numbers result = 
    let config = HashidConfiguration.create { HashidConfiguration.defaultOptions with Salt = "zupdog" }
    Hashid.encode64 config numbers |> should equal result