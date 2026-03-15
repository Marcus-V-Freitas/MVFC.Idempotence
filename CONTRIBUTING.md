# Contributing to MVFC.SQLCraft

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) or later
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) running locally
- Git

## Running locally

```sh
git clone https://github.com/Marcus-V-Freitas/MVFC.Idempotence.git
cd MVFC.Idempotence
dotnet restore MVFC.Idempotence.slnx
dotnet build MVFC.Idempotence.slnx --configuration Release
```

## Running tests

The tests require Docker to be running.

```sh
dotnet test tests/MVFC.Idempotence.Tests/MVFC.Idempotence.Tests.csproj --configuration Release
```

## Adding a new helper

1. Create a new folder under `src/MVFC.Idempotence.{ServiceName}/`
2. Follow the structure of an existing helper (e.g. `MVFC.Idempotence.Mysql`)
3. Add the new project to `MVFC.Idempotence.slnx`
4. Add the package version to `Directory.Packages.props`
5. Add integration tests in `tests/MVFC.Idempotence.Tests/`
6. Update `README.md` and `README.pt-BR.md` with the new package entry

## Branch naming

- `feat/` — new feature or helper
- `fix/` — bug fix
- `chore/` — dependency update or maintenance
- `docs/` — documentation only
- `test/` — tests only
- `refactor/` — no feature change, no bug fix

Example: `feat/add-idempotence-helper`

## Commit convention

This project follows [Conventional Commits](https://www.conventionalcommits.org/):

- `feat: add idempotence helper`
- `fix: fix idempotence connection timeout`
- `docs: update README badges`
- `chore: bump SqlKata to 2.5.0`
- `test: add idempotence integration tests`
- `refactor: simplify idempotence commander setup`

## Pull Request process

1. Fork and create your branch from `main`
2. Make your changes and ensure all tests pass locally
3. Open a PR against `main` and fill in the PR template
4. Wait for the CI to pass
5. A maintainer will review and merge
