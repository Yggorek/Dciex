<div align="center">
  <h1>Dciex</h1>
  <p><strong>A cross-platform CLI for managing local development environments.</strong></p>
  <p>
    <a href="README.md">🇬🇧 English</a> ·
    <a href="README.ru.md">🇷🇺 Русский</a> ·
    <a href="README.es.md">🇪🇸 Español</a>
  </p>
</div>

> **Project status:** Under active development. The project has not reached its first stable release yet.

Dciex is an open-source command-line tool that aims to simplify the setup and management of local development environments. It brings project registration, YAML-based environment configuration, Docker Compose discovery and generation, and environment startup into one workflow.

## Current capabilities

- **Project registry** — register, list, inspect, and unregister projects.
- **YAML configuration** — load and validate `devenv.yaml` files.
- **Compose generation** — generate a `compose.yaml` file from the Dciex environment configuration.
- **Existing Compose discovery** — find an existing Docker Compose file in a project directory.
- **Environment startup** — orchestrate configuration loading, Compose file selection or generation, and Docker Compose startup.
- **Automated tests** — unit and integration-oriented tests for configuration, Compose handling, and orchestration.

## Requirements

- .NET 10 SDK
- Docker Engine or Docker Desktop
- Docker Compose v2

## Build from source

```bash
git clone https://github.com/Yggorek/Dciex.git
cd Dciex
dotnet build Dciex.sln
```

## CLI commands

Run commands from the repository root:

```bash
dotnet run --project src/Dciex.CLI -- hello
```

Register a project by its directory path:

```bash
dotnet run --project src/Dciex.CLI -- register ./MyProject
```

List registered projects:

```bash
dotnet run --project src/Dciex.CLI -- list
```

Show information about a registered project:

```bash
dotnet run --project src/Dciex.CLI -- info MyProject
```

Remove a project from the registry:

```bash
dotnet run --project src/Dciex.CLI -- unregister MyProject
```

Validate an environment configuration:

```bash
dotnet run --project src/Dciex.CLI -- validate ./MyProject/devenv.yaml
```

Start a registered project's environment:

```bash
dotnet run --project src/Dciex.CLI -- up MyProject
```

## Environment configuration

Dciex looks for `devenv.yaml` in the registered project's root directory when starting an environment. For example:

```yaml
name: MyProject
environmentMode: External

services:
  - name: postgres
    image: postgres:17
    port: 5432

  - name: redis
    image: redis:7
    port: 6379

compose:
  source: Generated
```

`compose.source` supports two values:

- `Generated` — generate `compose.yaml` from the `services` section.
- `Existing` — discover and use one existing Compose file in the project directory.

For `Existing`, Dciex searches for `compose.yaml`, `compose.yml`, `docker-compose.yaml`, and `docker-compose.yml`. If more than one supported file is found, startup is stopped because the selection is ambiguous.

`environmentMode` supports `External` and `Managed`. Managed configurations currently require an `application` section to pass validation; full managed application lifecycle support is planned work and is not yet advertised as implemented.

## Project structure

```text
Dciex/
├── src/
│   ├── Dciex.CLI/             # CLI commands and application composition
│   ├── Dciex.Core/            # Core models and configuration types
│   ├── Dciex.Application/     # Use cases and orchestration
│   └── Dciex.Infrastructure/  # Filesystem, YAML, and Docker Compose integrations
└── tests/
    └── Dciex.Tests/           # Automated tests
```

## Technology stack

- C# / .NET 10
- System.CommandLine
- Docker Compose
- YamlDotNet
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection
- xUnit

## Run tests

```bash
dotnet test Dciex.sln
```

## Roadmap

Dciex is being developed incrementally. Planned areas include:

- Managed application lifecycle management.
- Environment variables and `.env` generation/management.
- Service health checks and diagnostics.
- More complete cross-platform process management.
- Kubernetes support.
- Extensible plugin architecture.
- Docker image analysis.

## Contributing

Dciex is under active development. Bug reports, ideas, and contributions are welcome. Please open an issue to discuss substantial changes before submitting a pull request.

## License

A license has not been selected yet. Licensing information will be added before the first stable release.

---

**Dciex — One command. Your development environment.**
