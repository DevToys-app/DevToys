namespace DevToys.Api;

/// <summary>
/// A circular component that indicates the progress of an operation.
/// </summary>
public interface IUIProgressRing : IUIProgressBar
{
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Value = {{{nameof(Value)}}}")]
internal sealed class UIProgressRing : UIProgressBar, IUIProgressRing
{
    internal UIProgressRing(string? id)
        : base(id)
    {
    }
}

public static partial class GUI
{
    /// <summary>
    /// Create a circular component that indicates the progress of an operation.
    /// </summary>
    public static IUIProgressRing ProgressRing()
    {
        return ProgressRing(null);
    }

    /// <summary>
    /// Create a circular component that indicates the progress of an operation.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIProgressRing ProgressRing(string? id)
    {
        return new UIProgressRing(id);
    }
}
