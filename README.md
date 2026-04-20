# Calabonga.Commandex

## Description

Command Executer or Command Launcher. To run commands of any type for any purpose. For example, to execute a stored procedure or just to copy some files to some destination.

## Ingredients

WPF, MVVM, CommunityToolkit, etc.

## What is Calabonga.Commandex

The `Calabonga.Commandex` - This is an application on WPF-platform built with CommunityToolkit.MVVM for modules (plugins) using: launch and execute.

What is the `Calabonga.Commandex` can:
* Find a modules `.dll` (plugins) in the folder you set up.
* Launch or execute modules `.dll` (plugins) from GUI.
* Get the results of the module's (plugins) work after they completed.

It's a complex solution with a few repositories:

* **[Calabonga.Commandex.Shell](https://github.com/Calabonga/Calabonga.Commandex.Shell)** →  Command Executer or Command Launcher. To run commands of any type for any purpose. For example, to execute a stored procedure or just to copy some files to some destination.
* **[Calabonga.Commandex.Commands](https://github.com/Calabonga/Calabonga.Commandex.Commands)** →  Commands for Calabonga.Commandex.Shell that can execute them from unified shell.
* **[Calabonga.Commandex.Shell.Develop.Template](https://github.com/Calabonga/Calabonga.Commandex.Shell.Develop.Template)** →  (`Tool Template`) This is a Developer version of the Command Executer Shell (`Calabonga.Commandex`). Which is created to runs commands of any type for any purposes. For example, to execute a stored procedure or just to co…
* **[Calabonga.Commandex.Engine](https://github.com/Calabonga/Calabonga.Commandex.Engine)** →  Engine and contracts library for Calabonga.Commandex. Contracts are using for developing a modules for Commandex Shell.
* **[Calabonga.Commandex.Engine.Processors](https://github.com/Calabonga/Calabonga.Commandex.Engine.Processors)** →  Results Processors for Calabonga.Commandex.Shell commands execution results. This is an extended version of the just show string in the notification dialog.
* **[Calabonga.CommandexCommand.Template](https://github.com/Calabonga/Calabonga.CommandexCommand.Template)** →  (`Tool Template`) This is a template of the project to create a Command for Commandex. Just install this nuget as a template for Visual Studio (Rider or dotnet CLI) and then you can create a DialogCommand faster.

## History of changes

### v4.0.0 2026-04-20

Migration to NET10

### v3.0.0 2025-07-30

Engine package update brings a new type of the Commandex command - ZoneCommandexCommand<TView, TViewModel>. It's like a dialog but shown in shell (built-in).

**Full Changelog**: https://github.com/Calabonga/Calabonga.Commandex.Shell/compare/2.8.1...3.0.0

### v2.5.0 2025-06-29

* Nuget-packages dependencies updates installed
* All commands updated to 2.5.0 engine use
* `IVersionService` removed. Version now taken from `Assembly`
* Menu `About` some helpful links added.

### v2.0.0 2025-06-20

* ` viewModelLocator:ViewModelLocator.AutoBindingViewModel="True"` for some views implemented.
* Nuget-packages dependencies updates installed

### v1.10.0 2024-10-12

* New parameter created globally - `SettingsPath`. It appends possibility store commands settings files separately from commands. If it not set then command settings env-files will store in the commands folder.
* Enigne release update 1.3.0
* Some summaries added/updated
* AboutWindow information extended

### v1.9.0 2024-10-05

New feature - OAuth2.0 now in the Commandex Command! Authentication OAuth2.0 added (OAuth2.0). Demo OAuth2.0 server provided by https://demo.duendesoftware.com. Some settings already set up in Commandex. You can test OAuth2.0 in Commandex using login/pass bob/bob or alice/alice. This logins provided by [Duende](https://demo.duendesoftware.com/grants). 

LogIn Screen


![](https://github.com/user-attachments/assets/a422c66e-492e-4e82-8cd3-dacde752927c)

Main screen after log in success

![](https://github.com/user-attachments/assets/58ff0de3-2329-4c89-ac16-b9d0c3a71cea)

Thanks to Duende for testing OAuth2.0 server! 

### v1.8.0 2024-09-30

* Enigne release update 1.0.0
* Some summaries added/updated
* Some methods are renamed for clarity

### v1.7.0 2024-09-18

*What's Changed*

* Huge refactoring by @Calabonga in https://github.com/Calabonga/Calabonga.Commandex.Shell/pull/20
* Wizard windows style fix by @Calabonga in https://github.com/Calabonga/Calabonga.Commandex.Shell/pull/21
* application icon added by @Calabonga in https://github.com/Calabonga/Calabonga.Commandex.Shell/pull/22
* RC.16 by @Calabonga in https://github.com/Calabonga/Calabonga.Commandex.Shell/pull/23
* Hierarchical commands list by @Calabonga in https://github.com/Calabonga/Calabonga.Commandex.Shell/pull/24


**Full Changelog**: https://github.com/Calabonga/Calabonga.Commandex.Shell/compa

### v1.0.0-rc.16 2024-09-16

* UX refactored
  * Menu added
  * Shortcuts added
  * Three type of the command list view added
* `IDialogResult` renamed to `IViewModel`
* New property `Tags` added to `ICommandexCommand` for future commands groups management

## Known Issues

When your Command need to use `Microsoft.Data.SqlClient` then you should install reference (nuget) in the `Shell` application. [More information](https://stackoverflow.com/questions/78411196/what-data-sqlclient-can-be-used-with-net-8)

## Видео (Video)

### [1. Commandex Framework - Модульный монолит. Идея.](https://boosty.to/calabonga/posts/88abe79c-c396-4b03-9dc2-4c76b20a25ca)


В продолжение темы про эволюцию архитектуры программного обеспечения, в котором речь зашла про "модульный монолит".
В этом видео - пример реализации модульного монолита.

[![Commandex-1-intro](https://github.com/user-attachments/assets/acf1289a-c89e-40ba-bfda-0b24867c18bd)](https://boosty.to/calabonga/posts/88abe79c-c396-4b03-9dc2-4c76b20a25ca)

### [2. Commandex Framework - Модульный монолит. Знакомство.](https://boosty.to/calabonga/posts/3915ad48-1f0b-44cc-83ac-a1981d8d6c8e)

В этом видео я представлю вашему вниманию свой новый фреймворк, которые позволит без особых усилий организовать на платформе WPF приложение по работе с плагинами.

[![Commandex-2-framework](https://github.com/user-attachments/assets/599dd945-8a1f-43ac-8318-73bea7b99cb8)](https://boosty.to/calabonga/posts/3915ad48-1f0b-44cc-83ac-a1981d8d6c8e)

### [3. Commandex Framework - Модульный монолит. Shell.](https://boosty.to/calabonga/posts/dba3f618-314e-4383-ae7b-2485ba93a058)

Немного теории про Commandex Framework. В этом видео подробнее про Commandex Shell:

* Клонирование репозитория (загрузка)
* Компиляция
* Настройки в .env
* Справка по работе с Shell
   * Папка с плагинами
   * Nuget установленные и не только...
   * Логирование
   * И другие полезные мелочи

[![Commandex-3-shell](https://github.com/user-attachments/assets/72ce2079-9fef-44fb-aa61-5aa2fa9b7219)](https://boosty.to/calabonga/posts/dba3f618-314e-4383-ae7b-2485ba93a058)

### [4. Commandex Framework - Модульный монолит. EmptyCommand.](https://boosty.to/calabonga/posts/fdfd99c2-a3d2-4b19-94ee-eae01aac2ae0)

`EmptyCommandexCommand` - это самый простой тип команды, которые ничего не должен возвращать на Shell, простой триггер, которые можно что-то сделать, и при этом не должен никому ни в чем отчитываться. Simplest command type for Commandex. This type does not anything returns to shell.

[![Commandex-4-empty-command](https://github.com/user-attachments/assets/9574459f-94e2-4cc3-8aa1-2d5d70a9c606)](https://boosty.to/calabonga/posts/fdfd99c2-a3d2-4b19-94ee-eae01aac2ae0)

### [5. Commandex Framework - Модульный монолит. Result-команды.](https://boosty.to/calabonga/posts/6fc9185d-feda-4155-b92b-796607604c58)

`ResultCommandexCommand<T>` - это вид команды, который может вернуть на Shell результаты своей работы. На видео были созданы две новые команды:

* `CheckApiReadyCommandexCommand` - возвращает `bool`-значение, которое выбирается случайным образом. 
* `ValidateDocumentCommandexCommand` - возвращает класс `ValidateResult`, значения свойств которого, также выбирается случайным образом. Вдобавок, я показал как обрабатывает Shell ошибки, которые возникают при выполнении команд.

[![Commandex-5-result-command](https://github.com/user-attachments/assets/e690628f-1be5-4c1f-92e7-7468a22b0686)](https://boosty.to/calabonga/posts/6fc9185d-feda-4155-b92b-796607604c58)

### [6. Commandex Framework - Модульный монолит. Dialog-команды.](https://boosty.to/calabonga/posts/f73a1f14-4e64-4703-9c08-2add245125d6)

В этом видео покажу пример создания команды нового типа. На этот раз будем создавать DialogCommandexComand. То есть команда, которая открывается в диалоговом окне. 

[![Commandex-6-framework](https://github.com/user-attachments/assets/6dc338cb-cccd-4007-9229-9ee1f67c9126)](https://boosty.to/calabonga/posts/f73a1f14-4e64-4703-9c08-2add245125d6)

### [7. Commandex Framework - Модульный монолит. Wizard-команды.](https://boosty.to/calabonga/posts/5b5f8647-ea25-420b-9f97-9c5a5c1a200e)

Пошаговые окна на MVVM. Тип команды для Commandex - Wizard. В этом видео покажу пример создания команды нового типа. На этот раз будем создавать WizardCommandexComand. То есть команда, которая открывается в диалоговом окне и при этом имеет несколько шагов с переходами Вперед и Назад. На выходе - объект с собранными данными.

[![Commandex-7-framework](https://github.com/user-attachments/assets/f2d26272-b7cc-4b4b-a8cd-3824d7f871bb)](https://boosty.to/calabonga/posts/5b5f8647-ea25-420b-9f97-9c5a5c1a200e)

### [8. Commandex Framework - Модульный монолит. Shell. Re-design.](https://boosty.to/calabonga/posts/8c50a396-cbfb-4a1b-b32b-60431f09dc43)

Ничего не стоит на месте, и Shell для Commandex Framework тоже притерпел некоторые изменения.
Ключевые моменты редизайна:
 * Больше места для команд
 * Меню вместо кнопок
 * Быстрые кнопки (shortcuts)
 * 6 типов представлений команд (3 линейных списка + 3 древовидных списка)
 * Система меток для комманд
 * Управляемая иерархия на базе предопределенных пунктов (+ система меток)
 * И другие улучшения.

[![Commandex-8-framework](https://github.com/user-attachments/assets/9ef53830-12c4-4705-9a1f-3babc111a877)](https://boosty.to/calabonga/posts/8c50a396-cbfb-4a1b-b32b-60431f09dc43)

### [9. Commandex Framework - Модульный монолит. Parameter-команды.](https://boosty.to/calabonga/posts/d39c830e-8b99-4f93-8e12-248f10a88f69)

Передача параметров между командами? Легко! Такое тоже возможно при использовании команды специального типа.
В этом видео покажу пример создания команды типа ParameterCommandexComand. То есть команда, которая может читать параметр и сохранять параметр. Другими словами, две команды типа ParameterCommandexComand могут между собой передавать данные. В этом видео пример таких команд.

[![Commandex-9-framework](https://github.com/user-attachments/assets/15062bf0-98d8-4d72-a011-4799484861e4)](https://boosty.to/calabonga/posts/d39c830e-8b99-4f93-8e12-248f10a88f69)

### [10. Commandex Framework - Модульный монолит. Вопросы и ответы.](https://boosty.to/calabonga/posts/d67a3232-1aae-46d0-8286-42f0d6b6e3b0)

Вот список пришедших вопросов (по разным каналам связи):
* Commandex Framework — это все четыре репозитория?
* Commandex Framework — это законченный проект, он готов для production?
* Shell можно сделать на ASP.NET Core?
* Commandex Framework имеет "дорожную карту"? Что планируется?
Команды для Commandex Framework можно загружать как zip-файлы и запускать?
* Нельзя поставить некоторые nuget-пакеты, вернее поставишь один пакет, а говорит, что не хватает совершенного другого. Как это?
* Можно загружать nuget-пакеты не с nuget.org?

[![commandEx-faq-1](https://github.com/user-attachments/assets/7ba3f008-7851-4c25-ba3d-88b6ba5e5192)](https://boosty.to/calabonga/posts/d67a3232-1aae-46d0-8286-42f0d6b6e3b0)

### [11. Commandex Framework - Модульный монолит. OAuth2.0 Аутентификация.](https://boosty.to/calabonga/posts/4ee915e1-4196-4c2a-b270-c76d0352ae96)
Краткий обзор нового функционала Commandex в приложении Shell. 
В этом видео покажу, как работает custom OAuth2.0 на WPF + MVVM. А также покажу, как и из чего состоит система, а также напомню (кратко) про то, что такое OAuth2.0.

[![Commandex-11-shell-oauth](https://github.com/user-attachments/assets/f2c0ffba-564f-4ce5-91bb-8563348d0892)](https://boosty.to/calabonga/posts/d67a3232-1aae-46d0-8286-42f0d6b6e3b0)
