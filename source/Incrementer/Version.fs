module Incrementer.Version

open Incrementer.Process
open System
open System.Text.RegularExpressions

type IncrementMode =
    | SemanticVersioning
    | PatchPerCommit
    
type Parameters = {
    GitExecutablePath: string;
    GitRepositoryPath: string;
    Branch: string;
    IncrementMode: IncrementMode;
}

type SemVer = {
    Major: int;
    Minor: int;
    Patch: int;
}

let internal getRepositoryVersionUsingProcess (changeParameters : Parameters -> Parameters) (execProcess : Parameters -> ProcessOutput) =
    let defaultParameters = { GitExecutablePath = "git.exe"; Branch = "master"; GitRepositoryPath = "."; IncrementMode = SemanticVersioning }
    let parameters = changeParameters defaultParameters
    let output = execProcess parameters
    match output with
    | Error message ->
        failwith message
    | Success messages ->
        let extractVersionByTag (index : int) (message : string) =
            let matched = Regex.Match(message, @"\(.*?tag: [vV]?(?<major>[0-9]+)\.?(?<minor>[0-9]+)?\.?(?<patch>[0-9]+)?.*?\)")
            if matched.Success then
                let patchGroup = matched.Groups.["patch"]
                let patch = if patchGroup.Success then Int32.Parse(patchGroup.Value) else 0
                let minorGroup = matched.Groups.["minor"]
                let minor = if minorGroup.Success then Int32.Parse(minorGroup.Value) else 0
                let major = Int32.Parse(matched.Groups.["major"].Value)
                Some { Major = major; Minor = minor; Patch = patch + index; }
            else
                None
         
        let createInitialVersion messages =
            let patch = 
                match parameters.IncrementMode with
                | SemanticVersioning ->
                    0
                | PatchPerCommit ->
                    (Seq.length messages) - 1
            { Major = 1; Minor = 0; Patch = patch }
            
        let mostRecentTagVersion =
            messages
            |> Seq.mapi extractVersionByTag
            |> Seq.choose id
            |> Seq.tryHead
        
        match mostRecentTagVersion with
        | Some(v) -> v
        | _ -> createInitialVersion messages

let getRepositoryVersion changeParameters =
    let execGitProcess =
        fun parameters ->
            let processData = {
                Executable = parameters.GitExecutablePath;
                Args = sprintf "log %s --oneline --decorate" parameters.Branch;
                WorkingDirectory = parameters.GitRepositoryPath;
                Timeout = TimeSpan.FromSeconds(60.0);
            }
            Incrementer.Process.execute processData

    getRepositoryVersionUsingProcess changeParameters execGitProcess

let toSemVerString version =
    sprintf "%i.%i.%i" version.Major version.Minor version.Patch