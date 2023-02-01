using System.Globalization;

namespace DevToys.UI;

/// <summary>
/// Represents un language supported by the app.
/// </summary>
public class LanguageDefinition
{
    /// <summary>
    /// Unique internal name.
    /// </summary>
    public string InternalName { get; }

    /// <summary>
    /// Unique internal ID used to identify the language.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// The name of the language displayed to the user.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The culture to apply.
    /// </summary>
    public CultureInfo Culture { get; }

    public LanguageDefinition()
        : this(null)
    {
    }

    public LanguageDefinition(string? identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
#if HAS_UNO_WASM
            Culture = CultureInfo.CurrentUICulture;
#else
            Culture = new CultureInfo(Windows.System.UserProfile.GlobalizationPreferences.Languages[0]);
#endif
            DisplayName = Strings.Languages.DefaultLanguage;
            InternalName = "default";
        }
        else
        {
            Culture = new CultureInfo(identifier!);
            DisplayName = Culture.NativeName;
            InternalName = Culture.Name;
        }

        Identifier = Culture.Name;
    }
}
