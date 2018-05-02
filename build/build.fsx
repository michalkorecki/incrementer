#r "tools/FAKE/tools/FakeLib.dll"
#r "tools/Incrementer/lib/net452/Incrementer.dll"

open Fake
open Fake.FileSystem
open Fake.ProcessHelper
open Fake.RestorePackageHelper
open Fake.Testing.NUnit3
open Incrementer
open System
open System.IO

let outputDirectory = "./build/output"
let packageDirectory = "./build/package"
let testDirectory = "./build/tests"
let deployables = (!! "./source/**/*.?sproj") -- "./source/**/*Test*.?sproj"
let testables = !! "./source/**/*Test*.?sproj"


Target "Clean" (fun _ ->
    CleanDirs [outputDirectory;testDirectory;packageDirectory]
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
    let semVer = Incrementer.Version.getRepositoryVersion id
    let version = Incrementer.Version.toSemVerString semVer

    NuGetPack 
        (fun p ->
           { p with
               Version = version
               OutputPath = packageDirectory
               WorkingDir = outputDirectory
               ToolPath = "./build/nuget.exe" })
        "./source/Incrementer/Incrementer.nuspec"
    NuGetPublish
        (fun p ->
            { p with
               AccessKey = key
               Version = version
               OutputPath = packageDirectory
               WorkingDir = packageDirectory
               ToolPath = "./build/nuget.exe"
               Project = "Incrementer" })
)

Target "ShowVersion" (fun _ ->
    let semVer = Incrementer.Version.getRepositoryVersion id
    let version = Incrementer.Version.toSemVerString semVer
    printfn "Version: %s" version
)


"Clean"
  ==> "Compile"
  ==> "RunTests"
  ==> "Publish"


RunTargetOrDefault "RunTests"