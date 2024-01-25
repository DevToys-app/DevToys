namespace DevToys.Api;

/// <summary>
/// A component that can be used to display or edit passwords on a single line.
/// </summary>
public interface IUIPasswordInput : IUISingleLineTextInput
{
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Text = {{{nameof(Text)}}}")]
internal class UIPasswordInput : UISingleLineTextInput, IUIPasswordInput
{

    internal UIPasswordInput(string? id)
        : base(id)
    {
    }
}

public static partial class GUI
{
    /// <summary>
    /// A component that can be used to display or edit passwords on a single line.
    /// </summary>
    public static IUIPasswordInput PasswordInput()
    {
        return PasswordInput(null);
    }

    /// <summary>
    /// A component that can be used to display or edit passwords on a single line.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIPasswordInput PasswordInput(string? id)
    {
        return new UIPasswordInput(id);
    }
}
