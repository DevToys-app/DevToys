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
  <a style="text-decoration:none" href="https://etienne-baudoux.visualstudio.com/Side%20projects/_build/latest?definitionId=15&branchName=main">
    <img src="https://etienne-baudoux.visualstudio.com/Side%20projects/_apis/build/status/DevToys?branchName=main" alt="Build Status" />
  </a>
  <a style="text-decoration:none" href="https://github.com/veler/DevToys/releases">
    <img src="https://img.shields.io/github/release/veler/devtoys.svg?label=Latest%20version" alt="Latest version" />
  </a>
  <a style="text-decoration:none" href="https://www.microsoft.com/store/apps/9PGCV4V3BK4W">
    <img src="https://img.shields.io/badge/Microsoft%20Store-Download-green" alt="Store link" />
  </a>
</p>

## Introduction

DevToys helps in daily tasks like formatting JSON, comparing text, testing RegExp. No need to use many untruthful websites to do simple tasks with your data. With Smart Detection, DevToys is able to detect the best tool that can treat the data you copied in the clipboard of your Windows. Compact overlay lets you keep the app in small and on top of other windows. Multiple instances of the app can be used at once.

Many tools are available.
- Converters
  - Json <> Yaml
  - Number Base
- Encoders / Decoders
  - HTML
  - URL
  - Base64
  - JWT Decoder
- Formatters
  - Json
- Generators
  - Hash (MD5, SHA1, SHA256, SHA512)
  - UUID 1 and 4
- Text
  - Inspector & Case Converter
  - Regex Tester
  - Text Comparer
  - Markdown Preview
- Graphic
  - PNG / JPEG Compressor

... and more are coming!

![DevToys](/assets/screenshots/1.png)

## How to install (as an end-user)

### Prerequisite
- You need Windows 10 build 1903+ or later.

### Microsoft Store
- Search for DevToys in the Microsoft Store App or click [here](https://www.microsoft.com/store/apps/9PGCV4V3BK4W)

### WinGet
- Open a PowerShell command prompt.
- Type `winget search DevToys` to search and see details about DevToys.
- Type `winget install DevToys` to install the app.

### Manual

- Download and extract the latest [release](https://github.com/veler/DevToys/releases).
- Install the certificate in `Trusted Root`.
- Double click the *.msixbundle file.

## How to run DevToys

### Using Start Menu
Open Windows start menu, type `DevToys` and press `[Enter]`.

### Using PowerShell

A cool thing about DevToys is that you can start it in command line! For this, simply open a PowerShell command prompt and type
`start devtoys:?tool={tool name}`

For example, `start devtoys:?tool=jsonyaml` will open DevToys and start on the `Json <> Yaml` tool.

Here is the list of tool name you can use:
- `base64` - Base64 Encoder/Decoder
- `hash` - Hash Generator
- `uuid` - UUID Generator
- `jsonformat` Json Formatter
- `jsonyaml` - Json <> Yaml
- `jwt` - JWT Decoder
- `imgcomp` - PNG/JPEG compressor
- `markdown` - Markdown Preview
- `regex` - Regular Expression Tester
- `baseconverter` - Number Base Converter
- `string` - String Utilities
- `url` - URL Encoder/Decoder
- `html` - HTML Encoder/Decoder
- `diff` - Text Comparer
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

DevToys is using a license that permits redistribution of the app as trialware or shareware without changes. However, the authors @veler and @btiteux would prefer you not. If you believe you have a strong reason to do so, kindly reach out to discuss with us first.

## Special Thanks

* Code contributors: [BenjaminT](https://github.com/btiteux)
* Designer: [Jakub](https://github.com/AlurDesign)
