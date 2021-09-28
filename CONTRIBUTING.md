# How to Contribute:

You can contribute to DevToys app by:
- Report issues and bugs [here](//TODO)
- Submit feature requests [here](//TODO)
- Create a pull request to helps:
    * Fix an existing bug, prefix title with `[FIX] `.
    * Implement new features, prefix title with `[FEATURE] `.
    * Fix grammar errors or improve my documentations, prefix title with `[DOC] `.
    * Improve CI/CD pipeline, prefix title with `[CI] `.
    * Cleanup code and code refactoring or anything else you want to change in the project not listed above, prefix title with `[OTHER] ` or assign a custom prefix with the same format (`[LABEL] `).
- Internationalization and localization:
    * See instructions below.

# How to Build and Run DevToys from source

* Make sure your machine is running on Windows 10 1903+.
* Make sure you have Visual Studio 2019 16.10+ installed.
* In Visual Studio Installer, install the required components by importing the `.vsconfig` file from the root of the repository. 
* Run `init.ps1` in a PowerShell command prompt to restore all the dependencies.
* Open `src/DevToys.sln` with Visual Studio and set Solution Platform to x64*.
* Once opened, set `src/dev/startup/DevToys.Startup/DevToys.Startup.wapproj` as startup project.
* Now you should be able to build and run DevToys on your machine. If it fails, try close the solution and reopen it again.

**If x64 doesn't work, use the architecture of your system*

# Internationalization and localization

* After following `How to Build and Run DevToys from source`, open `src/DevToys.sln` with Visual Studio.
* Copy the folder `dev/impl/DevToys.Common/Strings/en` and rename the copied folder with the language indication of your choice. For example, "fr-FR" for French (France).
* Open each `.resw` files and translate the text.
* Open `dev/impl/DevToys.Common/LanguageManager.tt`.
* Copy the following block and add it to the line below: https://etienne-baudoux.visualstudio.com/Side%20projects/_git/DevToys/?path=src/dev/impl/DevToys.Common/LanguageManager.tt&version=GC35fe1ba4bf76a1a4e8274f8ce3119c384325634b&lineStyle=plain&line=100&lineEnd=106&lineStartColumn=18&lineEndColumn=18
* Change the `Identifier`, `DisplayName` and `Culture` to match your language.
* Save the file. Visual Studio will ask you to confirm. Click Yes.
* Build and Run the app and test your changes.

// TODO: We should seriously consider a solution where contributors don't need to change LanguageManager.tt

# Coding

## Main architecture

DevToys is using [MEF](https://docs.microsoft.com/en-us/dotnet/framework/mef/) as dependency injection framework.
Every tools available (i.e Base64 Encoder/Decoder, JSON Formatter, Settings...) are dynamically discovered and instanciated through MEF. A tool is divided in 3 components:
1. `IToolProvider` and its metadata, which represents the tool as displayed in the main menu in the app. `IToolProvider` should be MEF exported.
2. `IToolViewModel`, which is a ViewModel as described by MVVM pattern in UWP. It doesn't have to be MEF exported but may be required depending on what the tool needs to work.
3. A `Page` that represents the view of the tool.

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

5. DO NOT use underscores, hyphens, or any other nonalphanumeric characters.

6. DO NOT use Caps for any names.

7. DO use predefined type names like `int`, `string` etc. instead of `Int32`, `String`.

8. DO use `_` prefix for private field names.

9. DO use `I` prefix for Interface names.

10. DO vertically align curly brackets.

11. DO NOT use `Enum` or `Flag(s)` suffix for Enum names.

12. DO use prefix `Is`, `Has`, `Have`, `Any`, `Can` or similar keywords for Boolean names.

13. DO use curly brackets for single line `if`, `for` and `foreach` statements.
