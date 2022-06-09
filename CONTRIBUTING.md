# How to Contribute:

You can contribute to DevToys app by:
- Report issues and bugs [here](https://github.com/veler/DevToys/issues/new?template=bug_report.md).
- Submit feature requests [here](https://github.com/veler/DevToys/issues/new?template=feature_request.md).
- Creating a pull request.
- Internationalization and localization:
    * See instructions below.

# How to Build and Run DevToys from source:

* Make sure your machine is running on Windows 10 1903+.
* Make sure you have Visual Studio 2019 16.10+ or Visual Studio 2022 17.0+ installed.
* In Visual Studio Installer, install the required components by importing the [vs2022.vsconfig](vs2022.vsconfig) or [vs2019.vsconfig](vs2019.vsconfig) file.
* Run `init.ps1` in a PowerShell command prompt to restore all the dependencies.
* Open `src/DevToys.sln` with Visual Studio and set Solution Platform to x64*.
* Once opened, set `src/dev/DevToys.Startup/DevToys.Startup.wapproj` as startup project.
* Now you should be able to build and run DevToys on your machine. If it fails, try to close the solution and reopen it again.

**If x64 doesn't work, use the architecture of your system*

# Internationalization and localization

There are two possibilities offered:

## Use Crowdin (preferred)

* Go on [DevToy's Crowdin project](https://crowdin.com/project/devtoys). Crowdin is a localization management platform that helps individuals to translate a project without having to be familiar with its repository.
* Log in or create an account. Join the DevToys project.
* Select the language of your choice in the list of existing supported language and let yourself guided by the website to translate the app.
* If you want to add a new language, please create a new discussion on Crowdin's website or on GitHub. We will be happy to add your language to the list.
* When your translation is done, it will be synchronized with our GitHub repository within 1 hour and create a pull request.

## Change yourself the translations in repository

This approach is more complex but has the advantage that it allows you to test your changes on your local machine.

* After following `How to Build and Run DevToys from source`, close Visual Studio, if any instance is running.
* In File Explorer, copy the folder `dev/impl/DevToys/Strings/en-US` and rename the copied folder with the language indication of your choice. For example, "fr-FR" for French (France).
* Open `src/DevToys.sln` with Visual Studio.
* Open each `.resw` file from the language folder you created and translate the text.
* Build and Run the app and test your changes.

# Coding

## Main architecture

DevToys is using [MEF](https://docs.microsoft.com/en-us/dotnet/framework/mef/) as a dependency injection framework.
Every tool available (i.e Base64 Encoder/Decoder, JSON Formatter, Settings...) are dynamically discovered and instantiated through MEF. A tool is divided in 3 components:
1. [IToolProvider](https://github.com/veler/DevToys/blob/main/src/dev/impl/DevToys/Api/Tools/IToolProvider.cs) and its metadata, which represents the tool as displayed in the main menu in the app. `IToolProvider` should be MEF exported.
2. [IToolViewModel](https://github.com/veler/DevToys/blob/main/src/dev/impl/DevToys/Api/Tools/IToolViewModel.cs), which is a ViewModel as described by the [MVVM](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel) pattern in UWP. It doesn't have to be MEF exported but may be required depending on what the tool needs to work.
3. A `Page` that represents the view of the tool.

The tool provider is instantiated when the app starts. The view and view models are instantiated when the user selects the tool in the main menu.

## IToolProvider metadata

Several attributes can be used when implementing an `IToolProvider`. They can be used in customize the behavior of the tool in DevToys without needing to implement a special logic for it.
You can find the attributes [here](https://github.com/veler/DevToys/tree/main/src/dev/impl/DevToys/Api/Tools). Here is a non-exhaustive list of attribute to use:
* [CompactOverlaySizeAttribute](https://github.com/veler/DevToys/blob/main/src/dev/impl/DevToys/Api/Tools/CompactOverlaySizeAttribute.cs)
* [NameAttribute](https://github.com/veler/DevToys/blob/main/src/dev/impl/DevToys/Api/Tools/NameAttribute.cs)
* [NotScrollableAttribute](https://github.com/veler/DevToys/blob/main/src/dev/impl/DevToys/Api/Tools/NotScrollableAttribute.cs)
* [OrderAttribute](https://github.com/veler/DevToys/blob/main/src/dev/impl/DevToys/Api/Tools/OrderAttribute.cs)
* [ProtocolNameAttribute](https://github.com/veler/DevToys/blob/main/src/dev/impl/DevToys/Api/Tools/ProtocolNameAttribute.cs)

## Sample

A good tool to take an example on is `Json <> Yaml` converter.
* [The tool provider](https://github.com/veler/DevToys/blob/main/src/dev/impl/DevToys/ViewModels/Tools/Converters/JsonYaml/JsonYamlToolProvider.cs)
* [The view model](https://github.com/veler/DevToys/blob/main/src/dev/impl/DevToys/ViewModels/Tools/Converters/JsonYaml/JsonYamlToolViewModel.cs)
* [The view](https://github.com/veler/DevToys/tree/main/src/dev/impl/DevToys/Views/Tools/Converters/JsonYaml)

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
