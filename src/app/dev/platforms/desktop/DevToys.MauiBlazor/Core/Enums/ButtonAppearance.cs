using static Microsoft.Maui.ApplicationModel.Permissions;

namespace DevToys.MauiBlazor.Core.Enums;

public class ButtonAppearance
{
    public static readonly ButtonAppearance Neutral = new(nameof(Neutral).ToLowerInvariant(), nameof(Neutral));

    public static readonly ButtonAppearance Accent = new(nameof(Accent).ToLowerInvariant(), nameof(Accent));

    public static readonly ButtonAppearance Hypertext = new(nameof(Hypertext).ToLowerInvariant(), nameof(Hypertext));

    public static readonly ButtonAppearance Lightweight = new(nameof(Lightweight).ToLowerInvariant(), nameof(Lightweight));

    public static readonly ButtonAppearance Outline = new(nameof(Outline).ToLowerInvariant(), nameof(Outline));

    public static readonly ButtonAppearance Stealth = new(nameof(Stealth).ToLowerInvariant(), nameof(Stealth));

    public static readonly ButtonAppearance Filled = new(nameof(Filled).ToLowerInvariant(), nameof(Filled));

    public static readonly List<ButtonAppearance> All = new() { Neutral, Accent, Hypertext,
        Lightweight, Outline, Stealth, Filled };

    public string Code { get; }

    public string Name { get; }

    protected ButtonAppearance(string code, string name)
    {
        Guard.IsNotNullOrWhiteSpace(code);
        Guard.IsNotNullOrWhiteSpace(name);
        Code = code;
        Name = name;
    }

    public static ButtonAppearance FindByCode(string code)
    {
        Guard.IsNotNullOrWhiteSpace(code, nameof(code));

        foreach (ButtonAppearance appearance in All)
        {
            if (appearance.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return appearance;
            }
        }
        return Neutral;
    }
}
