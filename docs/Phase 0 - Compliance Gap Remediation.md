# Phase 0 - Compliance Gap Remediation

**Date:** 2026-08-24

A compliance audit against this user's standing personal conventions found that Teron Addon
Manager had never received a `/docs` folder or the build-tooling/runtime-safety pieces of a full
compliance pass, despite its naming, license, and display-name work already being done in earlier
history. This is the first entry in this app's `/docs`, covering five real gaps found and fixed in
one pass.

## Findings and fixes

- **`LangVersion` was never set.** `TeronAddonManager.csproj` had `<Nullable>enable</Nullable>`
  and `<ImplicitUsings>enable</ImplicitUsings>` but no `<LangVersion>`, unlike this user's other
  apps. Added `<LangVersion>latest</LangVersion>` to the same `PropertyGroup`.
- **A stray fossil `.user` file was left on disk.** `Teron_Addon_Manager.csproj.user` (the old
  underscored project name) sat alongside the correct `TeronAddonManager.csproj.user`. Both are
  untracked/gitignored (`*.user`), but the fossil is confusing clutter from before the project was
  renamed. Deleted it; the correctly-named file was left untouched.
- **No `Directory.Build.props` (BUILDER convention).** Unlike this user's other apps, this project
  built directly into `bin\`/`obj\` at the project root instead of a consolidated `Build\` tree.
  Added `Directory.Build.props` at the repo root, redirecting `OutputPath` to
  `Build\$(Configuration)\$(MSBuildProjectName)\`, `BaseIntermediateOutputPath` to
  `Build\obj\$(MSBuildProjectName)\`, and a generic RID-based `PublishDir` under
  `Build\Publish\$(MSBuildProjectName)\$(RuntimeIdentifier)\` (used only for plain `dotnet build`,
  not the real publish path - see below). Added `/Build/` to `.gitignore` next to the existing
  `bin`/`obj` entries.
  - Because this app has a DISSEMINATE installer pipeline
    (`Properties/PublishProfiles/*.pubxml` + `Installer/TeronAddonManager.iss`), each `.pubxml`'s
    own `<PublishDir>` was updated to an explicit path
    (`Build\Publish\TeronAddonManager\win-x64\` / `win-x86\`) so the profile's explicit per-RID
    path wins over `Directory.Build.props`'s generic formula, which collapses `$(RuntimeIdentifier)`
    to empty for a plain `dotnet build` and would otherwise produce the wrong directory for a real
    publish.
  - Hit the known WPF gotcha building Debug then Release back-to-back: bogus `CS0102`/`CS0111`
    "already defines" duplicate-member errors from a stale `Build\obj\`, because
    `BaseIntermediateOutputPath` isn't configuration-specific. Deleted `Build\` and rebuilt clean
    each time rather than treating it as a real bug - both configurations then built with 0
    errors.
- **Hardcoded `<Version>2.8.0</Version>` with no build-number tracking (CODEX convention).**
  Replaced the single hardcoded `<Version>` with `<MajorMinorPatchVersion>2.8.1</MajorMinorPatchVersion>`
  plus a `BuildNumberFile` (`BuildNumber.txt`, tracked in git starting at `0`) and an
  `IncrementBuildNumber` target (`BeforeTargets="BeforeBuild"`) that reads the last build number,
  adds one, writes it back, and derives `AssemblyVersion`/`FileVersion`/`Version` as
  `$(MajorMinorPatchVersion).$(BuildNumber)`. Synced the version bump (hotfix: build-tooling work,
  not a user-facing feature) to `Installer/TeronAddonManager.iss`'s `MyAppVersion` fallback and to
  `CHANGELOG.md`.
- **Missing `AppDomain.CurrentDomain.UnhandledException` hook.** `App.xaml.cs` wired
  `DispatcherUnhandledException` and `TaskScheduler.UnobservedTaskException` to `error.log`, but a
  fatal exception on a non-UI, non-`Task` thread (a raw `Thread`, a finalizer, etc.) would have
  gone unlogged. Added the missing hook alongside the existing two, matching their exact style
  (inline lambda calling the same `LogException` helper, which is already wrapped in its own
  try/catch so a logging failure can never itself throw).

## Version

Bumped `2.8.0` -> `2.8.1` (hotfix - build-tooling/identity work, no new user-facing capability),
synced across `TeronAddonManager.csproj`'s `MajorMinorPatchVersion`,
`Installer/TeronAddonManager.iss`'s `MyAppVersion` fallback, and `CHANGELOG.md`.

## Verification

`dotnet build TeronAddonManager.csproj -c Debug` and `-c Release` - both 0 warnings, 0 errors
(clean rebuild after the stale-`obj` gotcha above), output correctly lands under
`Build\Debug\TeronAddonManager\` and `Build\Release\TeronAddonManager\` respectively (no root
`bin\`/`obj\` recreated). `dotnet publish` and the installer build itself were intentionally not
run - out of scope for this pass.
