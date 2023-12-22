using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Functions.Percentage;

[Export(typeof(IFunctionInterpreter))]
[Name("percentage.isWhatPercentOn")]
[Culture(SupportedCultures.English)]
internal sealed class IsWhatPercentOnInterpreter : IFunctionInterpreter
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

        var one
            = new DecimalData(
                secondNumericData.LineTextIncludingLineBreak,
                secondNumericData.StartInLine,
                secondNumericData.EndInLine,
                1);

        // ((250 / 62.5) - 1) = 3
        // so 250 represents an increase of 300% on 62.5.

        IData? dividedNumbers
            = ArithmeticAndRelationOperationService.PerformAlgebraOperation(
                    firstNumericData,
                    BinaryOperatorType.Division,
                    secondNumericData);

        var percentageData
            = ArithmeticAndRelationOperationService.PerformAlgebraOperation(
                dividedNumbers,
                BinaryOperatorType.Subtraction,
                one) as INumericData;

        if (percentageData is not null)
        {
            return Task.FromResult(
                (IData?)new PercentageData(
                    percentageData.LineTextIncludingLineBreak,
                    percentageData.StartInLine,
                    percentageData.EndInLine,
                    percentageData.NumericValueInCurrentUnit));
        }

        return Task.FromResult<IData?>(null);
    }
}
