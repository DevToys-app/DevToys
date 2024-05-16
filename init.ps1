function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

# Verify LongPath is enabled
# Forked from https://github.com/dotnet/roslyn/blob/c8eecdb9563127988b3cb564a493eae9ef254a88/eng/build.ps1#L607
$regKeyProperty = Get-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem -Name "LongPathsEnabled" -ErrorAction Ignore
if (($null -eq $regKeyProperty) -or ($regKeyProperty.LongPathsEnabled -ne 1)) {
  Write-Host -ForegroundColor Yellow "Warning: LongPath is not enabled, you may experience build errors. You can avoid these by enabling LongPath. You can enable it by running `"tools/enable-long-paths.reg`""
}

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