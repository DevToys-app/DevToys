<p align="center">
  <img width="128" align="center" src="/assets/logo/300x300.png">
</p>
<h1 align="center">
  DevToys
</h1>
<p align="center">
  A Swiss Army knife for developers.
</p>
<p align="center">
  <a style="text-decoration:none" href="https://etienne-baudoux.visualstudio.com/DevToys/_build?definitionId=19&branchName=main" target="_blank">
    <img src="https://etienne-baudoux.visualstudio.com/DevToys/_apis/build/status/DevToys?branchName=main" alt="Build Status" />
  </a>
  <a title="Crowdin" target="_blank" href="https://crowdin.com/project/devtoys">
    <img src="https://badges.crowdin.net/devtoys/localized.svg">
  </a>
  <a style="text-decoration:none" href="https://github.com/veler/DevToys/releases" target="_blank">
    <img src="https://img.shields.io/github/release/veler/devtoys.svg?label=Latest%20version" alt="Latest version" />
  </a>
  <a style="text-decoration:none" href="https://community.chocolatey.org/packages/devtoys" target="_blank">
    <img src="https://img.shields.io/chocolatey/v/devtoys?include_prereleases" alt="Chocolatey Version (including pre-releases)">
  </a>
  <a style="text-decoration:none" href="https://www.microsoft.com/store/apps/9PGCV4V3BK4W" target="_blank">
    <img src="https://img.shields.io/badge/Microsoft%20Store-Download-brightgreen" alt="Store link" />
  </a>
  <a style="text-decoration:none" href="https://devtoys.app" target="_blank">
    <img src="https://img.shields.io/badge/Website-devtoys.app-blue" alt="Website" />
  </a>
</p>

## Note

`DevToys 2.0` is currently under development in branch `dev/2.0`.

## Introduction

DevToys helps with daily development tasks like formatting JSON, comparing text, and testing RegExp. No need to use many untrustworthy websites to do simple tasks with your data. With Smart Detection, DevToys can detect the best tool to use for the data copied to the Windows clipboard. Compact overlay lets you keep the app small and on top of other windows. Multiple instances of the app can be used at once.

Many tools are available.

- Converters
  - JSON <> YAML
  - Timestamp
  - Number Base
  - Cron Parser
- Encoders / Decoders
  - HTML
  - URL
  - Base64 Text & Image
  - GZip
  - JWT Decoder
- Formatters
  - JSON
  - SQL
  - XML
- Generators
  - Hash (MD5, SHA1, SHA256, SHA512)
  - UUID 1 and 4
  - Lorem Ipsum
  - Checksum
- Text
  - Escape / Unescape
  - Inspector & Case Converter
  - Regex Tester
  - Text Comparer
  - XML Validator
  - Markdown Preview
- Graphic
  - Color Blindness Simulator
  - Color Picker & Contrast
  - PNG / JPEG Compressor
  - Image Converter

... and more are coming!

![DevToys](/assets/screenshots/1.png)

## Rate on Microsoft Store as of 5/23/2022

![Microsoft Store rate](/assets/ms-store-rate.png)

## How to install (as an end-user)

### Prerequisite

- You need Windows 10 build 1903+ or later.

### Microsoft Store

- Search for DevToys in the Microsoft Store App or click [here](https://www.microsoft.com/store/apps/9PGCV4V3BK4W)

### Manual

- Download and extract the latest [release](https://github.com/veler/DevToys/releases).
- Double click the *.msixbundle file.
- Install.

### WinGet

- Open a PowerShell command prompt.
- Type `winget search DevToys` to search and see details about DevToys.
- Type `winget install DevToys` to install the app.

__Note:__ a Microsoft Store account is required for WinGet. We're trying to workaround it. See here https://github.com/microsoft/winget-pkgs/pull/43996

### Chocolatey

- Make sure you already have [Chocolatey](https://chocolatey.org/) installed on your computer.
- Open a PowerShell command prompt.
- Type `choco install devtoys` or visit the [chocolatey community package](https://community.chocolatey.org/packages/devtoys/).

## App Permission

DevToys works entirely offline, meaning that none of the data used by the app goes on internet. However, the app requires some other permissions in order to work correctly.

1. `Uses all system resources` - This permission is required for some tools like `PNG / JPEG Compressor` or (upcoming) `On-screen color picker / measurer`, which use a 3rd party Open-Source Win32 process like [Efficient-Compression-Tool](https://github.com/fhanau/Efficient-Compression-Tool).
   All the code requiring this permission can be found [here](https://github.com/veler/DevToys/tree/main/src/dev/impl/DevToys.OutOfProcService).

## How to run DevToys

### Using Start Menu

Open Windows start menu, type `DevToys` and press `[Enter]`.

### Using PowerShell

A cool thing about DevToys is that you can start it in command line! For this, simply open a PowerShell command prompt and type
`start devtoys:?tool={tool name}`

For example, `start devtoys:?tool=jsonyaml` will open DevToys and start on the `Json <> Yaml` tool.

Here is the list of tool name you can use:

- `base64` - Base64 Text Encoder/Decoder
- `base64img` - Base64 Image Encoder/Decoder
- `gzip` - GZip Encoder/Decoder
- `hash` - Hash Generator
- `uuid` - UUID Generator
- `loremipsum` - Lorem Ipsum Generator
- `checksum` - Checksum File
- `cronparser` - Cron Parser
- `jsonformat` Json Formatter
- `sqlformat` - SQL Formatter
- `xmlformat` - XML Formatter
- `jsonyaml` - Json <> Yaml
- `jwt` - JWT Decoder
- `colorblind` - Color Blindness Simulator
- `color` - Color Picker & Contrast
- `imgcomp` - PNG/JPEG compressor
- `imageconverter` - Image Converter
- `markdown` - Markdown Preview
- `regex` - Regular Expression Tester
- `time` - Unix Timestamp Converter
- `baseconverter` - Number Base Converter
- `string` - String Utilities
- `url` - URL Encoder/Decoder
- `html` - HTML Encoder/Decoder
- `diff` - Text Comparer
- `xmlvalidator` - XML Validator
- `escape` - Text Escape / Unescape
- `settings` - Settings

## Contribute

See [CONTRIBUTING](CONTRIBUTING.md)

## Privacy Policy

See [PRIVACY POLICY](PRIVACY-POLICY.md)

## Third-Party Softwares

See [ThirdPartyNotices](THIRD-PARTY-NOTICES.md)

## License

See [LICENSE](LICENSE.md)

### A few words regarding the license

DevToys is using a license that permits redistribution of the app as trialware or shareware without changes. However, the authors [Etienne BAUDOUX](https://github.com/veler) and [BenjaminT](https://github.com/btiteux) would prefer you not. If you believe you have a strong reason to do so, kindly reach out to discuss with us first.

## Special Thanks

### Code contributors

![Alt](https://repobeats.axiom.co/api/embed/31f4c9d2d532ef010e6171dc501f976c1481d799.svg "Repobeats analytics image")

<a href="https://github.com/veler/devtoys/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=veler/devtoys" />
</a>

### Designers

[Jakub](https://github.com/AlurDesign)
