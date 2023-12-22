using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api;

/// <summary>
/// A service for performing binary and algebra operation on data.
/// </summary>
public interface IArithmeticAndRelationOperationService
{
    /// <summary>
    /// Perform operation on two data.
    /// </summary>
    IData? PerformOperation(IData? leftData, BinaryOperatorType binaryOperatorType, IData? rightData);

    /// <summary>
    /// Perform binary operation on two data.
    /// </summary>
    IData? PerformBinaryOperation(IData? leftData, BinaryOperatorType binaryOperatorType, IData? rightData);

    /// <summary>
    /// Perform algebra operation on two data.
    /// </summary>
    /// <param name="binaryOperatorType">Addition, Substraction, Multiplication or Division</param>
    IData? PerformAlgebraOperation(IData? leftData, BinaryOperatorType binaryOperatorType, IData? rightData);
}
