namespace DevToys.UI.Framework.Controls;

/// <summary>
/// Provides a way to explicitly request a visual state.
/// </summary>
public interface IVisualStateListener
{
    /// <summary>
    /// Asks the implementing class to apply the given <paramref name="visualStateName"/>.
    /// </summary>
    void SetVisualState(string visualStateName);
}
