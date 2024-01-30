namespace DevToys.Api;

/// <summary>
/// A component that indicates the progress of an operation.
/// </summary>
public interface IUIProgressBar : IUIElement
{
    /// <summary>
    /// Gets the current position of the progress bar between 0 and 100.
    /// </summary>
    double Value { get; }

    /// <summary>
    /// Gets whether the progress bar shows actual values or generic, continuous progress feedback.
    /// </summary>
    bool IsIndeterminate { get; }

    /// <summary>
    /// Raised when <see cref="Value"/> is changed.
    /// </summary>
    event EventHandler? ValueChanged;

    /// <summary>
    /// Raised when <see cref="Value"/> is changing asynchronously.
    /// </summary>
    event EventHandler<double>? ValueChangingAsynchronously;

    /// <summary>
    /// Raised when <see cref="IsIndeterminate"/> is changed.
    /// </summary>
    event EventHandler? IsIndeterminateChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Value = {{{nameof(Value)}}}")]
internal class UIProgressBar : UIElement, IUIProgressBar, IDisposable
{
    private readonly ThrottledProgress<double> _asyncProgressReporter;
    private double _value;
    private bool _isIndeterminate;

    internal UIProgressBar(string? id)
        : base(id)
    {
        _asyncProgressReporter
            = new ThrottledProgress<double>(
                OnReportAsync,
                TimeSpan.FromMilliseconds(10));
    }

    /// <inheritdoc/>
    public double Value
    {
        get => _value;
        internal set => SetPropertyValue(ref _value, value, ValueChanged);
    }

    /// <inheritdoc/>
    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        internal set => SetPropertyValue(ref _isIndeterminate, value, IsIndeterminateChanged);
    }

    /// <inheritdoc/>
    public event EventHandler? ValueChanged;

    /// <inheritdoc/>
    public event EventHandler<double>? ValueChangingAsynchronously;

    /// <inheritdoc/>
    public event EventHandler? IsIndeterminateChanged;

    /// <inheritdoc/>
    public void Dispose()
    {
        _asyncProgressReporter.Flush();
    }

    internal ValueTask ReportAsync(double percentage)
    {
        ((IProgress<double>)_asyncProgressReporter).Report(percentage);
        return ValueTask.CompletedTask;
    }

    private void OnReportAsync(double percentage)
    {
        ValueChangingAsynchronously?.Invoke(this, percentage);
    }
}

public static partial class GUI
{
    /// <summary>
    /// Create a component that indicates the progress of an operation.
    /// </summary>
    /// <returns>The created <see cref="IUIProgressBar"/> instance.</returns>
    public static IUIProgressBar ProgressBar()
    {
        return ProgressBar(null);
    }

    /// <summary>
    /// Create a component that indicates the progress of an operation.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <returns>The created <see cref="IUIProgressBar"/> instance.</returns>
    public static IUIProgressBar ProgressBar(string? id)
    {
        return new UIProgressBar(id);
    }

    /// <summary>
    /// Synchronously set the <see cref="IUIProgressBar.Value"/> property.
    /// </summary>
    /// <remarks>It is highly recommended to call this method when being on the UI thread.</remarks>
    /// <param name="element">The <see cref="IUIProgressBar"/> instance.</param>
    /// <param name="percentage">A value between 0 and 100.</param>
    public static T Progress<T>(this T element, double percentage) where T : IUIProgressBar
    {
        Guard.IsBetweenOrEqualTo(percentage, 0, 100);
        if (element is UIProgressBar progressBar)
        {
            progressBar.Value = percentage;
        }
        return element;
    }

    /// <summary>
    /// Asynchronously set the <see cref="IUIProgressBar.Value"/> property. This method makes sure to update the
    /// progress bar's value without blocking the caller.
    /// </summary>
    /// <remarks>It is highly recommended to call this method when being off the UI thread.</remarks>
    /// <param name="element">The <see cref="IUIProgressBar"/> instance.</param>
    /// <param name="percentage">A value between 0 and 100.</param>
    public static ValueTask ProgressAsync<T>(this T element, double percentage) where T : IUIProgressBar
    {
        Guard.IsBetweenOrEqualTo(percentage, 0, 100);
        if (element is UIProgressBar progressBar)
        {
            return progressBar.ReportAsync(percentage);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Indicates the progress bar should a generic, continuous progress feedback.
    /// </summary>
    /// <param name="element">The <see cref="IUIProgressBar"/> instance.</param>
    public static T StartIndeterminateProgress<T>(this T element) where T : IUIProgressBar
    {
        if (element is UIProgressBar progressBar)
        {
            progressBar.IsIndeterminate = true;
        }

        return element;
    }

    /// <summary>
    /// Indicates the progress bar should show actual values.
    /// </summary>
    /// <param name="element">The <see cref="IUIProgressBar"/> instance.</param>
    public static T StopIndeterminateProgress<T>(this T element) where T : IUIProgressBar
    {
        if (element is UIProgressBar progressBar)
        {
            progressBar.IsIndeterminate = false;
        }

        return element;
    }
}
