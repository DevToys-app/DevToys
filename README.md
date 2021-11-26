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

DevToys helps in everyday tasks like formatting JSON, comparing text, testing RegExp. No need to use many untruthful websites to do simple tasks with your data. With Smart Detection, DevToys is able to detect the best tool that can treat the data you copied in the clipboard of your Windows. Compact overlay lets you keep the app in small and on top of other windows. Multiple instances of the app can be used at once.

Many tools are available.
- Base 64 Encoder/Decoder
- Hash Generator (MD5, SHA1, SHA256, SHA512)
- Guid Generator
- JWT Decoder
- Json Formatter
- RegExp Tester
- Text Comparer
- Json <> Yaml converter
- PNG/JPG image compressor
- String Utilities
- URL Encoder/Decoder
- HTML Encoder/Decoder
- Markdown Preview

... and more are coming!

![DevToys](/assets/screenshots/1.png)

## How to install (as an end-user)

### Prerequisite
- You need Windows 10 build 1903+ or later.

### Microsoft Store
- Search for DevToys in the Microsoft Store App or click [here](https://www.microsoft.com/store/apps/9PGCV4V3BK4W)

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
- `guid` - Guid Generator
- `jsonformat` Json Formatter
- `jsonyaml` - Json <> Yaml
- `jwt` - JWT Decoder
- `imgcomp` - PNG/JPG compressor
- `markdown` - Markdown Preview
- `regex` - Regular Expression Tester
- `string` - String Utilities
- `url` - URL Encoder/Decoder
- `html` - HTML Encoder/Decoder
- `diff` - Text Comparer
- `settings` - Settings

## Please, avoid selling this app as yours

I don't care if you copy the source code to use in your project, but please avoid simply changing the name and selling as your work. 
That's not why I'm sharing the source code, at all.

## Contribute
See [CONTRIBUTING](CONTRIBUTING.md)

## Privacy Policy
See [PRIVACY POLICY](PRIVACY-POLICY.md)

## Third-Party Softwares

See [ThirdPartyNotices](THIRD-PARTY-NOTICES.md)

## License

See [LICENSE](LICENSE.md)

## Special Thanks

* Code contributors: [BenjaminT](https://github.com/btiteux)
* Designer: [Jakub](https://github.com/AlurDesign)
