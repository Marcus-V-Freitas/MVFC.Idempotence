# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-11-12

### Added

- Added multi-targeting support for `net10.0` and `net11.0` in `MVFC.Idempotence`.
- Added .NET 11 SDK setup in GitHub Actions CI/CD workflows (`ci.yml`).
- Added framework-conditional package versioning for `Microsoft.Extensions.Caching.*` in `Directory.Packages.props`.

### Changed

- Migrated test and playground projects (`MVFC.Idempotence.Tests`, `MVFC.Idempotence.Playground.Api`, `MVFC.Idempotence.Playground.AppHost`) to target .NET 11 (`net11.0`).
- Updated `global.json` to allow prerelease SDKs and configure `rollForward` to `latestMajor`.
- Updated dependencies in `Directory.Packages.props`:
  - `Aspire.Hosting.*` to `13.6.5`
  - `MVFC.Aspire.Helpers.Redis` to `11.0.0`
  - `xunit.v3` to `4.0.3`
  - `Microsoft.NET.Test.Sdk` to `18.12.0`
- Updated README badges and documentation to reflect .NET 11 support.

## [1.2.5] - 2026-11-08

### Added

- Pinned .NET SDK version `10.0.100` with `latestFeature` roll-forward in `global.json`.
- Added `dotnet-coverage` and `dotnet-reportgenerator-globaltool` as local dotnet tools in `.config/dotnet-tools.json`.

### Changed

- Updated `build.cake` to run `dotnet-coverage` and `reportgenerator` via `dotnet tool run` instead of local ad-hoc installations.
- Overhauled GitHub Actions CI workflow with concurrency controls, .NET 9 & 10 SDK setup, tool restoration, and refined coverage publishing.
- Updated `MVFC.Pack.Analyzers` package to version `4.0.2` and refined test project exclusion logic in `Directory.Build.targets`.
- Updated CI/Codecov badge links in README documentation.

## [1.2.4] - 2026-10-31

### Changed

- Downgraded `Microsoft.Extensions.Caching.StackExchangeRedis` from prerelease `11.0.0-rc.3.26522.148` to stable `10.0.14`.

## [1.2.3] - 2026-10-31

### Added

- Added `global.json` configuring `Microsoft.Testing.Platform` as the test runner.
- Integrated `MVFC.Pack.Analyzers` in `Directory.Build.targets` to standardize code analysis across projects.

### Changed

- Replaced direct `SonarAnalyzer.CSharp` reference with `MVFC.Pack.Analyzers` package in central package management.
- Configured `.editorconfig` diagnostic suppressions (`MA0048` and `RCS1194`).
- Added `.ConfigureAwait(false)` in `IdempotencyService` operation callbacks.
- Enabled `AspireUseCliBundle` in `MVFC.Idempotence.Playground.AppHost`.
- Simplified test execution arguments in `build.cake`.

## [1.2.2] - 2026-10-31

### Changed

- Updated NuGet package dependencies across the solution.
- Enabled `CentralPackageTransitivePinningEnabled` in `Directory.Packages.props`.
- Updated Aspire AppHost SDK to version `13.6.0`.
- Configured asset access (`PrivateAssets` and `IncludeAssets`) for test runner and collector packages in `MVFC.Idempotence.Tests`.

## [1.2.1] - 2026-06-26

### Changed

- Updated NuGet package dependencies.

## [1.2.0] - 2026-04-03

### Added

- Integrated **MinVer** for automated, tag-based versioning.

### Changed

- Improved NuGet package generation and publishing workflow in CI/CD pipeline.
- Enhanced code quality by enforcing stricter rules via Roslyn Analyzers and SonarAnalyzer.
- Suppressed `CA2012` ("Use ValueTasks correctly") globally via `.editorconfig` to support standard mocking patterns.
- Refactored tests to use more specific exception types (e.g., `InvalidOperationException` instead of `Exception`) to satisfy analyzer rule `CA2201`.

## [1.1.3] - 2026-03-21

### Changed

- CI/CD workflow refinements for automated publishing and coverage reporting
- Minor adjustments to Codecov configuration for status checks precision

## [1.1.2] - 2026-03-14

### Added

- NuGet Downloads badge to English and Portuguese README files.
- Direct links to `CHANGELOG.md` in all documentation.
- Automated `CHANGELOG.md` link inclusion in GitHub Release notes via CI workflow.

### Changed

- Standardized Changelog reference across all files.

## [1.1.1] - 2026-03-14

### Added

- Portuguese translation for README (`README.pt-br.md`).

### Changed

- Improved library performance by adding `.ConfigureAwait(false)` to all asynchronous calls.
- Refactored private field naming to follow standard conventions (underscore prefix).
- Cleaned up CI workflow by removing .NET 9 (focusing on .NET 10).
- Explicitly marked interface methods as `public` in `IIdempotencyService`.

## [1.1.0] - 2026-03-14

### Added

- Achieved 100% line and branch code coverage across the entire project.
- Integrated `dotnet-coverage` into the `build.cake` script to collect coverage from all processes, including Aspire-hosted APIs.
- Comprehensive unit tests for `IdempotencyFilter`, `IdempotencyService`, extension methods, and models.
- Added `TestHelper` utility to centralize test setup and `HttpContext` mocking logic.

### Changed

- Refactored the entire test suite to use `FluentAssertions` for better readability and more descriptive messages.
- Centralized `using` directives into a global `Usings.cs` file.
- Improved `IdempotencyService` to handle `byte[]` payloads and custom TTL logic more robustly.

### Fixed

- Resolved issues with coverage collection in multi-process scenarios (Aspire integration tests).
- Suppressed `CA2012` warnings in tests to allow for standard mocking patterns with `ValueTask`.

## [1.0.1] - 2026-02-21

### Changed

- Improved internal project configuration and assembly metadata. (commit 805e0dd)  
  <https://github.com/Marcus-V-Freitas/MVFC.Idempotence/commit/805e0ddd186a2bf23411fa0d9c46af409d50b2a9>

## [1.0.0] - 2026-02-21

### Added

- Initial release of `MVFC.Idempotence` library.
- Middleware and services for idempotency in ASP.NET Core APIs.
- Support for distributed cache and Redis integration.
- Configurable header names and allowed HTTP methods. (commit de75b7a)  
  <https://github.com/Marcus-V-Freitas/MVFC.Idempotence/commit/de75b7ad853296230f81232c028e1215ba056805d>

[2.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.2.5...v2.0.0
[1.2.5]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.2.4...v1.2.5
[1.2.4]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.2.3...v1.2.4
[1.2.3]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.2.2...v1.2.3
[1.2.2]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.2.1...v1.2.2
[1.2.1]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.1.3...v1.2.0
[1.1.3]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.1.2...v1.1.3
[1.1.2]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.1.1...v1.1.2
[1.1.1]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Idempotence/releases/tag/v1.0.0
