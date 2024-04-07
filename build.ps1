[CmdletBinding()]
Param(
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$BuildArguments
)

function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

# Install .NET
ExecSafe { & $PSScriptRoot\tools\Install-DotNet.ps1 -RootFolder $PSScriptRoot }

# Build the builder project.
Write-Host "Building the pipeline"
$BuildProjectFile = "$PSScriptRoot\src\build\_build.csproj"

ExecSafe { & $env:DOTNET_EXE build $BuildProjectFile /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet }
Write-Host "Done."
Write-Output "---------------------------------------"

# Run the builder
ExecSafe { & $env:DOTNET_EXE run --project $BuildProjectFile --no-build -- $BuildArguments }
Write-Host "Done."
Write-Output "---------------------------------------"
