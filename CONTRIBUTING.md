# How to Contribute:

You can contribute to DevToys app by:
- Report issues and bugs [here](https://github.com/DevToys-app/DevToys/issues/new?template=bug_report.md).
- Submit feature requests [here](https://github.com/DevToys-app/DevToys/issues/new?template=feature_request.md).
- Creating a pull request.
- Internationalization and localization:
    * See instructions [here](#internationalization-and-localization).

# How to Build and Run DevToys and DevToys CLI (without tools) from source:

## From Windows

### Prerequisites
1. Make sure your machine is running on Windows 10 1903 (19h1) or later.
1. Install [**Visual Studio 2022 17.3 or later**](https://visualstudio.microsoft.com/vs/) installed with the following Workloads, or import the [vs2022.vsconfig](vs2022.vsconfig) file.
    * ASP.NET and web development
    * Node.js development
    * .NET desktop development
1. Install latest [Node.js](https://nodejs.org/).

### Finalize your environment
1. Clone this repository.
1. Open a PowerShell command prompt in the root folder of this repository.
1. Install Nuke.Build command line tooling with the following command from the command prompt:
    ```
    dotnet tool install Nuke.GlobalTool --global
    ```
1. Restore all the dependencies with the following command:
    ```
    .\init.ps1
    ```

### Build, Run & Debug
1. Open `src/DevToys-Windows.sln` with Visual Studio.
1. In Visual Studio, set `app/dev/platforms/desktop/DevToys.Windows` or `app/dev/platforms/desktop/DevToys.CLI` as startup project.
1. Now you should be able to build and run DevToys on your machine by pressing `F5`.
1. Most of the `DevToys.Windows` app runs in a web browser (WebView2). Press `F12` to open the web developer tools.

## From macOS

### Prerequisites
1. Make sure your machine is running on macOS 12.0 or later.
1. Install [**Xcode 15.0**](https://developer.apple.com/xcode/) or later. Run it at least once and allow it to install the built-in macOS and iOS tooling.
1. [**Visual Studio Code**](https://code.visualstudio.com/) with [**C# Dev Kit**](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit), or [**JetBrains Rider**](https://www.jetbrains.com/rider/).
   > [**Visual Studio for Mac**](https://visualstudio.microsoft.com/vs/mac/) is not supported because it is a deprecated product and does not support .NET 8.0.
1. **.NET SDK**. This is required to build the app itself.
    * [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet-core/8.0) (**version 8.0 (SDK 8.0.100)** or later). If you're on a [Mac computer with Apple silicon processor](https://support.apple.com/en-us/HT211814), you need to install the Arm64 version of the SDK.
    > Use `dotnet --version` from the terminal to get the version installed.
1. [Node.js](https://nodejs.org/) **14.0 or later**. This is required to build Monaco Editor.
2. If you're using an [Mac computer with Apple silicon processor](https://support.apple.com/en-us/HT211814), install [Rosetta 2](https://developer.apple.com/documentation/apple-silicon/about-the-rosetta-translation-environment) using the following command in a Terminal:
    ```
    softwareupdate --install-rosetta
    ```

### Finalize your environment
1. Clone this repository.
1. Open a Terminal
1. Install Nuke.Build command line tooling with the following command from the command prompt:
    ```
    dotnet tool install Nuke.GlobalTool --global
    ```
1. Restore all the dependencies with the following command:
    ```
    sh init.sh
    ```

### Build, Run & Debug
#### If you are using JetBrains Rider:
1. Open `src/DevToys-MacOS.sln` with JetBrains Rider.
1. Set `app/dev/platforms/desktop/DevToys.MacOS` or `app/dev/platforms/desktop/DevToys.CLI` as startup project.
1. Now you should be able to build and run DevToys on your machine.

#### If you are using Visual Studio Code:
1. Open the repository in Visual Studio Code to edit the code.
1. In `Run and Debug`, select `DevToys MacOS` or `DevToys CLI` and press Start.

#### Special note for `DevToys.MacOS`
Most of the `DevToys.MacOS` app runs in a web browser (Safari). In order to access the Safari developer tools with macOS to debug the HTML/CSS/JS of the Blazor app, you might need to follow the following instructions:
1. Open desktop Safari.
2. Select the Safari > Settings (named Preferences before Safari 17) > Advanced > Show features for web developers in the menu bar checkbox.
3. Run the `DevToys.MacOS` app in macOS, in the `Debug` configuration. The Web Inspector is only enabled in `Debug`.
4. Return to Safari. On the main menu, select Develop > {REMOTE INSPECTION TARGET} > {HOST}, where the {REMOTE INSPECTION TARGET} placeholder is either the devices's plain name (for example, MacBook Pro) or the device's serial number (for example XMVM7VFF10), and {HOST} is `localhost` (on macOS 15.0 and later or `0.0.0.0` on earlier versions). If multiple entries for {HOST} are present, select the entry that highlights the BlazorWebView. The BlazorWebView is highlighted in blue in macOS when the correct {HOST} entry is selected.
5. The Web Inspector window appears for the BlazorWebView.

## From Linux

### Prerequisites
1. Make sure your machine has GTK4 and WebKitGTK installed. Distro like Ubuntu generally have it pre-installed.
1. [**Visual Studio Code**](https://code.visualstudio.com/) with [**C# Dev Kit**](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit), or [**JetBrains Rider**](https://www.jetbrains.com/rider/).
1. **.NET SDK**. This is required to build the app itself.
    * [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet-core/8.0) (**version 8.0 (SDK 8.0.100)** or later).
    * Unless GTK has been installed through `snap`, we recommend to avoid installing .NET from `snap`.
    * We recommend installing .NET from the [official script](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install), then [the dependencies](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#dependencies), and finally set the [environment variables](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#set-environment-variables-system-wide).
    * > Use `dotnet --version` from the terminal to get the version installed.
1. [Node.js](https://nodejs.org/) **14.0 or later**. This is required to build Monaco Editor.

### Finalize your environment
1. Clone this repository.
1. Open a Terminal command prompt in the root folder of this repository.
1. Install Nuke.Build command line tooling with the following command from the command prompt:
    ```
    dotnet tool install Nuke.GlobalTool --global
    ```
1. Restore all the dependencies with the following command:
    ```
    bash init.sh
    ```

### Build, Run & Debug
#### If you are using JetBrains Rider:
1. Open `src/DevToys-Linux.sln` with JetBrains Rider.
1. Set `app/dev/platforms/desktop/DevToys.Linux` or `app/dev/platforms/desktop/DevToys.CLI` as startup project.
1. Now you should be able to build and run DevToys on your machine.

#### If you are using Visual Studio Code:
1. Open the repository in Visual Studio Code to edit the code.
1. In `Run and Debug`, select `DevToys Linux` or `DevToys CLI` and press Start.

# How to Build and Run DevToys and DevToys CLI with default tools from source:

See [DevToys.Tools's CONTRIBUTING.md](https://github.com/DevToys-app/DevToys.Tools/blob/main/CONTRIBUTING.md) file.

## Running with the default tools

The default tools live in the [DevToys.Tools](https://github.com/DevToys-app/DevToys.Tools) repository and are loaded as an extension. Point the `EXTRAPLUGIN` environment variable at the extension's build output to load it without installing it, leaving any DevToys installation on the machine untouched.

These steps assume both repositories sit side by side:

```
<your workspace>/
  DevToys/         <- this repository
  DevToys.Tools/
```

1. Clone and build the extension:
    ```
    cd <your workspace>
    git clone https://github.com/DevToys-app/DevToys.Tools
    cd DevToys.Tools
    dotnet build src/DevToys.Tools/DevToys.Tools.csproj -c Debug
    ```
1. Launch the build you made from this repository, with `EXTRAPLUGIN` set to the folder containing `DevToys.Tools.dll`. On macOS:
    ```
    cd <your workspace>/DevToys
    EXTRAPLUGIN="<your workspace>/DevToys.Tools/bin/Debug/AnyCPU/DevToys.Tools/net8.0" \
      ./bin/Debug/AnyCPU/DevToys.MacOS/net8.0-macos/osx-arm64/DevToys.app/Contents/MacOS/DevToys
    ```
    On Windows and Linux, set the variable in the environment the app starts from, or in your IDE's run configuration, then start the app as described in **Build, Run & Debug** above.

If the tool list is still empty, check that `EXTRAPLUGIN` is an absolute path to the folder containing `DevToys.Tools.dll`. A path that does not exist is silently ignored.

To debug the extension itself rather than the shell, use the `DevToys GUI` launch profile in the `DevToys.Tools` repository. It sets `EXTRAPLUGIN` for you and launches the executable named by `DevToysGuiDebugEntryPoint`, which you can point at the build you made here.

# Internationalization and localization

Please use Crowdin to translate DevToys and its tools. Crowdin is a localization management platform that helps individuals to translate a project without having to be familiar with its repository.

* Go on [DevToy's Crowdin project](https://crowdin.com/project/devtoys).
* Log in or create an account. Join the DevToys project.
* Select the language of your choice in the list of existing supported language and let yourself guided by the website to translate the app.
* If you want to add a new language, please create a new discussion on Crowdin's website or on GitHub. We will be happy to add your language to the list.
* When your translation is done, it will be synchronized with our GitHub repository within 1 hour and create a pull request.
