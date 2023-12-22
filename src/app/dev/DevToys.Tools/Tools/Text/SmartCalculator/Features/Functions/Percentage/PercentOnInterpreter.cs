using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Functions.Percentage;

[Export(typeof(IFunctionInterpreter))]
[Name("percentage.percentOn")]
[Culture(SupportedCultures.English)]
internal sealed class PercentOnInterpreter : IFunctionInterpreter
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
        detectedData[0].IsOfSubtype("percentage");
        detectedData[1].IsOfType("numeric");
        var percentageData = (INumericData)detectedData[0];
        var numericData = (INumericData)detectedData[1];

        // x + (% of x)
        return Task.FromResult(
            ArithmeticAndRelationOperationService.PerformAlgebraOperation(
                numericData, BinaryOperatorType.Addition, percentageData));
    }
}
