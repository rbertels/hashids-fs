// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"

open Fake

RestorePackages()

let buildDir  = @".\build\"
let testDir   = @".\test\"

Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir]
)

Target "CompileHashids" (fun _ ->
    !! @"Hashids\Hashids.fsproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "Build-Output: "
)

Target "CompileTests" (fun _ ->
    !! @"Hashids.Tests\Hashids.Tests.fsproj"
      |> MSBuildRelease testDir "Build"
      |> Log "Build-Output: "
)

Target "RunTests" (fun _ ->
    !! (testDir + @"\*.Tests.dll")
      |> NUnit (fun p ->
                 {p with
                   DisableShadowCopy = true;
                   OutputFile = testDir + @"TestResults.xml"})
)

"Clean"
    ==> "CompileHashids"
    ==> "CompileTests"
    ==> "RunTests"

RunTargetOrDefault "RunTests"