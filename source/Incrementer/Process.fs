module internal Incrementer.Process

type ProcessOutput =
    | Success of string[]
    | Error of string

type ProcessData = {
    Executable : string;
    Args : string;
    WorkingDirectory : string;
    Timeout : System.TimeSpan;
}

let execute data =
    try
        let pi = new System.Diagnostics.ProcessStartInfo()
        pi.FileName <- data.Executable
        pi.Arguments <- data.Args
        pi.WorkingDirectory <- data.WorkingDirectory
        pi.CreateNoWindow <- true
        pi.UseShellExecute <- false
        pi.RedirectStandardOutput <- true
        pi.RedirectStandardError <- true

        let output = new System.Collections.Generic.List<string>()
        let error = new System.Collections.Generic.List<string>()
        let p = new System.Diagnostics.Process()
        p.StartInfo <- pi
        p.ErrorDataReceived.Add(fun d -> if d.Data <> null then error.Add(d.Data))
        p.OutputDataReceived.Add(fun d -> if d.Data <> null then output.Add(d.Data))
        p.Start() |> ignore

        p.BeginErrorReadLine()
        p.BeginOutputReadLine()

        if not <| p.WaitForExit(int data.Timeout.TotalMilliseconds) then
            try
                p.Kill()
            with ex ->
                failwithf "Could not kill process %s %s after timeout." p.StartInfo.FileName p.StartInfo.Arguments
            failwithf "Process %s %s timed out." p.StartInfo.FileName p.StartInfo.Arguments
        else
            // WaitForExit must be called twice due to synchronization issues
            // https://stackoverflow.com/a/16095658/1149924
            // https://msdn.microsoft.com/en-us/library/ty0d8k56.aspx
            p.WaitForExit()

            if error.Count > 0 then
                Error error.[0]
            else
                Success <| Seq.toArray output
    with
    | ex ->
        Error ex.Message