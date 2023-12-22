using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Functions.General;

[Export(typeof(IFunctionInterpreter))]
[Name("general.remainder")]
[Culture(SupportedCultures.English)]
internal sealed class RemainderInterpreter : IFunctionInterpreter
{
    public Task<IData?> InterpretFunctionAsync(
        string culture,
        FunctionDefinition functionDefinition,
        IReadOnlyList<IData> detectedData,
        CancellationToken cancellationToken)
    {
        Guard.HasSizeEqualTo(detectedData, 2);
        detectedData[0].IsOfType("numeric");
        detectedData[1].IsOfType("numeric");
        var firstNumber = (INumericData)detectedData[0];
        var secondNumber = (INumericData)detectedData[1];

        double first = firstNumber.NumericValueInStandardUnit;
        double second = secondNumber.NumericValueInStandardUnit;
        double result = first % second;

        return Task.FromResult((IData?)secondNumber.CreateFromStandardUnit(result).MergeDataLocations(firstNumber));
    }
}
