# Setup

<overview>
Open `ServiceCollection.Extensions.DependencyInjection.Named/ServiceCollection.Extensions.DependencyInjection.Named.sln`
in Visual Studio, or build/test from the CLI.
</overview>

<build>
```bash
dotnet build "ServiceCollection.Extensions.DependencyInjection.Named/ServiceCollection.Extensions.DependencyInjection.Named.sln"
dotnet test "ServiceCollection.Extensions.DependencyInjection.Named/DependencyInjection.Named.Test/DependencyInjection.Named.Test.csproj"
```
</build>

<targets>
- `DependencyInjection.Named` and `DependencyInjection.Named.Refit` multi-target `net461` and
  `netstandard2.1` — both must build clean; `netstandard2.1` is what most modern consumers pull.
- `DependencyInjection.Named.Test` targets `net8.0` (NUnit).
</targets>

<packaging>
Both library projects have `GeneratePackageOnBuild=True` — a local build produces a `.nupkg` per
project. `PackageId` is `ServiceCollection.Extensions.$(AssemblyName)`. Bump
`<AssemblyVersion>`/`<Version>` in the `.csproj` before a release build that should publish a new
package version.
</packaging>
