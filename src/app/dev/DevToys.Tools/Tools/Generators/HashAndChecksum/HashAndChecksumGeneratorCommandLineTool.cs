using System.Security.Authentication;
using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Generators.HashAndChecksum;

[Export(typeof(ICommandLineTool))]
[Name("HashAndChecksumGenerator")]
[CommandName(
    Name = "checksum",
    Alias = "hash",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Generators.HashAndChecksum.HashAndChecksumGenerator",
    DescriptionResourceName = nameof(HashAndChecksumGenerator.Description))]
internal sealed class HashAndChecksumGeneratorCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(HashAndChecksumGenerator.InputOptionDescription))]
    internal OneOf<string, FileInfo>? Input { get; set; }

    [CommandLineOption(
        Name = "algorithm",
        Alias = "a",
        DescriptionResourceName = nameof(HashAndChecksumGenerator.HashAlgorithmOptionDescription))]
    internal HashAlgorithmType HashAlgorithm { get; set; } = HashAlgorithmType.Md5;

    [CommandLineOption(
        Name = "uppercase",
        Alias = "u",
        DescriptionResourceName = nameof(HashAndChecksumGenerator.UppercaseOptionDescription))]
    internal bool Uppercase { get; set; } = false;

    [CommandLineOption(
        Name = "hmac",
        Alias = "m",
        DescriptionResourceName = nameof(HashAndChecksumGenerator.HmacSecretOptionDescription))]
    internal string? HmacSecretKey { get; set; }

    [CommandLineOption(
        Name = "checksum",
        Alias = "c",
        DescriptionResourceName = nameof(HashAndChecksumGenerator.ChecksumVerificationOptionDescription))]
    internal OneOf<FileInfo, string>? ChecksumVerification { get; set; }

    [CommandLineOption(
        Name = "silent",
        Alias = "s",
        DescriptionResourceName = nameof(HashAndChecksumGenerator.SilentOptionDescription))]
    internal bool Silent { get; set; } = false;

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!Input.HasValue || HashAlgorithm == HashAlgorithmType.None)
        {
            return -1;
        }

        try
        {
            ResultInfo<Stream> streamResult = await Input.Value.GetStreamAsync(_fileStorage, cancellationToken);
            using Stream inputStream = streamResult.Data;

            if (inputStream == null || inputStream == Stream.Null)
            {
                Console.Error.WriteLine(HashAndChecksumGenerator.InvalidInputOrFileCommand);
                return -1;
            }

            string? fileHashString = null;
            ConsoleProgressBar? progressBar = Silent ? null : new ConsoleProgressBar();
            using (progressBar)
            {
                fileHashString
                    = await HashingHelper.ComputeHashAsync(
                        HashAlgorithm,
                        inputStream,
                        HmacSecretKey,
                        hashProgress =>
                        {
                            progressBar?.Report(hashProgress.GetPercentage());
                        },
                        cancellationToken);
            }

            if (ChecksumVerification.HasValue)
            {
                ResultInfo<string> checksum = await ChecksumVerification.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
                if (checksum.HasSucceeded && string.Equals(checksum.Data, fileHashString, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(HashAndChecksumGenerator.ChecksumVerificationSucceeded);
                    return 0;
                }
                else
                {
                    Console.WriteLine(HashAndChecksumGenerator.ChecksumVerificationFailed);
                    return -1;
                }
            }

            if (!Uppercase)
            {
                Console.WriteLine(fileHashString.ToLowerInvariant());
            }
            else
            {
                Console.WriteLine(fileHashString);
            }

            return 0;
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
        {
            return -1;
        }
        catch
        {
            // TODO: Log exception.
            return -1;
        }
    }
}
