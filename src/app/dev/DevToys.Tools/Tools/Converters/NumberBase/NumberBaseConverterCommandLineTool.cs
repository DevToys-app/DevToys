using DevToys.Tools.Helpers;
using DevToys.Tools.Models.NumberBase;
using Microsoft.Extensions.Logging;
using Decimal = DevToys.Tools.Models.NumberBase.Decimal;

namespace DevToys.Tools.Tools.Converters.NumberBase;

[Export(typeof(ICommandLineTool))]
[Name("NumberBaseConverter")]
[CommandName(
    Name = "numberbase",
    Alias = "nb",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.NumberBase.NumberBaseConverter",
    DescriptionResourceName = nameof(NumberBaseConverter.Description))]
internal sealed class NumberBaseConverterCommandLineTool : ICommandLineTool
{
    internal enum WellKnownNumberBases
    {
        Decimal,
        Octal,
        Hexadecimal,
        Binary
    }

    [CommandLineOption(
               Name = "input",
               Alias = "i",
               IsRequired = true,
               DescriptionResourceName = nameof(NumberBaseConverter.InputOptionDescription))]
    internal string? Input { get; set; }

    [CommandLineOption(
               Name = "base",
               Alias = "b",
               DescriptionResourceName = nameof(NumberBaseConverter.InputBaseOptionDescription))]
    internal WellKnownNumberBases InputBase { get; set; } = WellKnownNumberBases.Decimal;

    [CommandLineOption(
               Name = "outputBase",
               Alias = "o",
               DescriptionResourceName = nameof(NumberBaseConverter.OutputBaseOptionDescription))]
    internal WellKnownNumberBases OutputBase { get; set; } = WellKnownNumberBases.Hexadecimal;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrEmpty(Input);

        INumberBaseDefinition<long> inputBaseDefinition
            = InputBase switch
            {
                WellKnownNumberBases.Binary => Binary.Instance,
                WellKnownNumberBases.Octal => Octal.Instance,
                WellKnownNumberBases.Decimal => Decimal.Instance,
                WellKnownNumberBases.Hexadecimal => Hexadecimal.Instance,
                _ => throw new NotSupportedException()
            };

        INumberBaseDefinition<long> outputBaseDefinition
            = OutputBase switch
            {
                WellKnownNumberBases.Binary => Binary.Instance,
                WellKnownNumberBases.Octal => Octal.Instance,
                WellKnownNumberBases.Decimal => Decimal.Instance,
                WellKnownNumberBases.Hexadecimal => Hexadecimal.Instance,
                _ => throw new NotSupportedException()
            };

        bool succeeded
            = NumberBaseHelper.TryConvertNumberBase(
                Input,
                inputBaseDefinition,
                outputBaseDefinition,
                format: true,
                out string? result,
                out string? error);

        if (succeeded)
        {
            Console.WriteLine(result);
            return ValueTask.FromResult(0);
        }
        else
        {
            Console.Error.WriteLine(error);
            return ValueTask.FromResult(-1);
        }
    }
}
