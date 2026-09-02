# CLAUDE.md — Calabonga.Commandex.Shell

Руководство для Claude Code при работе в этом репозитории. Стиль кода и git-процесс — в
`.claude/rules/code-styles.md` и `.claude/rules/workflow.md`, они обязательны к соблюдению.

## Что это

`Calabonga.Commandex.Shell` — самостоятельный Git-репозиторий
(remote `github.com/Calabonga/Calabonga.Commandex.Shell`). Внутри — **Commandex**,
WPF-приложение-лаунчер на .NET 10 (только Windows): находит в заданной папке `.dll`-модули
команд, через рефлексию поднимает их DI-регистрации, показывает список и запускает выбранную
команду.

- «Shell» — имя репозитория; «Commandex» — имя приложения (программы) в нём.
- Engine потребляется **только как NuGet-пакет** `Calabonga.Commandex.Engine.Processors`
  (сейчас `4.0.0`), который включает `Calabonga.Commandex.Engine`. Никогда не через project
  reference. Локальные правки в исходниках Engine не видны, пока не собран и не опубликован
  (или не подключён локальным feed) новый пакет.
- CI нет (в `.github/` только `FUNDING.yml`). В NuGet приложение не пакуется.
- В терминах фреймворка это репозиторий типа **Samples** — пример реализации на базе Engine.

## Структура репозитория

```
src/
  Calabonga.Commandex.sln            — решение (2 проекта)
  Calabonga.Commandex.Shell/         — приложение (OutputType=WinExe)
  Calabonga.Commandex.Shell.Tests/   — тесты (xUnit v3 / MTP)
  commandex.env                      — конфигурация окружения (DotNetEnv, TraversePath)
PublishedCommands/                   — сюда проекты команд кладут свои .dll (+ .pdb) post-build
global.json                          — только секция test (runner = Microsoft.Testing.Platform)
README.md                            — пользовательское описание и история версий (не для разработки)
```

## Окружение

- **.NET 10 SDK**, Windows. Target `net10.0-windows8.0`, `UseWPF=true`.
- `Directory.Build.props` нет; версии пакетов заданы в самом `.csproj`.
- У `Calabonga.Commandex.Shell.csproj` нет `<Version>` — версия приложения задаётся git-тегами
  (`vX.Y.Z`, последний — `v2.6.0`).

## Сборка и запуск

```bash
dotnet build src/Calabonga.Commandex.sln -c Release
dotnet run --project src/Calabonga.Commandex.Shell/Calabonga.Commandex.Shell.csproj
```

Для запуска нужен `commandex.env` с обязательным `COMMANDS_FOLDER` (см. «Конфигурация»);
`DotNetEnv` ищет файл вверх по дереву от рабочей папки процесса.

## Тесты

Покрывают группировку/преобразование в `CommandFinder` (`Calabonga.Commandex.Shell.Tests`,
**xUnit v3** + Moq).

Проект работает на **Microsoft.Testing.Platform (MTP)**, не на VSTest:

- пакеты `xunit.v3` + `xunit.runner.visualstudio`, `<OutputType>Exe</OutputType>`;
- **нет** `Microsoft.NET.Test.Sdk` и `coverlet.collector` (оба только под VSTest);
- сбор покрытия отсутствует — при необходимости `Microsoft.Testing.Extensions.CodeCoverage`
  + `dotnet test --coverage`;
- `global.json` в корне репо содержит секцию `test` (`"runner": "Microsoft.Testing.Platform"`) —
  без неё `dotnet test` на .NET 10 SDK падает с «VSTest target is no longer supported».
  Секции `sdk` в `global.json` нет.

```bash
dotnet test src/Calabonga.Commandex.sln
# фильтры — опции MTP после `--`:
dotnet test src/Calabonga.Commandex.sln -- --filter-class "*CommandFinderConverterTests"
dotnet test src/Calabonga.Commandex.sln -- --filter-method "*CanConvert_ToList*"
```

## Конфигурация (`commandex.env`)

`SettingsFinder.Configure()` загружает `commandex.env` через `DotNetEnv`
(`LoadOptions.TraversePath()`) в `CurrentAppSettings`. Ключи:

| Ключ | Назначение |
| --- | --- |
| `COMMANDS_FOLDER` | **обязательный** — папка, сканируемая на `.dll`-модули |
| `SETTINGS_FOLDER` | по умолчанию = папка команд |
| `SHOW_SEARCH_PANEL_ONSTARTUP` | показывать панель поиска при старте |
| `ARTIFACTS_FOLDER_NAME` | имя папки артефактов (по умолчанию `Artifacts`) |
| `DEFAULT_VIEW_NAME` | одно из значений `CommandViewType` |
| `NUGET_FEED_URL` | feed для NuGet-зависимостей команд (по умолчанию `https://api.nuget.org/v3/index.json`) |
| `AUTHORIZATION_SERVER_URL` / `AUTHORIZATION_CLIENT_ID` / `AUTHORIZATION_CLIENT_SECRET` / `AUTHORIZATION_GRANT_TYPE` | OAuth2.0 |

## Архитектура Shell

### Composition root: модульный DI через AppDefinitions

Регистрация построена на `Calabonga.Wpf.AppDefinitions`. `DependencyContainer.ConfigureServices()`
(`src/Calabonga.Commandex.Shell/Engine/DependencyContainer.cs`) — composition root, вызывается из
конструктора `App`; вызывает `services.AddDefinitions(types)` с типами самого Shell плюс всеми
`AppDefinition`, найденными в DLL-модулях. Каждый модуль команды предоставляет
`public sealed class XxxDefinition : AppDefinition` с `ConfigureServices(IServiceCollection)`, где
регистрирует команду + View + ViewModel (обычно `AddScoped<ICommandexCommand, XxxCommand>()`), а при
необходимости — `INugetDependency`.

### Автопривязка View ↔ ViewModel

Соглашение: `FooView` ↔ `FooViewModel` (тот же сегмент namespace, `View`→`ViewModel`). В XAML
включается через `viewModelLocator:ViewModelLocator.AutoBindingViewModel="True"`. Shell активирует
механизм вызовом `ViewModelLocationProvider.SetDefaultViewModelFactory(type => provider.GetRequiredService(type))`
сразу после `BuildServiceProvider()`.

### Конвейер обработки результата

`IResultProcessor` (из Engine) превращает `GetResult()` команды в видимый пользователю вывод. Engine
поставляет `DefaultResultProcessor` (просто строка), NuGet `Calabonga.Commandex.Engine.Processors` —
`AdvancedResultProcessor` с диспетчеризацией по типу результата (`TextFileResult`, `ClipboardResult`)
через `IProcessor`. Shell подключает продвинутый вызовом `services.AddAdvancedResultProcessor()` в
`DependencyContainer` (закомментированный `AddResultProcessor<DefaultResultProcessor>()` — альтернатива).

### Поток выполнения

1. **Конфигурация** — `SettingsFinder.Configure()` → `CurrentAppSettings` (см. «Конфигурация»).
2. **Обнаружение** — `CommandFinder.Find(CommandsPath)` (`Engine/CommandFinder.cs`) создаёт папку,
   если её нет, делает `Assembly.LoadFrom` для каждого `*.dll` и собирает экспортированные
   `AppDefinition` и `ICommandexCommand`. `AddModulesDefinitions()` передаёт их в `AddDefinitions`.
3. **Список** — команды фильтруются по поисковому запросу и группируются по `Tags` через
   `IGroupBuilder` / `DefaultGroupBuilder` в (возможно вложенное) дерево `CommandGroup`; шесть
   вариантов раскладки (`CommandViewType`).
4. **Выполнение** — `CommandExecutor.ExecuteAsync(CommandItem)`: на каждый запуск создаётся отдельный
   DI-scope, `ICommandexCommand` резолвится по `TypeName` → событие `CommandPrepareStart` →
   `ArtifactService.CheckDependenciesReadyAsync` (тянет объявленные NuGet-зависимости через
   `NugetLoader` по `NugetFeedUrl`) → `CommandPreparedSuccess` → `command.ExecuteCommandAsync()` →
   обработка результата (`IResultProcessor`, пока команда жива) → освобождение scope (и команды) на
   любом исходе, включая исключение.
5. **Identity** — `ICommandexIdentity` / `ISecureData` / `IUserManager` — контракты Engine; Shell
   реализует OAuth2.0 (`AuthenticationService`), хранит токены через `Infrastructure/Security/SecureData`
   и держит вошедшего пользователя в `CommandexStorage` (рассылка `LoginSuccessMessage` по
   `WeakReferenceMessenger`).
6. **Логирование** — Serilog в `logs/commandex-.log` (посуточная ротация); глобальные перехватчики
   в `App.SetupExceptionHandling`.

### Типы команд (контракты Engine, которые Shell исполняет)

Модуль реализует `ICommandexCommand` (`: IDisposable`): метаданные (`DisplayName`, `Description`,
`Version`, `CopyrightInfo`, `Tags`), `Task<OperationEmpty<…>> ExecuteCommandAsync()`,
`object? GetResult()`, `bool IsPushToShellEnabled`. Конкретные команды наследуют один из абстрактных
базовых классов Engine:

- `EmptyCommandexCommand` — «запустил и забыл», без результата;
- `ResultCommandexCommand<TResult>` — типизированный результат;
- `DialogCommandexCommand<TDialogView, TDialogResult>` — модальный диалог через `IDialogService`;
- `WizardDialogCommandexCommand<…>` — многошаговый wizard (`IWizardManager<>`);
- `ParameterCommandexCommand<TParams>` — общий файл параметра (JSON в base64,
  `<CommandsPath>/<kebab-имя>.prm`) для обмена данными между командами;
- `ZoneCommandexCommand<TView, TViewModel>` — View встроенно в `ContentControl` Shell с именем
  `MainZone` (`IZoneManager`), не в отдельном окне;
- `InnerCommandexCommand` — команда, вызываемая другой командой.

Полное описание контрактов и их поведения — в корневом `C:\Projects\Commandex\CLAUDE.md`
(обзор workspace, вне git).

## Добавление новой команды-модуля

1. `dotnet new install Calabonga.CommandexCommand.Template`
2. `dotnet new wpfcmdx-dialog -n My.Thing --CommandName MyThingCommand` (или `wpfcmdx-wizard` /
   `wpfcmdx-zone`). Токены `COMMAND_NAME` и имя файла `*Command` подставляются.
3. Сгенерированная WPF class library ссылается на NuGet `Calabonga.Commandex.Engine`. Реализуйте
   выбранный базовый класс и `AppDefinition`. **Версию пакета Engine держите равной версии Engine,
   с которой собран этот Shell** (сейчас Shell собран против
   `Calabonga.Commandex.Engine.Processors 4.0.0`; опубликованные версии Engine могут отставать от
   исходников в репозитории Engine).
4. В `.csproj` команды есть post-build target `CopyDLLs`, копирующий `<имя>.dll` + `.pdb` в
   `Calabonga.Commandex.Shell/PublishedCommands` (поправьте относительный `PublishedCommandsDir`,
   если проект лежит не рядом).
5. Отладка команды без полного Shell: `dotnet new install Calabonga.Commandex.Shell.Develop.Template`,
   создать этот проект рядом с командой, добавить project reference на команду и зарегистрировать её
   `AppDefinition` в его `DependencyContainer.cs`.

## Версионирование Shell

- Мажорная версия Shell всегда равна мажорной версии фреймворка (Engine). Минор/патч — своя линия.
- Framework поднял мажор → Shell переезжает на новый мажор.
- Framework поднял минор/патч → обновить `PackageReference` на `Engine.Processors` и поднять
  **только патч** Shell.
- Версии NuGet-пакетов поднимаются вручную. Релиз Shell — git-тег.
- После обновления версий команд — пересобрать их и обновить `.dll` в `PublishedCommands/`.

## Известные проблемы

- Если команде нужен `Microsoft.Data.SqlClient`, ссылку на NuGet добавляют **в сам Shell**
  (а не только в команду), иначе зависимая сборка не разрешится при загрузке модуля.

## Стиль кода и рабочий процесс

- `.claude/rules/code-styles.md` — язык C# и стиль (file-scoped namespaces, `sealed` по умолчанию,
  порядок членов, `Async`-суффикс, `Result` вместо исключений для ожидаемых потоков, `TimeProvider`
  и т. д.).
- `.claude/rules/workflow.md` — git-процесс: отдельная ветка (`feature/` `bugfix/` `hotfix/`),
  коммиты `type: description`, `dotnet test` перед коммитом, атомарные коммиты.

## Связь с остальным workspace

Локально репозиторий лежит в рабочем пространстве `C:\Projects\Commandex` рядом с ещё пятью
независимыми репозиториями (`Engine`, `Engine.Processors`, `Commands`, `Shell.Develop.Template`,
`CommandexCommand.Template`). Направление зависимостей: `Engine` → `Engine.Processors` → `Shell`,
всегда через опубликованные NuGet-пакеты. Полная карта и choreography версионирования/публикации
фреймворка — в `C:\Projects\Commandex\CLAUDE.md` (вне git, только для локальной работы во всём
workspace).
