using System.Globalization;
using Windows.Globalization;
using Microsoft.UI.Xaml;
using System.Resources;
using DevToys.UI.Strings;
using Windows.ApplicationModel.Resources;

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

#if HAS_UNO
        IReadOnlyList<string> supportedLanguageIdentifiers = ApplicationLanguages.ManifestLanguages;
        for (int i = 0; i < supportedLanguageIdentifiers.Count; i++)
        {
            AvailableLanguages.Add(
                new LanguageDefinition(
                    supportedLanguageIdentifiers[i]));
        }
#else
        IReadOnlyList<Windows.ApplicationModel.Resources.Core.ResourceCandidate> candidates
            = Windows.ApplicationModel.Resources.Core.ResourceManager.Current
                .MainResourceMap[$"DevToys.UI/{nameof(Languages)}/{nameof(Languages.DefaultLanguage)}"]
                .Candidates;
        for (int i = 0; i < candidates.Count; i++)
        {
            AvailableLanguages.Add(
                new LanguageDefinition(
                    candidates[i].GetQualifierValue("Language")));
        }
#endif
    }

    /// <summary>
    /// Change the current culture of the application
    /// </summary>
    public void SetCurrentCulture(LanguageDefinition language)
    {
        CultureInfo.DefaultThreadCurrentCulture = language.Culture;
        CultureInfo.DefaultThreadCurrentUICulture = language.Culture;
#if HAS_UNO
        ApplicationLanguages.PrimaryLanguageOverride = language.Identifier;
#endif

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
