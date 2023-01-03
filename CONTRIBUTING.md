# How to Contribute:

You can contribute to DevToys app by:
- Report issues and bugs [here](https://github.com/veler/DevToys/issues/new?template=bug_report.md).
- Submit feature requests [here](https://github.com/veler/DevToys/issues/new?template=feature_request.md).
- Creating a pull request.
- Internationalization and localization:
    * See instructions below.

# How to Build and Run DevToys from source:

## On Windows

### Prerequisites
1. Make sure your machine is running on Windows 10 1903 (19h1) or later.
1. Install [Visual Studio 2022 17.3 or later](https://visualstudio.microsoft.com/vs/) installed with the following Workloads, or import the [vs2022.vsconfig](vs2022.vsconfig) file.
    * ASP.NET and web development
    * .NET Multi-Platform App UI development
    * .NET desktop development
    * Universal Windows Platform development

### Finalize your environment
1. Clone this repository.
1. Open a PowerShell command prompt in the root folder of this repository.
1. Run `init.ps1` to restore all the dependencies.
1. Open `src/DevToys.sln` with Visual Studio.
1. Once opened, set `app/dev/platforms/DevToys.Wasm` or `app/dev/platforms/DevToys.Windows` or `app/dev/platforms/DevToys.CLI` as startup project.
1. Now you should be able to build and run DevToys on your machine. If it fails, try to close the solution and reopen it again.

## On macOS and Linux

### Prerequisites
1. [**Visual Studio Code**](https://code.visualstudio.com/)
1. **.NET SDK**
    * [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet-core/6.0) (**version 6.0 (SDK 6.0.100)** or later)
    > Use `dotnet --version` from the terminal to get the version installed.
1. The [Uno Platform Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=unoplatform.vscode) Extension

### Finalize your environment
1. Clone this repository.
1. Open a Terminal
1. Install the Uno Platform tool by running the following command from the command prompt:
    ```
    dotnet tool install -g uno.check
    ```
1. Run the tool from the command prompt with the following command:
    ```
    uno-check
    ```
    If the above command fails, use the following:
    ```
    ~/.dotnet/tools/uno-check
    ```
1. Follow the instructions indicated by the tool
1. Run `init.sh` to restore all the dependencies.

// TODO explain how to debug

# Internationalization and localization

There are two possibilities offered:

## Use Crowdin (preferred)

* Go on [DevToy's Crowdin project](https://crowdin.com/project/devtoys). Crowdin is a localization management platform that helps individuals to translate a project without having to be familiar with its repository.
* Log in or create an account. Join the DevToys project.
* Select the language of your choice in the list of existing supported language and let yourself guided by the website to translate the app.
* If you want to add a new language, please create a new discussion on Crowdin's website or on GitHub. We will be happy to add your language to the list.
* When your translation is done, it will be synchronized with our GitHub repository within 1 hour and create a pull request.

## Change the translations in the repository yourself 

This approach is more complex but has the advantage that it allows you to test your changes on your local machine.

* After following `How to Build and Run DevToys from source`, close Visual Studio, if any instance is running.
* In File Explorer, copy the folder `dev/impl/DevToys/Strings/en-US` and rename the copied folder with the language indication of your choice. For example, "fr-FR" for French (France).
* Open `src/DevToys.sln` with Visual Studio.
* Open each `.resw` file from the language folder you created and translate the text.
* Build and Run the app and test your changes.

# Coding

## Main architecture

// TODO

## Develop a tool

// TODO

## Iconography

Icons in the UI of a tool or in the main UI of DevToys uses [Fluent System Icons](https://github.com/microsoft/fluentui-system-icons).
For the icons of the tools, a custom font is used. See [documentation](https://github.com/veler/DevToys/blob/main/assets/font/README.md) here for modifying it (or ask us for help!).

## Sample

// TODO

## Things to keep in mind

We try to avoid at maximum any UWP capability/permission like `internet`, `camera`, `location`...etc. The reason why is that this app is designed to be a tool that we can **trust** when pasting sensitive data inside.
Therefore, when making changes to DevToys, please try at maximum to avoid any capability requirement.

## Code Style

1. DO use `PascalCase`:
- class names
- method names
- const names

2. DO use `camelCase`:
- method arguments
- local variables
- private fields

4. DO NOT use Hungarian notation.

5. DO NOT use underscores, hyphens, or any other non-alphanumeric characters.

6. DO NOT use Caps for any names.

7. DO use predefined type names like `int`, `string` etc. instead of `Int32`, `String`.

8. DO use `_` prefix for private field names.

9. DO use the `I` prefix for Interface names.

10. DO vertically align curly brackets.

11. DO NOT use `Enum` or `Flag(s)` suffix for Enum names.

12. DO use prefix `Is`, `Has`, `Have`, `Any`, `Can` or similar keywords for Boolean names.

13. DO use curly brackets for single line `if`, `for` and `foreach` statements.

14. DO use nullable reference type by adding `#nullable enable` at the top of every C# file.
