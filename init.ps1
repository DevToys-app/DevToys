function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

Function Get-MsBuildPath($useVsPreview) {
    if (-not (Test-Path "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"))
    {
        Throw "Unable to find 'vswhere.exe'. Is Visual Studio installed?"
    }

    $path = ""
    try {
        if ($useVsPreview)
        {
            $path = & "${env:ProgramFiles(x86)}\microsoft visual studio\installer\vswhere.exe" -latest -prerelease -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1
        }
        else
        {
            $path = & "${env:ProgramFiles(x86)}\microsoft visual studio\installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1
        }
    }
    catch {
    }

    if (!$path)
    {
        Write-Error "Unable to find MSBuild. Try importing './.vsconfig' into Visual Studio Installer."
    }

    return $path
}

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

# Install .NET
ExecSafe { & $PSScriptRoot\tools\Install-DotNet.ps1 -RootFolder $PSScriptRoot }

# Restore NuGet solution dependencies
Write-Host "Restoring all dependencies"
Get-ChildItem $PSScriptRoot\src\ -rec |? { $_.FullName.EndsWith('.sln') } |% {
    $SolutionPath = $_.FullName;
    Write-Host "Restoring packages for $($SolutionPath)..."
    ExecSafe { & $env:DOTNET_EXE restore -v:quiet $SolutionPath  }

    # If we run on Windows
    if ([System.Boolean](Get-CimInstance -ClassName Win32_OperatingSystem -ErrorAction SilentlyContinue)) {
        $MSBuildPath = Get-MsBuildPath true
        ExecSafe { & "$MSBuildPath" $SolutionPath /t:restore /p:Configuration=Release /p:platform=x86 /v:Quiet }
        ExecSafe { & "$MSBuildPath" $SolutionPath /t:restore /p:Configuration=Release /p:platform=x64 /v:Quiet }
        ExecSafe { & "$MSBuildPath" $SolutionPath /t:restore /p:Configuration=Release /p:platform=arm64 /v:Quiet }
        ExecSafe { & "$MSBuildPath" $SolutionPath /t:restore /p:Configuration=Debug /p:platform=x86 /v:Quiet }
        ExecSafe { & "$MSBuildPath" $SolutionPath /t:restore /p:Configuration=Debug /p:platform=x64 /v:Quiet }
        ExecSafe { & "$MSBuildPath" $SolutionPath /t:restore /p:Configuration=Debug /p:platform=arm64 /v:Quiet }
    }
}

Write-Host "Done."
Write-Output "---------------------------------------"

# Restore Monaco Editor
Write-Host "Restoring Monaco Editor"
ExecSafe { & $PSScriptRoot\tools\Restore-MonacoEditor.ps1 -RootFolder $PSScriptRoot }

Write-Host "Done."
Write-Output "---------------------------------------"