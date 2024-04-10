function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

# Install .NET
ExecSafe { & $PSScriptRoot\tools\Install-DotNet.ps1 -RootFolder $PSScriptRoot }

# Restore NuGet solution dependencies
Write-Host "Restoring all dependencies"
Get-ChildItem $PSScriptRoot\src\ -rec |? { $_.FullName.EndsWith('DevToys-Windows.sln') } |% {
    $SolutionPath = $_.FullName;
    Write-Host "Restoring packages for $($SolutionPath)..."
    ExecSafe { & $env:DOTNET_EXE restore -p:RestoreNpm=true -p:PublishReadyToRun=true -v:quiet $SolutionPath  }
}

Write-Host "Done."
Write-Output "---------------------------------------"

# Restore Monaco Editor
Write-Host "Restoring Monaco Editor"
ExecSafe { & $PSScriptRoot\tools\Restore-MonacoEditor.ps1 -RootFolder $PSScriptRoot }

Write-Host "Done."
Write-Output "---------------------------------------"