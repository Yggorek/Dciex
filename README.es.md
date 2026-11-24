<div align="center">
  <h1>Dciex</h1>
  <p><strong>Una CLI multiplataforma para gestionar entornos de desarrollo locales.</strong></p>
  <p>
    <a href="README.md">🇬🇧 English</a> ·
    <a href="README.ru.md">🇷🇺 Русский</a> ·
    <a href="README.es.md">🇪🇸 Español</a>
  </p>
</div>

> **Estado del proyecto:** en desarrollo activo. La primera versión estable todavía no se ha publicado.

Dciex es una herramienta de línea de comandos de código abierto cuyo objetivo es simplificar la creación y la gestión de entornos de desarrollo locales. Reúne en un mismo flujo el registro de proyectos, la configuración del entorno mediante YAML, la búsqueda y generación de archivos Docker Compose y el inicio del entorno.

## Funcionalidades implementadas

- **Registro de proyectos** — registrar, listar, consultar y eliminar proyectos del registro.
- **Configuración YAML** — cargar y validar archivos `devenv.yaml`.
- **Generación de Compose** — generar un archivo `compose.yaml` a partir de la configuración del entorno de Dciex.
- **Detección de Compose existente** — localizar un archivo Docker Compose en el directorio del proyecto.
- **Inicio del entorno** — coordinar la carga de la configuración, la selección o generación del archivo Compose y el inicio de Docker Compose.
- **Pruebas automatizadas** — pruebas de configuración, gestión de Compose y orquestación.

## Requisitos

- .NET 10 SDK
- Docker Engine o Docker Desktop
- Docker Compose v2

## Compilar desde el código fuente

```bash
git clone https://github.com/Yggorek/Dciex.git
cd Dciex
dotnet build Dciex.sln
```

## Comandos CLI

Ejecuta los comandos desde la raíz del repositorio:

```bash
dotnet run --project src/Dciex.CLI -- hello
```

Registrar un proyecto indicando la ruta de su directorio:

```bash
dotnet run --project src/Dciex.CLI -- register ./MyProject
```

Mostrar los proyectos registrados:

```bash
dotnet run --project src/Dciex.CLI -- list
```

Consultar la información de un proyecto registrado:

```bash
dotnet run --project src/Dciex.CLI -- info MyProject
```

Eliminar un proyecto del registro:

```bash
dotnet run --project src/Dciex.CLI -- unregister MyProject
```

Validar la configuración de un entorno:

```bash
dotnet run --project src/Dciex.CLI -- validate ./MyProject/devenv.yaml
```

Iniciar el entorno de un proyecto registrado:

```bash
dotnet run --project src/Dciex.CLI -- up MyProject
```

## Configuración del entorno

Al iniciar un entorno, Dciex busca el archivo `devenv.yaml` en el directorio raíz del proyecto registrado. Por ejemplo:

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

El parámetro `compose.source` admite dos valores:

- `Generated` — generar `compose.yaml` a partir de la sección `services`.
- `Existing` — localizar y utilizar un único archivo Compose existente en el directorio del proyecto.

En el modo `Existing`, Dciex busca `compose.yaml`, `compose.yml`, `docker-compose.yaml` y `docker-compose.yml`. Si encuentra más de un archivo compatible, detiene el inicio porque la selección es ambigua.

El parámetro `environmentMode` admite `External` y `Managed`. Actualmente, una configuración `Managed` necesita una sección `application` para superar la validación; la gestión completa del ciclo de vida de las aplicaciones sigue en desarrollo y no se presenta como una funcionalidad ya implementada.

## Estructura del proyecto

```text
Dciex/
├── src/
│   ├── Dciex.CLI/             # Comandos CLI y composición de la aplicación
│   ├── Dciex.Core/            # Modelos principales y tipos de configuración
│   ├── Dciex.Application/     # Casos de uso y orquestación
│   └── Dciex.Infrastructure/  # Sistema de archivos, YAML y Docker Compose
└── tests/
    └── Dciex.Tests/           # Pruebas automatizadas
```

## Arquitectura

Consulta la [documentación de arquitectura](docs/es/ARCHITECTURE.md) para ver un resumen de la estructura del proyecto y las dependencias entre sus componentes.

## Tecnologías

- C# / .NET 10
- System.CommandLine
- Docker Compose
- YamlDotNet
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection
- xUnit

## Ejecutar las pruebas

```bash
dotnet test Dciex.sln
```

## Hoja de ruta

Dciex se desarrolla de forma incremental. Entre los objetivos futuros se encuentran:

- Gestión del ciclo de vida de aplicaciones en modo Managed.
- Gestión de variables de entorno y generación de archivos `.env`.
- Comprobaciones de disponibilidad de servicios y diagnóstico.
- Gestión multiplataforma más completa de procesos.
- Compatibilidad con Kubernetes.
- Arquitectura extensible mediante plugins.
- Análisis de imágenes Docker.

## Contribuir

Dciex está en desarrollo activo. Agradecemos los informes de errores, las propuestas y las contribuciones. Para cambios importantes, recomendamos abrir primero una incidencia (Issue) para debatir la propuesta.

## Licencia

Este proyecto se distribuye bajo la licencia Apache License 2.0 (Apache-2.0). El texto completo de la licencia está en el archivo raíz [LICENSE](LICENSE).

---

**Dciex — Un comando. Tu entorno de desarrollo.**
