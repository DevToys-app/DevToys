using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Functions.Percentage;

[Export(typeof(IFunctionInterpreter))]
[Name("percentage.isWhatPercentOf")]
[Culture(SupportedCultures.English)]
internal sealed class IsWhatPercentOfInterpreter : IFunctionInterpreter
{
    [Import]
    public IArithmeticAndRelationOperationService ArithmeticAndRelationOperationService { get; set; } = null!;

    public Task<IData?> InterpretFunctionAsync(
        string culture,
        FunctionDefinition functionDefinition,
        IReadOnlyList<IData> detectedData,
        CancellationToken cancellationToken)
    {
        Guard.HasSizeEqualTo(detectedData, 2);
        detectedData[0].IsOfSubtype("numeric");
        detectedData[1].IsOfType("numeric");
        var firstNumericData = (INumericData)detectedData[0];
        var secondNumericData = (INumericData)detectedData[1];

        // ((62.5 * 100) / 250) / 100 = 0.25
        // So 62.5 represents 25% of 250.

        IData? firstNumericDataMultipliedPerOneHundred
            = ArithmeticAndRelationOperationService.PerformAlgebraOperation(
                firstNumericData,
                BinaryOperatorType.Multiply,
                new DecimalData(
                    firstNumericData.LineTextIncludingLineBreak,
                    firstNumericData.StartInLine,
                    firstNumericData.EndInLine,
                    100));

        var percentageData
            = ArithmeticAndRelationOperationService.PerformAlgebraOperation(
                firstNumericDataMultipliedPerOneHundred,
                BinaryOperatorType.Division,
                secondNumericData) as INumericData;

        if (percentageData is not null)
        {
            return Task.FromResult(
                (IData?)new PercentageData(
                    percentageData.LineTextIncludingLineBreak,
                    percentageData.StartInLine,
                    percentageData.EndInLine,
                    percentageData.NumericValueInCurrentUnit / 100));
        }

        return Task.FromResult<IData?>(null);
    }
}
