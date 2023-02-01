using System.Globalization;
using Windows.Globalization;

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using Microsoft.UI.Xaml;
#endif

namespace DevToys.UI;

public sealed class LanguageManager
{
    private static LanguageManager? languageManager;

    /// <summary>
    /// Gets an instance of <see cref="LanguageManager"/>.
    /// </summary>
    public static LanguageManager Instance => languageManager ??= new LanguageManager();

    /// <summary>
    /// Gets if the text must be written from left to right or from right to left.
    /// </summary>
    public FlowDirection FlowDirection { get; private set; }

    /// <summary>
    /// Gets the list of available languages in the app.
    /// </summary>
    public List<LanguageDefinition> AvailableLanguages { get; }

    public LanguageManager()
    {
        AvailableLanguages = new List<LanguageDefinition>
        {
            new LanguageDefinition() // default language
        };
        IReadOnlyList<string> supportedLanguageIdentifiers = ApplicationLanguages.ManifestLanguages;
        for (int i = 0; i < supportedLanguageIdentifiers.Count; i++)
        {
            AvailableLanguages.Add(new LanguageDefinition(supportedLanguageIdentifiers[i]));
        }
    }

    /// <summary>
    /// Change the current culture of the application
    /// </summary>
    public void SetCurrentCulture(LanguageDefinition language)
    {
        CultureInfo.DefaultThreadCurrentCulture = language.Culture;
        CultureInfo.DefaultThreadCurrentUICulture = language.Culture;
        ApplicationLanguages.PrimaryLanguageOverride = language.Identifier;

        if (language.Culture.TextInfo.IsRightToLeft)
        {
            FlowDirection = FlowDirection.RightToLeft;
        }
        else
        {
            FlowDirection = FlowDirection.LeftToRight;
        }
    }
}
