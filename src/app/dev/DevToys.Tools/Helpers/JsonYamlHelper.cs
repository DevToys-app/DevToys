using DevToys.Tools.Models;
using DevToys.Tools.Tools.Converters.JsonYaml;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers;

internal static class JsonYamlHelper
{
    public static async ValueTask<ResultInfo<string>> ConvertAsync(
        string input,
        JsonToYamlConversion conversion,
        Indentation indentation,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        ResultInfo<string> conversionResult;
        switch (conversion)
        {
            case JsonToYamlConversion.JsonToYaml:
                conversionResult = YamlHelper.ConvertFromJson(input, indentation, logger, cancellationToken);
                if (!conversionResult.HasSucceeded && string.IsNullOrWhiteSpace(conversionResult.Data))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return new(JsonYamlConverter.InvalidJson, false);
                }
                break;
            case JsonToYamlConversion.YamlToJson:
                conversionResult = JsonHelper.ConvertFromYaml(input, indentation, logger, cancellationToken);
                if (!conversionResult.HasSucceeded && string.IsNullOrWhiteSpace(conversionResult.Data))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return new(JsonYamlConverter.InvalidYaml, false);
                }
                break;
            default:
                throw new NotSupportedException();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return conversionResult;
    }
}
