[CmdletBinding()]
Param(
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string]$RootFolder
)

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }

if (-not (Test-Path -Path "$RootFolder\\.gitignore"))
{
    Write-Error "Please run this script from the repository's root folder"
    return
}

# ------------------------
$temp_dir_name = ".temp"
$tools_dir = "$RootFolder\tools"

Push-Location $tools_dir

# Reference to Monaco Version to Use in the Package
$monaco_version = Get-Content "monaco-editor-version-number.txt" -First 1
$monaco_tgz_url = "https://registry.npmjs.org/monaco-editor/-/monaco-editor-$monaco_version.tgz"

# Remove Old Dependency
Remove-Item "..\src\app\dev\DevToys.MonacoEditor\monaco-editor" -Force -Recurse -ErrorAction SilentlyContinue

# Clean-up Temp Dir, if already exist
Remove-Item $temp_dir_name -Force -Recurse -ErrorAction SilentlyContinue

# Create Temp Directory and Output
New-Item -Name $temp_dir_name -ItemType Directory | Out-Null
New-Item -Name "..\src\app\dev\DevToys.MonacoEditor\monaco-editor" -ItemType Directory | Out-Null

Write-Host "Downloading Monaco v.$monaco_version"

[Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls"
Invoke-WebRequest -Uri $monaco_tgz_url -OutFile ".\$temp_dir_name\monaco.tgz"

Write-Host "Extracting..."

mkdir "$tools_dir\$temp_dir_name\monaco"
tar -zxf "$tools_dir\$temp_dir_name\monaco.tgz" -C "$tools_dir\$temp_dir_name\monaco"

Copy-Item -Path ".\$temp_dir_name\monaco\package\*" -Destination "..\src\app\dev\DevToys.MonacoEditor\monaco-editor" -Recurse

# Clean-up Temp Dir
Remove-Item $temp_dir_name -Force -Recurse -ErrorAction SilentlyContinue

Pop-Location