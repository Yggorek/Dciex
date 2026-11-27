# Contributing to Dciex

Thank you for considering a contribution to Dciex. This guide explains how to set up the repository, work with its projects, and submit a pull request. For substantial changes, please open an Issue first to discuss the proposal.

## Development Requirements

- .NET 10 SDK
- Git
- Docker Engine and Docker Compose for checking scenarios that need a real Docker environment
- An IDE of your choice is recommended

## Getting Started

1. Fork the Dciex repository on GitHub.
2. Clone your fork and enter the repository directory:

   ```sh
   git clone https://github.com/<your-username>/Dciex.git
   cd Dciex
   ```

3. Restore dependencies:

   ```sh
   dotnet restore Dciex.sln
   ```

4. Build the solution:

   ```sh
   dotnet build Dciex.sln --configuration Release
   ```

## Running Tests

Tests should pass before you submit a pull request. Run the test suite with:

```sh
dotnet test Dciex.sln --configuration Release
```

Add or update tests to cover new functionality and bug fixes. When relevant, check behavior on both Windows and Linux. The test suite includes a FakeDocker helper, so tests do not all require a running Docker Engine. Use Docker Engine and Compose when validating behavior that depends on a real Docker environment. If you could not run tests, explain why in your pull request.

## Architecture Guidelines

The solution is organized into these projects:

- **Dciex.Core** — core models and entities; it has no project dependencies.
- **Dciex.Application** — application logic and contracts; it depends on Core.
- **Dciex.Infrastructure** — infrastructure implementations; it depends on Application and Core.
- **Dciex.CLI** — entry point, commands, and dependency setup; it connects Application and Infrastructure.
- **Dciex.Tests** — automated tests for the application, core, and infrastructure projects.

Keep dependencies flowing toward the core: Core must not depend on Infrastructure. Keep business logic in the appropriate application or core code rather than in CLI commands, and route Docker work through the existing application contracts and infrastructure implementations instead of controlling Docker directly from the CLI. Use the existing abstractions to isolate file-system, Docker, and process operations where appropriate; add interfaces or abstraction layers only when they solve a real need. Preserve support for Windows and Linux.

## Code Guidelines

- Follow the conventions already used in the relevant project.
- Keep Nullable Reference Types in mind; they are enabled across the solution projects.
- Prefer asynchronous operations for I/O.
- Handle errors and pass `CancellationToken` through operations where appropriate.
- Avoid dependencies that are not needed, and keep implementations readable and straightforward.

## Branching and Commits

The following branch names are recommended:

- `feature/<name>` for new functionality
- `fix/<name>` for bug fixes
- `docs/<name>` for documentation
- `refactor/<name>` for refactoring

Examples: `feature/config-validation`, `fix/compose-discovery`, `docs/contributing-guide`, and `refactor/project-registry`.

Use [Conventional Commits](https://www.conventionalcommits.org/) for commit messages. Common types include:

- `feat:` for a new feature, such as `feat: add environment validation`
- `fix:` for a bug fix, such as `fix: handle missing compose file`
- `docs:` for documentation, such as `docs: add contribution guidelines`
- `refactor:` for code changes that do not add a feature or fix a bug, such as `refactor: simplify project registry`
- `test:` for test changes, such as `test: add compose discovery tests`

## Pull Requests

Before opening a pull request:

- Confirm the solution builds and run the relevant tests.
- Update documentation when changes affect user-facing behavior.
- Check that no secrets or sensitive data are included.
- Describe the changes, tests performed, and any potential breaking changes.
- Use the repository's [Pull Request Template](.github/PULL_REQUEST_TEMPLATE.md).

## General Principles

- Prefer small, focused changes; avoid combining unrelated fixes and features in one pull request.
- Discuss large changes before starting implementation.
- Treat other project contributors with respect.
