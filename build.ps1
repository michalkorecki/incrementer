If (-not (Test-Path ".\build\tools\FAKE\tools\FAKE.exe"))
{
	.\build\nuget.exe install FAKE -OutputDirectory build\tools -ExcludeVersion
}

If (-not (Test-Path ".\build\tools\Incrementer\lib\net452\Incrementer.dll"))
{
	.\build\nuget.exe install Incrementer -OutputDirectory build\tools -ExcludeVersion
}

If (-not (Test-Path "build\tools\NUnit.ConsoleRunner\tools\nunit3-console.exe"))
{
	.\build\nuget.exe install NUnit.ConsoleRunner -OutputDirectory build\tools -ExcludeVersion
}

If ([string]::IsNullOrEmpty($args))
{
    .\build\tools\FAKE\tools\FAKE.exe build\build.fsx --listTargets
}
Else
{
    .\build\tools\FAKE\tools\FAKE.exe build\build.fsx @args
}
