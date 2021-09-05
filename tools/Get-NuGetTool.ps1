<#
.SYNOPSIS
    Downloads the NuGet.exe tool and returns the path to it.
.PARAMETER NuGetVersion
    The version of the NuGet tool to acquire.
#>
Param(
    [Parameter()]
    [string]$NuGetVersion='5.2.0'
)

# Value is documented here: https://msdn.microsoft.com/en-us/library/windows/desktop/ms723207(v=vs.85).aspx
$YesToAllOnAnyDialogs = 16

function Expand-ZIPFile($file, $destination) {
    if (!(Test-Path $destination)) { $null = mkdir $destination }
    $shell = new-object -com shell.application
    $zip = $shell.NameSpace((Resolve-Path $file).Path)
    foreach ($item in $zip.items()) {
        $shell.Namespace((Resolve-Path $destination).Path).copyhere($item, $YesToAllOnAnyDialogs) | Out-Null
    }
}

$binaryToolsBasePath = "$env:localappdata\Temp"
$binaryToolsPath = "$binaryToolsBasePath\$NuGetVersion"
if (!(Test-Path $binaryToolsPath)) { $null = mkdir $binaryToolsPath }
$nugetPath = "$binaryToolsPath\nuget.exe"

# Ignore the nuget.exe that came in the bundle and use the version we want to use.
if (!(Test-Path $nugetPath)) {
    Write-Host "Downloading nuget.exe $NuGetVersion..." -ForegroundColor Yellow
    (New-Object System.Net.WebClient).DownloadFile("https://dist.nuget.org/win-x86-commandline/v$NuGetVersion/NuGet.exe", $nugetPath)
}

$nugetPath
