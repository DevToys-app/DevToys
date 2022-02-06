# DevToys Chocolatey

First of all, ```update.ps1``` script uses [Automatic Package Updater Module](https://github.com/majkinetor/au) (AU) to deal with package generation taking the binaries from the GitHub repository (releases). You can install it using different ways (the Chocolatey method is recommended).

## Creating .nupkg

- After install Automatic Package Updater Module launch ```update.ps1```.
- A .nupkg package related to the latest version published as release in GitHub will be created.
- If you already have a .nupkg created for the latest version but you would like to recreate it, you should uncomment the ```-Force``` parameter present in ```update.ps1``` script (line ```update -ChecksumFor 64 #-Force```).
- Remove **.Msixbundle** file from inside .nupkg file (it will be placed in tools folder). Note: this could be automated (to do?)

## Pushing versions to Chocolatey

If you're already -or you would like to become one- a maintainer, use the following commands to push new generated version .nupkg file to Chocolatey repository (you should already have an API key from [Chocolatey Community](https://community.chocolatey.org/), if not, please create an account):

```choco apikey --key yourapikey --source https://push.chocolatey.org/```

```choco push .\DevToys.versiongenerated.nupkg --source https://push.chocolatey.org/```

## Become a maintainer

Please, contact [rlm96](https://github.com/rlm96) or [veler](https://github.com/veler/).