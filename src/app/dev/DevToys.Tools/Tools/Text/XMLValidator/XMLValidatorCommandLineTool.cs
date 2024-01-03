using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Text.XMLValidator;

[Export(typeof(ICommandLineTool))]
[Name("XMLValidator")]
[CommandName(
    Name = "xmlvalidator",
    Alias = "xsd",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.XMLValidator.XMLValidator",
    DescriptionResourceName = nameof(XMLValidator.Description))]
internal sealed class XMLValidatorCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "xsd",
        Alias = "s",
        IsRequired = true,
        DescriptionResourceName = nameof(XMLValidator.XsdOptionDescription))]
    private OneOf<FileInfo, string>? XsdFile { get; set; }

    [CommandLineOption(
        Name = "xml",
        Alias = "x",
        IsRequired = true,
        DescriptionResourceName = nameof(XMLValidator.XmlOptionDescription))]
    private OneOf<FileInfo, string>? XmlFile { get; set; }

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!XsdFile.HasValue)
        {
            Console.Error.WriteLine(XMLValidator.InvalidXsdOrFileCommand);
            return -1;
        }

        ResultInfo<string> xsd = await XsdFile.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!xsd.HasSucceeded)
        {
            Console.Error.WriteLine(XMLValidator.XsdFileNotFound);
            return -1;
        }

        if (!XmlFile.HasValue)
        {
            Console.Error.WriteLine(XMLValidator.InvalidXmlOrFileCommand);
            return -1;
        }

        ResultInfo<string> xml = await XmlFile.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!xml.HasSucceeded)
        {
            Console.Error.WriteLine(XMLValidator.XmlFileNotFound);
            return -1;
        }

        ResultInfo<string, XmlValidatorResultSeverity> result = XsdHelper.ValidateXmlAgainstXsd(xsd.Data, xml.Data, logger, cancellationToken);

        switch (result.Severity)
        {
            case XmlValidatorResultSeverity.Success:
                return 0;

            case XmlValidatorResultSeverity.Warning:
                Console.Error.WriteLine(result.Data);
                return -1;

            case XmlValidatorResultSeverity.Error:
                string errorDescription;
                if (string.IsNullOrWhiteSpace(result.Data))
                {
                    errorDescription = XMLValidator.XmlInvalidMessage;
                }
                else
                {
                    errorDescription = result.Data;
                }

                Console.Error.WriteLine(errorDescription);
                return -1;

            default:
                throw new NotImplementedException();
        }
    }
}
