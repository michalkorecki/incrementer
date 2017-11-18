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


Target "RunTests" (fun _ ->
    !! (testDirectory + "/*Tests.dll")
        |> NUnit3 (fun nunitParams ->
            { nunitParams with
                ShadowCopy = false;
                ToolPath = "build\\tools\\NUnit.ConsoleRunner\\tools\\nunit3-console.exe";
                ResultSpecs = [ testDirectory + "/TestsResults.xml" ] })
)

Target "Clean" (fun _ ->
    CleanDirs [outputDirectory;testDirectory]
)

Target "Compile" (fun _ ->
    RestoreMSSolutionPackages (fun p -> { p with OutputPath = "./source/packages" }) "./source/Incrementer.sln"
    MSBuildRelease outputDirectory "Build" deployables |> Log "Compile: "
    MSBuildRelease testDirectory "Build" testables |> Log "Compile: "
)


"Clean"
  ==> "Compile"
  ==> "RunTests"


RunTargetOrDefault "RunTests"