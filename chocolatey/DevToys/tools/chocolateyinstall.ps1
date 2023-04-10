$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url64      = 'https://github.com/veler/DevToys/releases/download/v1.0.11.0/64360VelerSoftware.DevToys_1.0.11.0_neutral_._j80j2txgjg9dj.Msixbundle'
$checksum64 = 'f119e2348dd384d94e62e4c22df7cc6e9fa8ddd7b289a1d1c44b391e7a4e5dcb'
$WindowsVersion  = [Environment]::OSVersion.Version;
$InstallDir = Split-Path $MyInvocation.MyCommand.Definition;
$AppxPackageName = "64360VelerSoftware.DevToys"

if ($WindowsVersion.Major -lt "10") {
  throw "This package requires Windows 10.";
};

#The .msixbundle format is not supported on Windows 10 version 1709 and 1803. https://docs.microsoft.com/en-us/windows/msix/msix-1709-and-1803-support
$IsCorrectBuild=[Environment]::OSVersion.Version.Build;
if ($IsCorrectBuild -lt "18362") {
  throw "This package requires at least Windows 10 version 1903/OS build 18362.x.";
};

$PackageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  url64bit      = $url64

  softwareName  = 'DevToys'

  checksum64    = $checksum64
  checksumType64= 'sha256'

  fileFullPath = Join-Path $InstallDir $([IO.Path]::GetFileName($Url64))
}

Get-ChocolateyWebFile @PackageArgs

if ((Get-AppxPackage -name $AppxPackageName).Version -Match $Version) {
  if($env:ChocolateyForce) {
    #You can't install the same version of an appx package, you need to remove it first
    Write-Host Removing already installed version first. ;
    Get-AppxPackage -Name $AppxPackageName | Remove-AppxPackage;
  } else {
    Write-Host The $Version version of the package is already installed. If you want to reinstall use --force ;
    return;
  };
};

Add-AppxPackage -Path $PackageArgs.fileFullPath
