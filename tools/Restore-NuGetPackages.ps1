<#
.SYNOPSIS
    Restores NuGet packages.
.PARAMETER Path
    The path of the solution, directory, packages.config or project.json file to restore packages from.
    If not specified, the current directory is used.
.PARAMETER MSBuildPath
    The path to directory that contains MSBuild.exe that should be used for package restore.
    Optional, but may be necessary if you have multiple installs of VS2017 and the 'guessed'
    installation is the wrong version or has an insufficient set of workloads installed.
.PARAMETER Verbosity
    The verbosity for the nuget restore command.
#>
[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='Low')]
Param(
    [Parameter(Position=1)]
    [string]$Path=(Get-Location),
    [Parameter()]
    [ValidateSet('Quiet','Minimal','Normal','Detailed','Diagnostic')]
    [string]$Verbosity='normal',
    [Parameter()]
    [string]$MSBuildPath,
    [Parameter()]
    [string]$NuGetVersion='5.2.0'
)

if ($Path.EndsWith('.sln')) {
    Write-Verbose "Restoring NuGet packages for $Path with verbosity $Verbosity"
    Invoke-Expression '& "$MSBuildPath" $Path /t:restore /p:Configuration=Release /p:platform=x86 /v:$Verbosity'
    Invoke-Expression '& "$MSBuildPath" $Path /t:restore /p:Configuration=Release /p:platform=x64 /v:$Verbosity'
    Invoke-Expression '& "$MSBuildPath" $Path /t:restore /p:Configuration=Release /p:platform=arm /v:$Verbosity'
    Invoke-Expression '& "$MSBuildPath" $Path /t:restore /p:Configuration=Debug /p:platform=x86 /v:$Verbosity'
    Invoke-Expression '& "$MSBuildPath" $Path /t:restore /p:Configuration=Debug /p:platform=x64 /v:$Verbosity'
    Invoke-Expression '& "$MSBuildPath" $Path /t:restore /p:Configuration=Debug /p:platform=arm /v:$Verbosity'
    if ($lastexitcode -ne 0) { throw }

    $slnDir = Split-Path -Path $Path -Parent
    $packagesConfig = Join-Path $slnDir "\nuget.config"
    if (Test-Path $packagesConfig) {
        & "$PSScriptRoot\Restore-NuGetPackages.ps1" -Path $packagesConfig -Verbosity $Verbosity -MSBuildPath $MSBuildPath -NuGetVersion $NuGetVersion
    }
}
