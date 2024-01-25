using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.EncodersDecoders.Certificate;

[Export(typeof(ICommandLineTool))]
[Name("CertificateDecoder")]
[CommandName(
    Name = "certificate",
    Alias = "cert",
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Certificate.CertificateDecoder",
    DescriptionResourceName = nameof(CertificateDecoder.Description))]
internal sealed class CertificateDecoderCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(CertificateDecoder.InputOptionDescription))]
    internal OneOf<FileInfo, string>? Input { get; set; }

    [CommandLineOption(
        Name = "pwd",
        Alias = "p",
        DescriptionResourceName = nameof(CertificateDecoder.InputOptionDescription))]
    internal string? Password { get; set; }

    [CommandLineOption(
        Name = "outputFile",
        Alias = "o",
        DescriptionResourceName = nameof(CertificateDecoder.OutputFileOptionDescription))]
    private FileInfo? OutputFile { get; set; }

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!Input.HasValue)
        {
            Console.Error.WriteLine(CertificateDecoder.InvalidInputOrFileCommand);
            return -1;
        }

        ResultInfo<string> result
            = await Input.Value.Match(
                async inputFile =>
                {
                    if (!_fileStorage.FileExists(inputFile.FullName))
                    {
                        return new ResultInfo<string>("", false);
                    }

                    using Stream fileStream = _fileStorage.OpenReadFile(inputFile.FullName);
                    using var memStream = new MemoryStream();
                    await fileStream.CopyToAsync(memStream, cancellationToken);
                    byte[] bytes = memStream.ToArray();

                    string rawCertificateString = CertificateHelper.GetRawCertificateString(bytes);

                    return new ResultInfo<string>(rawCertificateString, true);
                },
                inputString => Task.FromResult(new ResultInfo<string>(inputString, true)));

        if (!result.HasSucceeded)
        {
            Console.Error.WriteLine(CertificateDecoder.InputFileNotFound);
            return -1;
        }

        Guard.IsNotNull(result.Data);

        string? decoded = null;
        bool success = false;
        if (!string.IsNullOrWhiteSpace(result.Data))
        {
            success = CertificateHelper.TryDecodeCertificate(
                logger,
                result.Data,
                Password,
                out decoded);
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (success)
        {
            await FileHelper.WriteOutputAsync(decoded ?? string.Empty, OutputFile, cancellationToken);
            return 0;
        }
        else
        {
            Console.Error.WriteLine(decoded);
            return -1;
        }
    }
}
