using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Functions.Percentage;

[Export(typeof(IFunctionInterpreter))]
[Name("percentage.isPercentOfWhat")]
[Culture(SupportedCultures.English)]
internal sealed class IsPercentOfWhatInterpreter : IFunctionInterpreter
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

        PercentageData percentageData;
        INumericData numericData;

        if (detectedData[0].IsOfSubtype("percentage")
            && detectedData[1].IsOfType("numeric"))
        {
            percentageData = (PercentageData)detectedData[0];
            numericData = (INumericData)detectedData[1];
        }
        else if (detectedData[0].IsOfType("numeric")
            && detectedData[1].IsOfSubtype("percentage"))
        {
            numericData = (INumericData)detectedData[0];
            percentageData = (PercentageData)detectedData[1];
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException();
            return null;
        }

        // 250 * (1 / 0.25)
        // 250 * 4
        // = 1000
        // so 250 is 25% of 1000.
        // also, 250 is 25% off 1000.
        return Task.FromResult(
            ArithmeticAndRelationOperationService.PerformAlgebraOperation(
                numericData,
                BinaryOperatorType.Multiply,
                numericData.CreateFromCurrentUnit(1 / percentageData.NumericValueInCurrentUnit)));
    }
}
