using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DevToys.Blazor.BuiltInTools.ExtensionsManager;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Blazor.BuiltInTools;

public class ExtensionInstallationManagerTests
{
    private static readonly Assembly assembly = Assembly.GetExecutingAssembly();
    private readonly string _assemblyDirectory = Path.GetDirectoryName(assembly.Location);

    [Fact]
    public async Task InstallExtensionAsyncShouldReturnFailedResultWhenPackageIsInvalid()
    {
        string packageDirectory = Path.Combine(_assemblyDirectory, "Packages");
        try
        {
            LoggingExtensions.LoggerFactory ??= LoggerFactory.Create(builder => { });

            Directory.CreateDirectory(packageDirectory);
            string filePath = Path.Combine("TestData", "cve-invalid-package.nupkg");
            ExtensionInstallationManager.PreferredExtensionInstallationFolder = packageDirectory;
            ExtensionInstallationManager.ExtensionInstallationFolders = [packageDirectory];
            SandboxedFileReader fileReader = GetSandboxedFileReaderFromFilePath(filePath);

            ExtensionInstallationResult result = await ExtensionInstallationManager.InstallExtensionAsync(fileReader);

            result.Should().NotBeNull();
            result.AlreadyInstalled.Should().BeFalse();
            result.NuspecReader.Should().NotBeNull();
            result.ExtensionInstallationPath.Should().BeNullOrEmpty();
            result.HasSucceeded.Should().BeFalse();
            result.ErrorMessage.Should().Be("Security violation: Extension 'DevToys.PoC.ZipSlip' contains path traversal sequence");
        }
        finally
        {
            Directory.Delete(packageDirectory, true);
        }
    }

    [Fact]
    public async Task InstallExtensionAsyncShouldReturnSucceededResultWhenPackageIsValid()
    {
        string packageDirectory = Path.Combine(_assemblyDirectory, "Packages");
        try
        {
            LoggingExtensions.LoggerFactory ??= LoggerFactory.Create(builder => { });
            Directory.CreateDirectory(packageDirectory);

            string filePath = Path.Combine("TestData", "valid-package.nupkg");
            ExtensionInstallationManager.PreferredExtensionInstallationFolder = packageDirectory;
            ExtensionInstallationManager.ExtensionInstallationFolders = [packageDirectory];
            SandboxedFileReader fileReader = GetSandboxedFileReaderFromFilePath(filePath);

            ExtensionInstallationResult result = await ExtensionInstallationManager.InstallExtensionAsync(fileReader);

            result.Should().NotBeNull();
            result.AlreadyInstalled.Should().BeFalse();
            result.NuspecReader.Should().NotBeNull();
            result.ExtensionInstallationPath.Should().NotBeNullOrEmpty();
            result.HasSucceeded.Should().BeTrue();
            result.ErrorMessage.Should().BeNullOrEmpty();
        }
        finally
        {
            Directory.Delete(packageDirectory, true);
        }
    }

    private SandboxedFileReader GetSandboxedFileReaderFromFilePath(string filePath)
    {
        FileInfo fileInfo = GetFile(filePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException("Unable to find the indicated file.", fileInfo.FullName);
        }

        return SandboxedFileReader.FromFileInfo(fileInfo);
    }

    private FileInfo GetFile(string filePath)
    {
        try
        {
            string resourcePath = Path.Combine(_assemblyDirectory, filePath);
            return new(resourcePath);
        }
        catch
        {
            throw new FileNotFoundException(filePath);
        }
    }
}
