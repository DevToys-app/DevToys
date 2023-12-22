using System.Runtime.CompilerServices;
using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Core;

[Export(typeof(IArithmeticAndRelationOperationService))]
internal sealed class ArithmeticAndRelationOperationService : IArithmeticAndRelationOperationService
{
    public IData? PerformOperation(IData? leftData, BinaryOperatorType binaryOperatorType, IData? rightData)
    {
        if (leftData is not null)
        {
            switch (binaryOperatorType)
            {
                case BinaryOperatorType.Equality:
                case BinaryOperatorType.NoEquality:
                case BinaryOperatorType.LessThan:
                case BinaryOperatorType.LessThanOrEqualTo:
                case BinaryOperatorType.GreaterThan:
                case BinaryOperatorType.GreaterThanOrEqualTo:
                    return PerformBinaryOperation(leftData, binaryOperatorType, rightData);

                case BinaryOperatorType.Addition:
                case BinaryOperatorType.Subtraction:
                case BinaryOperatorType.Multiply:
                case BinaryOperatorType.Division:
                    return PerformAlgebraOperation(leftData, binaryOperatorType, rightData);

                default:
                    ThrowHelper.ThrowNotSupportedException();
                    break;
            }
        }

        return null;
    }

    public IData? PerformAlgebraOperation(IData? leftData, BinaryOperatorType binaryOperatorType, IData? rightData)
    {
        if (leftData is not INumericData leftNumericData || rightData is not INumericData rightNumericData)
            return leftData;

        StandardizeNumericData(
            leftNumericData,
            binaryOperatorType,
            rightNumericData,
            out leftNumericData,
            out binaryOperatorType,
            out rightNumericData);

        if (rightNumericData is IValueRelativeToOtherData rightNumericDataValueRelativeToOtherData)
        {
            rightNumericData
                = (INumericData)leftNumericData
                .CreateFromStandardUnit(rightNumericDataValueRelativeToOtherData.GetStandardUnitValueRelativeTo(leftNumericData))
                .MergeDataLocations(rightNumericData);
        }

        INumericData result;
        switch (binaryOperatorType)
        {
            case BinaryOperatorType.Addition:
                result = leftNumericData.Add(rightNumericData);
                break;

            case BinaryOperatorType.Subtraction:
                result = leftNumericData.Substract(rightNumericData);
                break;

            case BinaryOperatorType.Multiply:
                result = leftNumericData.Multiply(rightNumericData);
                break;

            case BinaryOperatorType.Division:
                double divisor = rightNumericData.NumericValueInStandardUnit;
                if (divisor == 0)
                {
                    result
                        = new DecimalData(
                            leftData.LineTextIncludingLineBreak,
                            leftData.StartInLine,
                            leftData.EndInLine,
                            double.PositiveInfinity);
                }
                else
                {
                    result = leftNumericData.Divide(rightNumericData);
                }
                break;

            default:
                ThrowHelper.ThrowNotSupportedException();
                return null;
        }

        return result.MergeDataLocations(rightNumericData);
    }

    public IData? PerformBinaryOperation(IData? leftData, BinaryOperatorType binaryOperatorType, IData? rightData)
    {
        if (leftData is not INumericData leftNumericData || rightData is not INumericData rightNumericData)
            return leftData;

        StandardizeNumericData(
            leftNumericData,
            binaryOperatorType,
            rightNumericData,
            out leftNumericData,
            out binaryOperatorType,
            out rightNumericData);

        bool result;

        switch (binaryOperatorType)
        {
            case BinaryOperatorType.Equality:
                result
                    = leftNumericData.NumericValueInStandardUnit
                    == rightNumericData.NumericValueInStandardUnit;
                break;

            case BinaryOperatorType.NoEquality:
                result
                    = leftNumericData.NumericValueInStandardUnit
                    != rightNumericData.NumericValueInStandardUnit;
                break;

            case BinaryOperatorType.LessThan:
                result
                    = leftNumericData.NumericValueInStandardUnit
                    < rightNumericData.NumericValueInStandardUnit;
                break;

            case BinaryOperatorType.LessThanOrEqualTo:
                result
                    = leftNumericData.NumericValueInStandardUnit
                    <= rightNumericData.NumericValueInStandardUnit;
                break;

            case BinaryOperatorType.GreaterThan:
                result
                    = leftNumericData.NumericValueInStandardUnit
                    > rightNumericData.NumericValueInStandardUnit;
                break;

            case BinaryOperatorType.GreaterThanOrEqualTo:
                result
                    = leftNumericData.NumericValueInStandardUnit
                    >= rightNumericData.NumericValueInStandardUnit;
                break;

            default:
                ThrowHelper.ThrowNotSupportedException();
                return null;
        }

        return new BooleanData(
            leftData.LineTextIncludingLineBreak,
            Math.Min(leftData.StartInLine, rightData.StartInLine),
            Math.Max(leftData.EndInLine, rightData.EndInLine),
            result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StandardizeNumericData(
        INumericData leftData,
        BinaryOperatorType binaryOperatorType,
        INumericData rightData,
        out INumericData newLeftData,
        out BinaryOperatorType newBinaryOperatorType,
        out INumericData newRightData)
    {
        newLeftData = leftData;
        newBinaryOperatorType = binaryOperatorType;
        newRightData = rightData;

        bool leftIsDecimal = leftData is IDecimal;
        bool rightIsDecimal = rightData is IDecimal;
        bool leftIsPercentage = leftData is PercentageData;
        bool rightIsPercentage = rightData is PercentageData;

        // If both left and right data are regular numbers or percentages, there's nothing to do.
        if (leftIsDecimal && rightIsDecimal
            || leftIsPercentage && rightIsPercentage)
        {
            return;
        }

        // If the left data is a percentage but not the right one (for example, `25% + 3`),
        // then we switch the value so we interpret it as `3 + 25%`.
        if (leftIsPercentage && !rightIsPercentage)
        {
            if (binaryOperatorType is BinaryOperatorType.Subtraction)
            {
                // If we do something like `25% - 3km`, we should interpret it as `-3km + 25%`.
                rightData = rightData.CreateFromCurrentUnit(rightData.NumericValueInCurrentUnit * -1);
                binaryOperatorType = BinaryOperatorType.Addition;
            }

            StandardizeNumericData(
                rightData,
                binaryOperatorType,
                leftData,
                out newLeftData,
                out newBinaryOperatorType,
                out newRightData);
            return;
        }

        // If we reached this part of code, we don't expect that the left data is a percentage.
        Guard.IsFalse(leftIsPercentage);

        // If the right data is a percentage and that we're doing an addition or substraction, then
        // we interpet something like `3 + 25%` as `3 + (25% of 3)`.
        if (rightIsPercentage)
        {
            if (binaryOperatorType is BinaryOperatorType.Addition or BinaryOperatorType.Subtraction)
            {
                newRightData
                    = (INumericData)leftData
                    .CreateFromStandardUnit(leftData.NumericValueInStandardUnit * rightData.NumericValueInStandardUnit)
                    .MergeDataLocations(rightData);
                Guard.IsNotOfType<PercentageData>(leftData);
                Guard.IsNotOfType<PercentageData>(newRightData);
                return;
            }

            // If we multiply or divide, then we will later interpret something like `3 * 25%` as `3 * 0.25`.
            newRightData
                = new DecimalData(
                    rightData.LineTextIncludingLineBreak,
                    rightData.StartInLine,
                    rightData.EndInLine,
                    rightData.NumericValueInStandardUnit);
            return;
        }

        // If we reached this part of code, we don't expect that the left or right data is a percentage.
        Guard.IsFalse(leftIsPercentage);
        Guard.IsFalse(rightIsPercentage);
        Guard.IsFalse(leftIsDecimal && rightIsDecimal);

        if (binaryOperatorType is not BinaryOperatorType.Division and not BinaryOperatorType.Multiply)
        {
            if (leftIsDecimal)
            {
                // Convert the left decimal data to the same unit than the right data.
                newLeftData
                    = (INumericData)rightData
                    .CreateFromCurrentUnit(leftData.NumericValueInCurrentUnit)
                    .MergeDataLocations(leftData);
                Guard.IsNotOfType<DecimalData>(newLeftData);
                return;
            }
            else if (rightIsDecimal)
            {
                // Convert the right decimal data to the same unit than the left data.

                if (rightData is IValueRelativeToOtherData rightNumericDataValueRelativeToOtherData)
                {
                    rightData
                        = (INumericData)leftData
                        .CreateFromStandardUnit(rightNumericDataValueRelativeToOtherData.GetStandardUnitValueRelativeTo(leftData))
                        .MergeDataLocations(rightData);
                }

                newRightData
                        = (INumericData)leftData
                        .CreateFromCurrentUnit(rightData.NumericValueInCurrentUnit)
                        .MergeDataLocations(rightData);
                Guard.IsNotOfType<DecimalData>(newLeftData);
                return;
            }

            Guard.IsTrue(!leftIsDecimal && !rightIsDecimal);

            if (newLeftData is not ISupportMultipleDataTypeForArithmeticOperation
                && (!leftData.IsOfType(rightData.Type) || !leftData.IsOfSubtype(rightData.Subtype!)))
            {
                throw new IncompatibleUnitsException();
            }
        }
    }
}
