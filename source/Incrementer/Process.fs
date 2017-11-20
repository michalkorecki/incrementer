module internal Incrementer.Process

type ProcessOutput =
    | Success of string[]
    | Error of string

type ProcessData = {
    Executable : string;
    Args : string;
    WorkingDirectory : string;
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

        let p = new System.Diagnostics.Process()
        p.StartInfo <- pi
        p.Start() |> ignore
        
        let rec read (output : System.IO.StreamReader) (result : list<string>) =
            if output.EndOfStream then
                result
            else
                let line = output.ReadLine()
                read output (line::result)
        
        let output = read p.StandardOutput []
        let error = read p.StandardError []
        
        if error.Length > 0 then
            Error error.[0]
        else
            Success <| Seq.toArray output
    with
    | ex ->
        Error ex.Message