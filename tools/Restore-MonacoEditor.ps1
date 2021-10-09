# Run this script using PowerShell to download and 
# install dependencies into the project directory before building.

# Reference to Monaco Version to Use in the Package
$monaco_version = "0.20.0"

# ------------------------
$monaco_tgz_url = "https://registry.npmjs.org/monaco-editor/-/monaco-editor-$monaco_version.tgz"
$sharp_zip_lib_url = "https://github.com/icsharpcode/SharpZipLib/releases/download/0.86.0.518/ICSharpCode.SharpZipLib.dll"
$temp_dir_name = ".temp"

function Get-ScriptDirectory {
    Split-Path -parent $PSCommandPath
}

$script_dir = Get-ScriptDirectory

Push-Location $script_dir

function Extract-TGZ {
    Param([string]$gzArchiveName, [string] $destFolder)
    
    $inStream = [System.IO.File]::OpenRead($gzArchiveName)
    $gzipStream = New-Object ICSharpCode.SharpZipLib.GZip.GZipInputStream -ArgumentList $inStream

    $tarArchive = [ICSharpCode.SharpZipLib.Tar.TarArchive]::CreateInputTarArchive($gzipStream);
    $tarArchive.ExtractContents($destFolder);
    $tarArchive.Close()

    $gzipStream.Close()
    $inStream.Close()
}

# Remove Old Dependency
Remove-Item "..\src\dev\impl\DevToys.MonacoEditor\monaco-editor" -Force -Recurse -ErrorAction SilentlyContinue

# Create Temp Directory and Output
New-Item -Name $temp_dir_name -ItemType Directory | Out-Null
New-Item -Name "..\src\dev\impl\DevToys.MonacoEditor\monaco-editor" -ItemType Directory | Out-Null

Write-Host "Downloading SharpZipLib"
[Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls"
Invoke-WebRequest -Uri $sharp_zip_lib_url -OutFile ".\$temp_dir_name\SharpZipLib.dll"

Write-Host "Downloading Monaco"

[Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls"
Invoke-WebRequest -Uri $monaco_tgz_url -OutFile ".\$temp_dir_name\monaco.tgz"

Write-Host "Extracting..."

# Load Sharp Zip Lib so we can unpack Monaco
# Load in memory so we can delete the dll after.
[System.Reflection.Assembly]::Load([IO.File]::ReadAllBytes("$script_dir\$temp_dir_name\SharpZipLib.dll")) | Out-Null

Extract-TGZ "$script_dir\$temp_dir_name\monaco.tgz" "$script_dir\$temp_dir_name\monaco"

Copy-Item -Path ".\$temp_dir_name\monaco\package\*" -Destination "..\src\dev\impl\DevToys.MonacoEditor\monaco-editor" -Recurse

# Clean-up Temp Dir
Remove-Item $temp_dir_name -Force -Recurse -ErrorAction SilentlyContinue

Pop-Location