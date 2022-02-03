$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url64      = 'https://github.com/veler/DevToys/releases/download/v1.0.2.0/64360VelerSoftware.DevToys_1.0.2.0_neutral___j80j2txgjg9dj.Msixbundle'
$checksum64 = '287e0fe143b25069df2d43eb8fd7dedeff9abc6f3fe7799ccb218a180d2068a3'
$WindowsVersion  = [Environment]::OSVersion.Version;
$InstallDir = Split-Path $MyInvocation.MyCommand.Definition;

if ($WindowsVersion.Major -ne "10") {
  throw "This package requires Windows 10.";
};

#The .msixbundle format is not supported on Windows 10 version 1709 and 1803. https://docs.microsoft.com/en-us/windows/msix/msix-1709-and-1803-support
$IsCorrectBuild=[Environment]::OSVersion.Version.Build;
if ($IsCorrectBuild -lt "18362") {
  throw "This package requires at least Windows 10 version 1903/OS build 18362.x.";
};

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'MSI'
  url64bit      = $url64

  softwareName  = 'DevToys'

  checksumType  = 'sha256'
  checksum64    = $checksum64
  checksumType64= 'sha256'

  # MSI
  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`"" # ALLUSERS=1 DISABLEDESKTOPSHORTCUT=1 ADDDESKTOPICON=0 ADDSTARTMENU=0
  validExitCodes= @(0, 3010, 1641)

  fileFullPath = Join-Path $InstallDir $([IO.Path]::GetFileName($Url64))
}

Get-ChocolateyWebFile @PackageArgs

if ((Get-AppxPackage -name $PackageArgs.packageName).Version -Match $Version) {
  if($env:ChocolateyForce) {
    # you can't install the same version of an appx package, you need to remove it first
    Write-Host Removing already installed version first. ;
    Get-AppxPackage -Name $PackageArgs.packageName | Remove-AppxPackage;
  } else {
    Write-Host The $Version version of the package is already installed. If you want to reinstall use --force ;
    return;
  };
};

Add-AppxPackage -Path $PackageArgs.fileFullPath
