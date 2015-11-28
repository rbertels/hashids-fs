// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile

RestorePackages()

let version = "1.0.0.0"
let buildDir  = @".\build\"
let testDir   = @".\test\"
let nugetDir = @".\nuget\"

Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; nugetDir]
)

Target "BuildAssemblyInfo" (fun _ ->
    CreateFSharpAssemblyInfo @"Hashids\AssemblyInfo.fs"
        [ Attribute.Title "Hashids-fs" 
          Attribute.Product "Hashids-fs"
          Attribute.Version version
          Attribute.FileVersion version ]
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

Target "BuildPackage" (fun _ ->
    XCopy buildDir (nugetDir + @".\lib")

    @"Hashids\Hashids-fs.nuspec"
    |> NuGet (fun p ->
        { p with
            Authors = [ "Rob Bertels" ]
            Version = version
            Project = "Hashids-fs"
            Description = "Generate short unique ids from integers."
            NoPackageAnalysis = true
            OutputPath = nugetDir})
)

"Clean"
    ==> "BuildAssemblyInfo"
    ==> "CompileHashids"
    ==> "CompileTests"
    ==> "RunTests"
    ==> "BuildPackage"

RunTargetOrDefault "BuildPackage"