module Incrementer.Tests.GitTests

open Fake
open FsUnit
open NUnit.Framework
open System

type FakeVersion = Incrementer.Git.Version


let createMessages commits = fun _ ->
    let consoleMessages =
        commits
        |> Seq.map (fun c -> { IsError = false; Message = c; Timestamp = DateTimeOffset(DateTime.Now); })
    (true, consoleMessages)

let createParameters parameters =
    parameters

let shouldEqual (expected : FakeVersion) (actual : FakeVersion) =
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

    let version = Incrementer.Git.getRepositoryVersionUsingProcess createParameters getMessages

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

    let version = Incrementer.Git.getRepositoryVersionUsingProcess createParameters getMessages

    version |> shouldEqual { Major = 1; Minor = 2; Patch = 4; }