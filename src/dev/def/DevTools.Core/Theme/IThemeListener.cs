using System;

namespace DevTools.Core.Theme
{
    public interface IThemeListener
    {
        /// <summary>
        /// Gets the current theme.
        /// </summary>
        AppTheme CurrentTheme { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the current theme is high contrast.
        /// </summary>
        bool IsHighContrast { get; }

        /// <summary>
        /// Raised when the theme has changed.
        /// </summary>
        event EventHandler? ThemeChanged;
    }
}
