# History and decisions

Newest last. Each entry: what was asked or decided, by whom, and why. The entries before 2026-11-15 are reconstructed
from the commit messages and the package files in `..\.Documents\`; a file's date there is when it was last written, not
necessarily when the version was published (see `OPEN.md`).

## 2022 – 2026-07: the versions

| Date | Commit message | Package files in `..\.Documents\` |
| --- | --- | --- |
| 2022-10-23 | Initial commit |  |
| 2022-10-24 | Initial commit, first version |  |
| 2022-11-21 | Maintenance update; Maintenance (11-25) |  |
| 2023-01-09 | Minor changes |  |
| 2023-05-16 | 1.2.7.0 |  |
| 2023-05-19 | 1.0.3 |  |
| 2023-05-25 | fixes | 1.0.4 |
| 2023-12-05 | Update Forge.Security.Jwt.Service.Storage.SqlServer.csproj | 1.0.5 |
| 2025-03-20 | v1.0.6 | 1.0.6 |
| 2025-09-23 | Version 1.0.6.1; Version 1.0.6.2 | 1.0.6.1, 1.0.6.2 |
| 2026-02-03 | 1.0.7.0 | 1.0.7 |
| 2026-02-13 | Maintenance | 1.0.8 |
| 2026-07-12 | v1.0.9; Upgrading a nuget package which has security issue | 1.0.9 |

## 2026-11-15 – The `net11.0` target

**Zoltán's change:** `net11.0` added to `TargetFrameworks`, in all seven packages of the solution. The version is not
raised.

The project was not built at the first analysis build (`Forge.Security.Jwt.Shared` failed on `net11.0`). With Zoltán's
`net11.0` reference in Shared, its own `net11.0` target fails: it has no EF Core reference (see `OPEN.md`).

## 2026-11-15 – The knowledge base

Created at Zoltán's request, from the knowledge base of `Forge.Next.Shared` (`CLAUDE.md`, `AI/`), adapted to this
solution. First planned once for the solution folder; Zoltán decided that each package gets its own, because each
package folder is a git repository of its own. The rules in `CLAUDE.md` were carried over and adapted, not decided one
by one (see `OPEN.md`).

## 2026-11-15 – The package icon: the Forge mark

At Zoltán's request, `logo.png` of `Forge.Next.Shared` is this package's icon: the Forge mark, a hexagon with a smaller,
filled hexagon inside, the two joined by three lines, on a dark rounded tile, 128 by 128 pixels. It sits in the
repository folder, like `LICENSE` and `README.md`, so that each git repository holds what its package needs;
`PackageIcon` names it, and it is packed to the package root. The published versions have no icon; the next version
carries it.

## 2026-11-15 – Packaging analysis

At Zoltán's request, the packaging of the solution was analysed, with `Forge.Next.Shared` as the model: the project
files, the package files in `..\.Documents\`, and builds of a copy of the solution (so that `..\.Documents\` and
`..\.Output\` stayed untouched). The findings for this package are in `OPEN.md`, the checks in `VERIFICATION.md`.
Nothing was fixed in the analysis itself, apart from the icon above; Zoltán's changes after it are recorded in this
file.

## 2026-11-15 – The vulnerable packages

The analysis found (`dotnet list package --vulnerable --include-transitive`): on `net10.0` the direct
`System.Security.Cryptography.Xml` 9.0.15, with eight high-severity advisories, and the transitive
`System.Security.Cryptography.Pkcs` 9.0.15; on `net8.0` the transitive `System.Security.Cryptography.Pkcs` 8.0.1. Zoltán
added patched versions the same day: `System.Security.Cryptography.Xml` 9.0.21 and `System.Security.Cryptography.Pkcs`
9.0.21 on `net10.0`, `System.Security.Cryptography.Pkcs` 8.0.2 on `net8.0`. The check reports no vulnerable package
since (`VERIFICATION.md`). Later the same day Zoltán moved `net10.0` to the 10.0.x packages, of the target's major
version as invariant 2 asks: `System.Security.Cryptography.Xml` and `System.Security.Cryptography.Pkcs` 10.0.13; still
no vulnerable package.

## 2026-11-15 – `net6.0` and `net7.0` dropped

**Zoltán's change,** after the analysis listed them as out of support: `net6.0` and `net7.0` left `TargetFrameworks`,
with their reference groups and `SupportedPlatform` conditions. The console harness `..\Testing.Security.Jwt` moved to
`net10.0`. A consumer on `net6.0` or `net7.0` cannot install the new version; it stays on an earlier one. A breaking
change for those consumers, so the version is raised by a minor step (below).

## 2026-11-15 – `net11.0`: EF Core 11

Zoltán added the `net11.0` group: EF Core 11.0.0 (`Microsoft.EntityFrameworkCore`, `.SqlServer`, and `.Design`, `.Tools`
as private assets) and `Microsoft.Extensions.DependencyInjection.Abstractions` 11.0.0. The target builds since. NuGet
prunes the DI reference from the `net11.0` dependency group of the package, because the .NET 11 shared framework
provides the assembly.

## 2026-11-15 – One XML documentation file per target

At Zoltán's request (a finding of the analysis), as in `Forge.Next.Shared` 3.3.2: `GenerateDocumentationFile` instead of
`DocumentationFile`. The `<None Remove="Forge.Security.Jwt.Service.Storage.SqlServer.xml" />` item and the `.gitignore`
entry (`# For Future`, `Forge.Security.Jwt.Service.Storage.SqlServer.xml`) were removed, and the file in the project
folder deleted. Each target writes its own file under `obj/<configuration>/<target>/`; the build copies it next to the
assembly, the post-build step to `..\.Output\<target>\`, and the pack takes it from there.

## 2026-11-15 – 1.0.9.0 → 1.1.0.0

At Zoltán's request. A minor version, because `net6.0` and `net7.0` were dropped; besides: `net11.0`, the icon, the XML
documentation per target, the cryptography packages. For publishing: the package depends on `Forge.Security.Jwt.Shared`
1.9.0, which is published before it.

## 2026-11-15 – The commit before the Release build

The analysis found that `Forge.Security.Jwt.Shared` 1.8.5 and `Forge.Security.Jwt.Service` 1.7.3 were built before their
commits, so their packages name an earlier commit. Zoltán: the published packages stay as they are; from now on the
change is committed before the Release build. A release step in `CLAUDE.md`.

## 2026-11-15 – `Forge.snk` instead of `Forge.pfx`

Zoltán decided to sign with a `.snk` exported from `Forge.pfx`, so that `dotnet build` and `dotnet pack` can build the
packages as well. `..\.Documents\Export-ForgeSnk.ps1` asks for the password of the `.pfx`, and writes
`..\.Documents\Forge.snk` only after checking that the key gives the public key of the signed assemblies (token
`97960309cfe8d317`). Found while testing it: .NET writes `CALG_RSA_KEYX` into the key blob whatever the `KeyNumber`, and
the public key, so the token, would then differ; the script writes `CALG_RSA_SIGN`, as `sn.exe` does. The `.snk` has no
password: whoever has it can sign as Forge. Zoltán ran it the same day. The `.snk` holds a 1024-bit key with
`CALG_RSA_SIGN`, its public key is that of the signed assemblies (token `97960309cfe8d317`), and its private key signs.
All seven projects now name `..\.Documents\Forge.snk` in `AssemblyOriginatorKeyFile`; this also put right the three
projects (`Forge.Security.Jwt.Service.Storage.SqlServer`, `Forge.Wasm.BrowserStorages`,
`Forge.Wasm.BrowserStorages.NewtonSoft.Json`) that named `..\..\.Documents\Forge.pfx`, the `.Documents` folder above the
solution folder. `dotnet build` builds the solution and its packages since, with the same public key token
(`VERIFICATION.md`). `Forge.pfx` stays in `..\.Documents\`; no project names it.

## 2026-11-15 – 1.1.0 → 1.1.1

Zoltán raised the version once more the same day (a patch step); the reason is not recorded. Neither version is
published: on nuget.org 1.0.9 is the latest (checked on 2026-11-15).

## 2026-11-15 – The other findings fixed

Zoltán commented out the post-build `xcopy` target in all seven projects, so a project builds alone as well. Then, at
Zoltán's request, the other findings:
- in this project: `Migrations/.editorconfig` turns CS1591 off for the generated migrations, which stay untouched; the
  `ExcludeRuntimeConfigFromPackage` target keeps the `.runtimeconfig.json` out of the package (it is still written, for
  the EF tools); the README: `dotnet add package …`, and the `IConfiguration` argument of
  `AddForgeJwtServerAuthenticationCore` in the setup example (the analysis had listed this one under
  `Forge.Security.Jwt.Service` by mistake).
- in all seven: the license is stated as `PackageLicenseExpression` `MIT` instead of `PackageLicenseFile`; the `LICENSE`
  file is still packed, since the MIT license asks for the notice to go with the copies.

The seven projects build with no warning since (the console harness `..\Testing.Security.Jwt` has eight of its own). Not
changed: a Release build still writes the package of the current version into `..\.Documents\` (`OPEN.md`).
