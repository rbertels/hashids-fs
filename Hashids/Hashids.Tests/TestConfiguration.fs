module TestConfiguration

open System
open NUnit
open NUnit.Framework
open FsUnit
open Hashids
open HashidConfiguration

[<Test>]
let ``Configuration passes complete transformation with default settings``() = 
    let config = HashidConfiguration.create defaultOptions
    config.Alphabet |> should equal "gjklmnopqrvwxyzABDEGJKLMNOPQRVWXYZ1234567890"
    config.Guards |> should equal "abde"
    config.Separators |> should equal "cfhistuCFHISTU"

[<Test>]
let ``Configuration passes complete transformation with salt``() = 
    let options = { defaultOptions with Salt = "zupdog" }
    let config = HashidConfiguration.create options
    config.Alphabet |> should equal "Jgzj0648Qp7ZdxVqv5MOm3bPak2GNrDYWoe1ElyRLwXn"
    config.Guards |> should equal "KAB9"
    config.Separators |> should equal "UCFfScTuthsiHI"
