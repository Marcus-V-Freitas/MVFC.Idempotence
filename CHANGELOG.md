# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
  https://github.com/Marcus-V-Freitas/MVFC.Idempotence/commit/805e0ddd186a2bf23411fa0d9c46af409d50b2a9

## [1.0.0] - 2026-02-21

### Added
- Initial release of `MVFC.Idempotence` library.
- Middleware and services for idempotency in ASP.NET Core APIs.
- Support for distributed cache and Redis integration.
- Configurable header names and allowed HTTP methods. (commit de75b7a)  
  https://github.com/Marcus-V-Freitas/MVFC.Idempotence/commit/de75b7ad853296230f81232c028e1215ba056805d
