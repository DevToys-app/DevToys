Function Get-MsBuildPath {
    if (-not (Test-Path "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"))
    {
        Throw "Unable to find 'vswhere.exe'. Is Visual Studio installed?"
    }

    $path = ""
    try {
        $path = & "${env:ProgramFiles}\microsoft visual studio\installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1
    }
    catch {
        $path = & "${env:ProgramFiles(x86)}\microsoft visual studio\installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1
    }

    if (!$path)
    {
        Write-Error "Unable to find MSBuild. Try importing './.vsconfig' into Visual Studio Installer."
    }

    return $path
}

Function Restore-PackagesUnder($searchRoot) {
    # Restore VS solution dependencies
    Get-ChildItem $searchRoot -rec |? { $_.FullName.EndsWith('.sln') } |% {
        Write-Host "Restoring packages for $($_.FullName)..." -ForegroundColor $HeaderColor
        & "$toolsPath\Restore-NuGetPackages.ps1" -Path $_.FullName -Verbosity $nugetVerbosity -MSBuildPath $MSBuildPath
    }
}

Function Restore-MonacoEditor() {
    # Restore Monaco Editor
    Write-Host "Restoring Monaco Editor..." -ForegroundColor $HeaderColor
    & "$toolsPath\Restore-MonacoEditor.ps1"
}

Push-Location $PSScriptRoot
try {
    $EnvVarsSet = $false
    $HeaderColor = 'Green'
    $toolsPath = "$PSScriptRoot\tools"
    $nugetVerbosity = 'minimal'
    $MSBuildPath = Get-MsBuildPath
    if ($VerbosePreference -eq 'continue') { $nugetVerbosity = 'Detailed' }

    Restore-PackagesUnder "$PSScriptRoot\src"

    Restore-MonacoEditor

    Write-Host "Successfully restored all dependencies" -ForegroundColor Yellow
}
catch {
    Write-Error $error[0]
    exit $lastexitcode
}
finally {
    Pop-Location
}
