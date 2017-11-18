module Incrementer.Tests.VersionTests

open Fake
open Incrementer
open FsUnit
open NUnit.Framework
open System


let createMessages commits = fun _ ->
    let consoleMessages =
        commits
        |> Seq.map (fun c -> { IsError = false; Message = c; Timestamp = DateTimeOffset(DateTime.Now); })
    (true, consoleMessages)

let createParameters parameters =
    parameters

let shouldEqual (expected : Incrementer.Version.SemVer) (actual : Incrementer.Version.SemVer) =
    actual.Major |> should equal expected.Major
    actual.Minor |> should equal expected.Minor
    actual.Patch |> should equal expected.Patch

[<Test>]
let ``Version is equal to number of commits when there is no tag in repo commit history`` () =
    let getMessages =
        createMessages [
            "44d1fca (HEAD -> master) Setup FAKE and build script.";
            "1e90a42 (origin/master, origin/HEAD) Add linqpad POC.";
            "ca3cf48 Initial commit";
        ]

    let version = Incrementer.Version.getRepositoryVersionUsingProcess createParameters getMessages

    version |> shouldEqual { Major = 1; Minor = 0; Patch = 3; }

[<Test>]
let ``Version is equal to number of commits after most recent tag`` () =
    let getMessages =
        createMessages [
            "4510e19 Adjust package restore.";
            "be419bf Remove process-killing from build.";
            "67d55d6 Adjust binaries names.";
            "9dc43df Fix config paths.";
            "da24eba (tag: v1.2) Add more clear status messages, notify when";
            "87e123c Adjust build to include fsharp projects.";
            "aa1ccd3 Merge branch FSharp. Resolves #37, resolves #40.";
            "0a4bef9 Fix a bug causing program to crash when existing ticker";
            "d85d467 (tag: v1.1) Adjust build scripts for SQLite deployment.";
        ]

    let version = Incrementer.Version.getRepositoryVersionUsingProcess createParameters getMessages

    version |> shouldEqual { Major = 1; Minor = 2; Patch = 4; }

[<Test>]
let ``Version patch component is incremented when most recent tag contains patch number (issue #2)`` () =
    let getMessages =
        createMessages [
            "be419bf Remove process-killing from build.";
            "67d55d6 Adjust binaries names.";
            "9dc43df Fix config paths.";
            "da24eba (tag: v2.4.29) Add more clear status messages.";
            "87e123c Adjust build to include fsharp projects.";
        ]

    let version = Incrementer.Version.getRepositoryVersionUsingProcess createParameters getMessages

    version |> shouldEqual { Major = 2; Minor = 4; Patch = 32; }

[<Test>]
let ``Version is extracted correctly from tag created at the same commit as branch head`` () =
    let getMessages =
        createMessages [
            "6f63314 (HEAD -> master) Include Incrementer package for easier versioning.";
            "eb43498 (tag: v1.3, origin/master, origin/HEAD) Increment version.";
            "7938560 Remove read rows limit for commodities provider.";
            "bcf6d73 Increment version to 1.2.7.0";
        ]
    
    let version = Incrementer.Version.getRepositoryVersionUsingProcess createParameters getMessages
    
    version |> shouldEqual { Major = 1; Minor = 3; Patch = 1; }

[<Test>]
let ``Version is extracted correctly from tag created at local HEAD (issue #4)`` () =
    let getMessages =
        createMessages [
            "6f63314 (HEAD -> master, tag: 1.3) Include Incrementer package for easier versioning.";
            "7938560 Remove read rows limit for commodities provider.";
            "bcf6d73 Increment version to 1.2.7.0";
        ]

    let version = Incrementer.Version.getRepositoryVersionUsingProcess createParameters getMessages

    version |> shouldEqual { Major = 1; Minor = 3; Patch = 0 }

[<Test>]
let ``Version can be converted to sem ver string`` () =
    let semVer = Incrementer.Version.toSemVerString { Major = 2; Minor = 5; Patch = 17 }

    semVer |> should equal "2.5.17"
