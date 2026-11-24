<div align="center">
  <h1>Dciex</h1>
  <p><strong>Кроссплатформенный CLI для управления локальными окружениями разработки.</strong></p>
  <p>
    <a href="README.md">🇬🇧 English</a> ·
    <a href="README.ru.md">🇷🇺 Русский</a> ·
    <a href="README.es.md">🇪🇸 Español</a>
  </p>
</div>

> **Статус проекта:** активная разработка. Первая стабильная версия ещё не выпущена.

Dciex — это Open Source CLI-инструмент, цель которого — упростить создание и управление локальными окружениями разработки. Он объединяет регистрацию проектов, конфигурацию окружения через YAML, поиск и генерацию Docker Compose-файлов, а также запуск окружения в единый рабочий процесс.

## Что уже реализовано

- **Реестр проектов** — регистрация, просмотр списка, получение информации и удаление проектов из реестра.
- **YAML-конфигурация** — чтение и проверка файлов `devenv.yaml`.
- **Генерация Compose** — создание `compose.yaml` на основе конфигурации окружения Dciex.
- **Поиск существующего Compose** — обнаружение Docker Compose-файла в директории проекта.
- **Запуск окружения** — координация загрузки конфигурации, выбора или генерации Compose-файла и запуска Docker Compose.
- **Автоматизированные тесты** — тесты конфигурации, работы с Compose и оркестрации.

## Требования

- .NET 10 SDK
- Docker Engine или Docker Desktop
- Docker Compose v2

## Сборка из исходного кода

```bash
git clone https://github.com/Yggorek/Dciex.git
cd Dciex
dotnet build Dciex.sln
```

## Команды CLI

Команды запускаются из корня репозитория:

```bash
dotnet run --project src/Dciex.CLI -- hello
```

Зарегистрировать проект по пути к его директории:

```bash
dotnet run --project src/Dciex.CLI -- register ./MyProject
```

Посмотреть список зарегистрированных проектов:

```bash
dotnet run --project src/Dciex.CLI -- list
```

Получить информацию о зарегистрированном проекте:

```bash
dotnet run --project src/Dciex.CLI -- info MyProject
```

Удалить проект из реестра:

```bash
dotnet run --project src/Dciex.CLI -- unregister MyProject
```

Проверить конфигурацию окружения:

```bash
dotnet run --project src/Dciex.CLI -- validate ./MyProject/devenv.yaml
```

Запустить окружение зарегистрированного проекта:

```bash
dotnet run --project src/Dciex.CLI -- up MyProject
```

## Конфигурация окружения

При запуске окружения Dciex ищет `devenv.yaml` в корневой директории зарегистрированного проекта. Пример:

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

Параметр `compose.source` поддерживает два значения:

- `Generated` — сгенерировать `compose.yaml` на основе секции `services`.
- `Existing` — найти и использовать один существующий Compose-файл в директории проекта.

Для режима `Existing` Dciex ищет `compose.yaml`, `compose.yml`, `docker-compose.yaml` и `docker-compose.yml`. Если найдено несколько поддерживаемых файлов, запуск прекращается, поскольку выбор неоднозначен.

Параметр `environmentMode` поддерживает значения `External` и `Managed`. Для прохождения валидации конфигурация `Managed` сейчас требует секцию `application`; полноценное управление жизненным циклом приложения пока находится в планах и не заявляется как реализованная возможность.

## Структура проекта

```text
Dciex/
├── src/
│   ├── Dciex.CLI/             # CLI-команды и сборка приложения
│   ├── Dciex.Core/            # Основные модели и типы конфигурации
│   ├── Dciex.Application/     # Сценарии использования и оркестрация
│   └── Dciex.Infrastructure/  # Файловая система, YAML и Docker Compose
└── tests/
    └── Dciex.Tests/           # Автоматизированные тесты
```

## Архитектура

Обзор структуры проекта и зависимостей компонентов приведён в [документе по архитектуре](docs/ru/ARCHITECTURE.md).

## Технологический стек

- C# / .NET 10
- System.CommandLine
- Docker Compose
- YamlDotNet
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection
- xUnit

## Запуск тестов

```bash
dotnet test Dciex.sln
```

## Roadmap

Dciex развивается итеративно. В планах:

- Управление жизненным циклом приложений в режиме Managed.
- Управление переменными окружения и генерация `.env`.
- Проверка доступности сервисов и диагностика.
- Более полное кроссплатформенное управление процессами.
- Поддержка Kubernetes.
- Расширяемая архитектура плагинов.
- Анализ Docker-образов.

## Участие в разработке

Dciex находится в активной разработке. Мы открыты к сообщениям об ошибках, идеям и вкладу сообщества. Перед отправкой крупных изменений предлагаем сначала обсудить их в Issue.

## Лицензия

Проект распространяется под лицензией Apache License 2.0 (Apache-2.0). Полный текст лицензии находится в корневом файле [LICENSE](LICENSE).

---

**Dciex — одна команда. Твоё окружение разработки.**
