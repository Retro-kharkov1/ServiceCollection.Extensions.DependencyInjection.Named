# Processes

<release>
1. Bump `<AssemblyVersion>` (and `<Version>` mirrors it via `$(AssemblyVersion)`) in the relevant
   `.csproj` — `DependencyInjection.Named/DependencyInjection.Named.csproj` and/or
   `DependencyInjection.Named.Refit/DependencyInjection.Named.Refit.csproj`.
2. `dotnet build` the `.sln` in Release config — `GeneratePackageOnBuild=True` produces the `.nupkg`
   next to each project's output.
3. Push the `.nupkg` to NuGet.org (no automated publish pipeline exists in this repo — manual
   `dotnet nuget push` today).
4. Tag the release in git (`git tag -a vX.Y.Z`) once published and verified installable.
</release>

<no_cicd>
There is no CI/CD pipeline in this repo as of 2026-09-14 — no GitHub Actions workflow, no Jenkins
job. Build/test/pack is run locally. If CI is added later, this file should be updated with the
actual workflow, and `ci-cd-engineer` should own it going forward.
</no_cicd>
