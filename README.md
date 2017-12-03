# incrementer

[![NuGet](https://badge.fury.io/nu/incrementer.svg)](https://www.nuget.org/packages/Incrementer)

Incrementer is a simple library to ease incremental NuGet packages development. The idea behind is to use git commit log to "deduce" current package version.

### installation

Incrementer is available on [NuGet](https://www.nuget.org/packages/Incrementer). Install it using:

```
Install-Package Incrementer
```


### behavior

Incrementer uses git tags to track most recent version and then appends number of commits since last tag to increment *patch* part of version. When your git log looks like this:

```
4510e19 Adjust package restore
be419bf Remove process-killing from build
67d55d6 Adjust binaries names
9dc43df Fix config paths
da24eba (tag: v1.2) Add more clear status messages.
87e123c Adjust build to include fsharp projects
aa1ccd3 Merge branch FSharp. Resolves #37, resolves #40
d85d467 (tag: v1.1) Adjust build scripts for SQLite deployment
```

The version returned by Incrementer will be `1.2.4`.

### usage (F#)

When using [FAKE](https://fake.build/), your publish target might use Incrementer in the following fashion:

```fsharp
Target "Publish" (fun _ ->
    let semVer = Incrementer.Version.getRepositoryVersion id
    let semVerString = Incrementer.Version.toSemVerString semVer

    NuGetPack 
        (fun p ->
           { p with
                    Version = semVerString
                    OutputPath = "D:/local-nuget-repo/packages"
                    WorkingDir = "./build/output"
                    ToolPath = "./build/nuget.exe" })
        "Incrementer.fsproj"
)
```

It assumes the following defaults:

 * there is `git.exe` executable
 * there is `master` branch
 * git repository in question is *here* (current directory)

When you are using different setup defaults are easily changeable:

```fsharp
Target "Publish" (fun _ ->
    let changeDefaults = fun p -> { p with GitExecutablePath = "D:/i-hide-git/git.exe" }
    let semVer = Incrementer.Version.getRepositoryVersion changeDefaults
    let semVerString = Incrementer.Version.toSemVerString semVer

    // ...
)
```


### usage (C#)

Interop with C# build system like [Cake](https://cakebuild.net/) is also possible:

```csharp
#addin nuget:?package=Incrementer
#addin nuget:?package=FSharp.Core

#reference "build/Addins/incrementer/Incrementer/lib/net452/Incrementer.dll"
#reference "build/Addins/fsharp.core/FSharp.Core/lib/net45/FSharp.Core.dll"

Task("Publish")
	.Does(() => 
	{
		var id = Microsoft.FSharp.Core.FuncConvert.ToFSharpFunc(
			new Converter<Incrementer.Version.Parameters, Incrementer.Version.Parameters>(p => p));
		var semVer = Incrementer.Version.getRepositoryVersion(id);
		var semVerString = Incrementer.Version.toSemVerString(semVer);

		Console.WriteLine(semVerString);
	});
```

FSharp.Core package is required for interop between C# and F#.