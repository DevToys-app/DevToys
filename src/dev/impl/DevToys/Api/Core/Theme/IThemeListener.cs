#nullable enable

using System;

namespace DevToys.Api.Core.Theme
{
    public interface IThemeListener
    {
        /// <summary>
        /// Gets the current Windows theme.
        /// </summary>
        AppTheme CurrentSystemTheme { get; }

        /// <summary>
        /// Gets the current app theme.
        /// </summary>
        AppTheme CurrentAppTheme { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the current theme is high contrast.
        /// </summary>
        bool IsHighContrast { get; }

        /// <summary>
        /// Raised when the theme has changed.
        /// </summary>
        event EventHandler? ThemeChanged;

        /// <summary>
        /// Change the color theme of the app based on <see cref="CurrentSystemTheme"/> and <see cref="CurrentAppTheme"/>.
        /// </summary>
        void ApplyDesiredColorTheme();
    }
}
