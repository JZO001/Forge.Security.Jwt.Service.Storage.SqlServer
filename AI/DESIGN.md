# Design

## The parts

| File / folder | Namespace | Role |
| --- | --- | --- |
| `SqlServerStorage.cs` | `Forge.Security.Jwt.Service.Storage.SqlServer` | `IStorage<JwtRefreshToken>` over the database |
| `SqlServerStorageOptions.cs` | `Forge.Security.Jwt.Service.Storage.SqlServer` | The options (the connection string) |
| `ServiceCollectionExtension.cs` | `Forge.Security.Jwt.Service.Storage.SqlServer` | `AddForgeJwtServiceSqlServerStorage(IServiceCollection, Action<SqlServerStorageOptions>)` |
| `Database/DatabaseContext.cs` | `….Database` | The EF Core `DbContext` |
| `Models/Token.cs` | `….Models` | The token entity |
| `Migrations/` | `….Migrations` | The EF Core migrations (`Initial`) and the model snapshot |

## The targets

| Target | Package references |
| --- | --- |
| `net8.0` | EF Core 8.0.0; `Microsoft.Data.SqlClient` 6.0.1; `Microsoft.Extensions.Caching.Memory` 8.0.1; `Microsoft.Extensions.DependencyInjection.Abstractions` 8.0.2; `System.Security.Cryptography.Pkcs` 8.0.2 |
| `net9.0` | EF Core 9.0.0; `Microsoft.Extensions.DependencyInjection.Abstractions` 9.0.0 |
| `net10.0` | EF Core 10.0.0; `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.0; `System.Security.Cryptography.Xml` 10.0.13; `System.Security.Cryptography.Pkcs` 10.0.13 |
| `net11.0` | EF Core 11.0.0; `Microsoft.Extensions.DependencyInjection.Abstractions` 11.0.0 (added on 2026-11-15; NuGet prunes it from the dependency group, as the .NET 11 shared framework provides it) |

`Forge.Security.Jwt.Shared` (project reference) and `Azure.Identity` 1.17.1 on every target. "EF Core" means
`Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.SqlServer`, with
`Microsoft.EntityFrameworkCore.Design` and `.Tools` of the same version as private assets (they do not reach the
consumers).

## The build and the package

- SDK: `Microsoft.NET.Sdk`. `SignAssembly` with `..\.Documents\Forge.snk`.
- `GenerateDocumentationFile`: each target writes its own `Forge.Security.Jwt.Service.Storage.SqlServer.xml` under
  `obj/<configuration>/<target>/`, and the build copies it next to the assembly (since 2026-11-15; before, one
  `DocumentationFile` in the project folder served every target).
- `GeneratePackageOnBuild` for Release; `PackageOutputPath` is `..\.Documents\`.
- The package holds `lib/<target>/Forge.Security.Jwt.Service.Storage.SqlServer.dll` and `.xml` for every target, and at
  its root `LICENSE` (the license is stated as `PackageLicenseExpression` `MIT` since 2026-11-15; the file is still
  packed), `README.md` (`PackageReadmeFile`) and `logo.png` (`PackageIcon`, from 2026-11-15). The `.runtimeconfig.json`,
  written for the EF tools, is kept out of the package by the `ExcludeRuntimeConfigFromPackage` target (since
  2026-11-15; before, it was packed into every `lib/<target>/`).
- `Migrations/.editorconfig` turns CS1591 off for the generated migrations (EF needs them public; they are not edited).
- The post-build copy (`xcopy` of the `.dll`, `.pdb`, `.xml` to `$(SolutionDir).Output\<target>\`) is commented out in
  the project file (Zoltán, 2026-11-15).
- The `Scc*` properties (`SAK`) and the `.vspscc` file belong to the TFS binding of the solution.

## Why each choice was made

- **One package version of the framework libraries per target:** the same pattern as in `Forge.Next.Shared`, where the
  reason is recorded: the package does not force a newer framework package on a consumer than its framework's own.
- **Strong naming:** the seven packages of the solution are signed with the same key, so the assemblies have a stable
  identity. Until 2026-11-15 the projects named the password-protected `Forge.pfx`, which only Visual Studio's MSBuild
  can sign with; since then its key pair as `Forge.snk`, which `dotnet build` can use too.
- **The extra references:** `System.Security.Cryptography.Xml` on `net10.0` was added on 2026-07-12 because of a
  vulnerability ("Upgrading a nuget package which has security issue"); on 2026-11-15, against the advisories found by
  the analysis, it went to 10.0.13 (the target's major version), and `System.Security.Cryptography.Pkcs` was added
  (10.0.13 on `net10.0`, 8.0.2 on `net8.0`; `HISTORY.md`). The direct references of `Azure.Identity`,
  `Microsoft.Data.SqlClient` and `Microsoft.Extensions.Caching.Memory` look like the same kind of lift of a transitive
  version; not confirmed.
- **`Microsoft.EntityFrameworkCore.Design`:** needed for the EF Core tools (migrations). Its
  `build/<target>/Microsoft.EntityFrameworkCore.Design.props` sets `GenerateRuntimeConfigurationFiles` to `True`
  (checked in 10.0.0), which is why a `.runtimeconfig.json` is written and packed (see `OPEN.md`).
