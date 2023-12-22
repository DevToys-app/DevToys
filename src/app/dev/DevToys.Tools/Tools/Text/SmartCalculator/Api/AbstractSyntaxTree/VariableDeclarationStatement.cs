using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;

public sealed class VariableDeclarationStatement : Statement
{
    public string VariableName { get; }

    public LinkedToken VariableNameStart { get; }

    public LinkedToken VariableNameEnd { get; }

    public Expression AssignedValue { get; }

    public VariableDeclarationStatement(LinkedToken variableNameStart, LinkedToken variableNameEnd, string variableName, Expression assignedValue)
        : base(variableNameStart, assignedValue.LastToken)
    {
        Guard.IsNotNull(variableNameStart);
        Guard.IsNotNull(variableNameEnd);
        Guard.IsNotNull(assignedValue);
        Guard.IsNotNullOrWhiteSpace(variableName);
        VariableNameStart = variableNameStart;
        VariableNameEnd = variableNameEnd;
        AssignedValue = assignedValue;
        VariableName = variableName;
    }

    public override string ToString()
    {
        return $"Variable $({VariableName}) = {AssignedValue}";
    }
}
