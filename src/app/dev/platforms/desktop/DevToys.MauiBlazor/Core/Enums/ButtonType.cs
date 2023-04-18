namespace DevToys.MauiBlazor.Core.Enums;
public class ButtonType
{
    public static readonly ButtonType Button = new(nameof(Button).ToLowerInvariant(), nameof(Button));

    public static readonly ButtonType Submit = new(nameof(Submit).ToLowerInvariant(), nameof(Submit));

    public static readonly ButtonType Reset = new(nameof(Reset).ToLowerInvariant(), nameof(Reset));

    public static readonly List<ButtonType> All = new() { Button, Submit, Reset };

    public string Code { get; }

    public string Name { get; }

    protected ButtonType(string code, string name)
    {
        Guard.IsNotNullOrWhiteSpace(code);
        Guard.IsNotNullOrWhiteSpace(name);
        Code = code;
        Name = name;
    }

    public static ButtonType FindByCode(string code)
    {
        Guard.IsNotNullOrWhiteSpace(code, nameof(code));

        foreach (ButtonType buttonType in All)
        {
            if (buttonType.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return buttonType;
            }
        }
        return Button;
    }
}
