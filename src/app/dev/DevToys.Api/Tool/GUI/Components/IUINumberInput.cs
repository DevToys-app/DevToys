namespace DevToys.Api;

/// <summary>
/// A component that can be used to display or edit numbers on a single line.
/// </summary>
public interface IUINumberInput : IUISingleLineTextInput
{
    /// <summary>
    /// Gets the minimum value possible. Default is <see cref="int.MinValue"/>.
    /// </summary>
    double Min { get; }

    /// <summary>
    /// Gets the maximum value possible. Default is <see cref="int.MaxValue"/>.
    /// </summary>
    double Max { get; }

    /// <summary>
    /// Gets the interval between legal numbers. Default is 1.
    /// </summary>
    double Step { get; }

    /// <summary>
    /// Gets the value of the input.
    /// </summary>
    double Value { get; }

    /// <summary>
    /// Raised when <see cref="Min"/> is changed.
    /// </summary>
    event EventHandler? MinChanged;

    /// <summary>
    /// Raised when <see cref="Max"/> is changed.
    /// </summary>
    event EventHandler? MaxChanged;

    /// <summary>
    /// Raised when <see cref="Step"/> is changed.
    /// </summary>
    event EventHandler? StepChanged;

    /// <summary>
    /// Raised when <see cref="Value"/> is changed.
    /// </summary>
    event EventHandler? ValueChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}, Min = {{{nameof(Min)}}}, Max = {{{nameof(Max)}}}, Step = {{{nameof(Step)}}}")]
internal class UINumberInput : UISingleLineTextInput, IUINumberInput
{
    private double _min = int.MinValue;
    private double _max = int.MaxValue;
    private double _step = 1;

    internal UINumberInput(string? id)
        : base(id)
    {
    }

    public double Min
    {
        get => _min;
        internal set => SetPropertyValue(ref _min, value, MinChanged);
    }

    public double Max
    {
        get => _max;
        internal set => SetPropertyValue(ref _max, value, MaxChanged);
    }

    public double Step
    {
        get => _step;
        internal set => SetPropertyValue(ref _step, value, StepChanged);
    }

    public double Value
    {
        get
        {
            if (double.TryParse(Text, out double value))
            {
                return Math.Min(Math.Max(value, Min), Max);
            }

            return Math.Min(Math.Max(0, Min), Max);
        }
    }

    public event EventHandler? MinChanged;
    public event EventHandler? MaxChanged;
    public event EventHandler? StepChanged;
    public event EventHandler? ValueChanged;

    internal void OnValueChanged()
    {
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}

public static partial class GUI
{
    /// <summary>
    /// A component that can be used to display or edit numbers on a single line.
    /// </summary>
    public static IUINumberInput NumberInput()
    {
        return NumberInput(null);
    }

    /// <summary>
    /// A component that can be used to display or edit numbers on a single line.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="min">The minimum value possible.</param>
    /// <param name="max">The maximum value possible.</param>
    /// <param name="step">The interval between legal numbers.</param>
    public static IUINumberInput NumberInput(string? id, double min = int.MinValue, double max = int.MaxValue, double step = 1)
    {
        return new UINumberInput(id)
        {
            Min = min,
            Max = max,
            Step = step
        };
    }

    /// <summary>
    /// Sets the minimum value possible.
    /// </summary>
    public static IUINumberInput Minimum(this IUINumberInput element, double minimum)
    {
        ((UINumberInput)element).Min = minimum;
        return element;
    }

    /// <summary>
    /// Sets the maximum value possible.
    /// </summary>
    public static IUINumberInput Maximum(this IUINumberInput element, double maximum)
    {
        ((UINumberInput)element).Max = maximum;
        return element;
    }

    /// <summary>
    /// Sets the interval between legal numbers.
    /// </summary>
    public static IUINumberInput Step(this IUINumberInput element, double step)
    {
        ((UINumberInput)element).Step = step;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUISingleLineTextInput.Text"/> property of the <see cref="IUINumberInput"/>.
    /// </summary>
    public static IUINumberInput Value(this IUINumberInput element, double value)
    {
        double safeValue = Math.Min(Math.Max(value, element.Min), element.Max);
        element.Text(safeValue.ToString());
        return element;
    }

    /// <summary>
    /// Sets the action to run when the value changed.
    /// </summary>
    public static IUINumberInput OnValueChanged(this IUINumberInput element, Func<double, ValueTask> actionOnValueChanged)
    {
        var numberInput = (UINumberInput)element;
        numberInput.ActionOnTextChanged
            = (value) =>
            {
                numberInput.OnValueChanged();
                return actionOnValueChanged?.Invoke(numberInput.Value) ?? ValueTask.CompletedTask;
            };

        return element;
    }

    /// <summary>
    /// Sets the action to run when the value changed.
    /// </summary>
    public static IUINumberInput OnValueChanged(this IUINumberInput element, Action<double> actionOnValueChanged)
    {
        var numberInput = (UINumberInput)element;
        numberInput.ActionOnTextChanged
            = (value) =>
            {
                numberInput.OnValueChanged();
                actionOnValueChanged?.Invoke(numberInput.Value);
                return ValueTask.CompletedTask;
            };

        return element;
    }
}
