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
$destination_dir = "..\src\app\dev\DevToys.Blazor\wwwroot\lib\monaco-editor"

Push-Location $tools_dir

# Reference to Monaco Version to Use in the Package
$monaco_version = Get-Content "monaco-editor-version-number.txt" -First 1
$monaco_tgz_url = "https://registry.npmjs.org/monaco-editor/-/monaco-editor-$monaco_version.tgz"

# Remove Old Dependency
Remove-Item $destination_dir -Force -Recurse -ErrorAction SilentlyContinue

# Clean-up Temp Dir, if already exist
Remove-Item $temp_dir_name -Force -Recurse -ErrorAction SilentlyContinue

# Create Temp Directory and Output
New-Item -Name $temp_dir_name -ItemType Directory | Out-Null
New-Item -Name $destination_dir -ItemType Directory | Out-Null

Write-Host "Downloading Monaco v.$monaco_version"

[Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls"
Invoke-WebRequest -Uri $monaco_tgz_url -OutFile ".\$temp_dir_name\monaco.tgz"

Write-Host "Extracting..."

New-Item -Path "$tools_dir\$temp_dir_name\monaco" -ItemType Directory | out-null
tar -zxf "$tools_dir\$temp_dir_name\monaco.tgz" -C "$tools_dir\$temp_dir_name\monaco"

New-Item -Path "$destination_dir\min" -ItemType Directory | out-null
Copy-Item -Path ".\$temp_dir_name\monaco\package\min\*" -Destination "$destination_dir\min" -Recurse

# Clean-up Temp Dir
Remove-Item $temp_dir_name -Force -Recurse -ErrorAction SilentlyContinue

Pop-Location