namespace DevTools.Core.Theme
{
    /// <summary>
    /// Specifies a UI theme that should be used for individual UIElement parts of an app UI.
    /// </summary>
    public enum AppTheme
    {
        /// <summary>
        /// Use the Application.RequestedTheme value for the element. This is the default.
        /// </summary>
        Default,

        /// <summary>
        /// Use the **Light** default theme.
        /// </summary>
        Light,

        /// <summary>
        /// Use the **Dark** default theme.
        /// </summary>
        Dark
    }
}
