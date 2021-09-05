Function Get-MsBuildPath {
    try {
        & "${env:ProgramFiles}\microsoft visual studio\installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1
    }
    catch {
        & "${env:ProgramFiles(x86)}\microsoft visual studio\installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | select-object -first 1
    }
}

Function Restore-PackagesUnder($searchRoot) {
    # Restore VS solution dependencies
    Get-ChildItem $searchRoot -rec |? { $_.FullName.EndsWith('.sln') } |% {
        Write-Host "Restoring packages for $($_.FullName)..." -ForegroundColor $HeaderColor
        & "$toolsPath\Restore-NuGetPackages.ps1" -Path $_.FullName -Verbosity $nugetVerbosity -MSBuildPath $MSBuildPath
    }

    # Restore vcxproj dependencies
    $nugetPath = & "$PSScriptRoot\tools\Get-NuGetTool.ps1"
    Get-ChildItem $searchRoot -rec |? { $_.FullName.EndsWith('.vcxproj') } |% {
        $packages = Join-Path -Path $_.PSParentPath -ChildPath "nuget.config"
        if (Test-Path $packages) {
            Write-Host "Restoring packages for $(Convert-Path $_.PSParentPath)..." -ForegroundColor $HeaderColor
            & $nugetPath restore $(Convert-Path $_.PSParentPath)
        }
    }
}

Push-Location $PSScriptRoot
try {
    $EnvVarsSet = $false
    $HeaderColor = 'Green'
    $toolsPath = "$PSScriptRoot\tools"
    $nugetVerbosity = 'minimal'
    $MSBuildPath = Get-MsBuildPath
    if ($VerbosePreference -eq 'continue') { $nugetVerbosity = 'Detailed' }

    & "$PSScriptRoot\tools\Get-NuGetTool.ps1" | Out-Null
    Restore-PackagesUnder "$PSScriptRoot\src"

    Write-Host "Successfully restored all dependencies" -ForegroundColor Yellow
}
catch {
    Write-Error $error[0]
    exit $lastexitcode
}
finally {
    Pop-Location
}
