namespace DevToys.MauiBlazor.Core.Enums;

public class ButtonAppearance
{
    public static ButtonAppearance Neutral = new(nameof(Neutral).ToLowerInvariant(), nameof(Neutral));

    public static ButtonAppearance Accent = new(nameof(Accent).ToLowerInvariant(), nameof(Accent));

    public static ButtonAppearance Hypertext = new(nameof(Hypertext).ToLowerInvariant(), nameof(Hypertext));

    public static ButtonAppearance Lightweight = new(nameof(Lightweight).ToLowerInvariant(), nameof(Lightweight));

    public static ButtonAppearance Outline = new(nameof(Outline).ToLowerInvariant(), nameof(Outline));

    public static ButtonAppearance Stealth = new(nameof(Stealth).ToLowerInvariant(), nameof(Stealth));

    public static ButtonAppearance Filled = new(nameof(Filled).ToLowerInvariant(), nameof(Filled));

    public string Code { get; }

    public string Name { get; }

    protected ButtonAppearance(string code, string name)
    {
        Guard.IsNotNullOrWhiteSpace(code, nameof(code));
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));
        Code = code;
        Name = name;
    }

    public static IEnumerable<ButtonAppearance> GetAll()
        => new List<ButtonAppearance> { Neutral, Accent, Hypertext };

    public static ButtonAppearance FindByCode(string code)
    {
        Guard.IsNotNullOrWhiteSpace(code, nameof(code));

        ButtonAppearance? found = GetAll().SingleOrDefault(button => button.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (found is null)
            return Neutral;
        return found;
    }
}
