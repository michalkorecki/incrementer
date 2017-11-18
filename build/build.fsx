#r "tools/FAKE/tools/FakeLib.dll"

open Fake
open Fake.FileSystem
open Fake.ProcessHelper
open Fake.RestorePackageHelper
open Fake.Testing.NUnit3
open System
open System.IO

let outputDirectory = "./build/output"
let testDirectory = "./build/tests"
let deployables = (!! "./source/**/*.?sproj") -- "./source/**/*Test*.?sproj"
let testables = !! "./source/**/*Test*.?sproj"


Target "Clean" (fun _ ->
    CleanDirs [outputDirectory;testDirectory]
)

Target "Compile" (fun _ ->
    RestoreMSSolutionPackages (fun p -> { p with OutputPath = "./source/packages" }) "./source/Incrementer.sln"
    MSBuildRelease outputDirectory "Build" deployables |> Log "Compile: "
    MSBuildRelease testDirectory "Build" testables |> Log "Compile: "
)

Target "RunTests" (fun _ ->
    !! (testDirectory + "/*Tests.dll")
        |> NUnit3 (fun nunitParams ->
            { nunitParams with
                ShadowCopy = false;
                ToolPath = "./build/tools/NUnit.ConsoleRunner/tools/nunit3-console.exe";
                ResultSpecs = [ testDirectory + "/TestsResults.xml" ] })
)

Target "Publish" (fun _ ->
    let key = getBuildParam "key"
    let version = getBuildParam "version"
    NuGetPack 
        (fun p ->
           { p with
               Version = version
               OutputPath = outputDirectory
               WorkingDir = outputDirectory
               ToolPath = "./build/nuget.exe" })
        "./source/Incrementer/Incrementer.fsproj"
    NuGetPublish
        (fun p ->
            { p with
               AccessKey = key
               Version = version
               OutputPath = outputDirectory
               WorkingDir = outputDirectory
               ToolPath = "./build/nuget.exe"
               Project = "Incrementer" })
)


"Clean"
  ==> "Compile"
  ==> "RunTests"
  ==> "Publish"


RunTargetOrDefault "RunTests"