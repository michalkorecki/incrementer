module Incrementer.Git

open Fake
open ProcessHelper
open System
open System.Text.RegularExpressions

// Tests
// several tags
// no tags
// no commits
// execproc fail

type Parameters = {
    GitExecutablePath: string;
    GitRepositoryPath: string;
    Branch: string;
}

type Version = {
    Major: int;
    Minor: int;
    Patch: int;
}

let getRepositoryVersionUsingProcess (changeParameters : Parameters -> Parameters) (execProcess : Parameters -> bool * seq<ConsoleMessage>) =
    let defaultParameters = { GitExecutablePath = "git.exe"; Branch = "master"; GitRepositoryPath = "." }
    let parameters = changeParameters defaultParameters
    let status, messages = execProcess parameters
    let extractVersionByTag (index : int) (consoleMessage : ConsoleMessage) =
        let matched = Regex.Match(consoleMessage.Message, @"\(tag: [vV]?(?<major>[0-9]+)\.?(?<minor>[0-9]+)?\)")
        if matched.Success then
            let minorGroup = matched.Groups.["minor"]
            let minor = if minorGroup.Success then Int32.Parse(minorGroup.Value) else 0
            let major = Int32.Parse(matched.Groups.["major"].Value)
            Some { Major = major; Minor = minor; Patch = index; }
        else
            None
            
    let mostRecentTagVersion =
        messages
        |> Seq.mapi extractVersionByTag
        |> Seq.choose id
        |> Seq.tryHead
        
    match mostRecentTagVersion with
    | Some(v) -> v
    | _ -> { Major = 1; Minor = 0; Patch = (Seq.length messages); }

let getRepositoryVersion changeParameters =
    let execGitProcess =
        fun parameters -> ExecProcessRedirected (fun info ->
            info.FileName <- parameters.GitExecutablePath
            info.WorkingDirectory <- parameters.GitRepositoryPath
            info.Arguments <- sprintf "log %s --oneline --decorate" parameters.Branch
            info.CreateNoWindow <- true) (TimeSpan.FromSeconds 30.0)
    getRepositoryVersionUsingProcess changeParameters execGitProcess
