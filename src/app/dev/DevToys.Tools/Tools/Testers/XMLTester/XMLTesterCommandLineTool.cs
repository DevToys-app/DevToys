using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Testers.XMLTester;

[Export(typeof(ICommandLineTool))]
[Name("XMLTester")]
[CommandName(
    Name = "xmltester",
    Alias = "xsd",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Testers.XMLTester.XMLTester",
    DescriptionResourceName = nameof(XMLTester.Description))]
internal sealed class XMLTesterCommandLineTool : ICommandLineTool
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    [CommandLineOption(
        Name = "xsd",
        Alias = "s",
        IsRequired = true,
        DescriptionResourceName = nameof(XMLTester.XsdOptionDescription))]
    private OneOf<FileInfo, string>? XsdFile { get; set; }

    [CommandLineOption(
        Name = "xml",
        Alias = "x",
        IsRequired = true,
        DescriptionResourceName = nameof(XMLTester.XmlOptionDescription))]
    private OneOf<FileInfo, string>? XmlFile { get; set; }

    public async ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!XsdFile.HasValue)
        {
            Console.Error.WriteLine(XMLTester.InvalidXsdOrFileCommand);
            return -1;
        }

        ResultInfo<string> xsd = await XsdFile.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!xsd.HasSucceeded)
        {
            Console.Error.WriteLine(XMLTester.XsdFileNotFound);
            return -1;
        }

        if (!XmlFile.HasValue)
        {
            Console.Error.WriteLine(XMLTester.InvalidXmlOrFileCommand);
            return -1;
        }

        ResultInfo<string> xml = await XmlFile.Value.ReadAllTextAsync(_fileStorage, cancellationToken);
        if (!xml.HasSucceeded)
        {
            Console.Error.WriteLine(XMLTester.XmlFileNotFound);
            return -1;
        }

        ResultInfo<string> result = XsdHelper.ValidateXmlAgainstXsd(xsd.Data, xml.Data, logger, cancellationToken);

        switch (result.Severity)
        {
            case ResultInfoSeverity.Success:
                return 0;

            case ResultInfoSeverity.Warning:
                Console.Error.WriteLine(result.Data);
                return -1;

            case ResultInfoSeverity.Error:
                string errorDescription;
                if (string.IsNullOrWhiteSpace(result.Data))
                {
                    errorDescription = XMLTester.XmlInvalidMessage;
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
