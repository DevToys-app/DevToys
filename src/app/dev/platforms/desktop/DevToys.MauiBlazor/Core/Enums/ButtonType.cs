namespace DevToys.MauiBlazor.Core.Enums;
public class ButtonType
{
    public static ButtonType Button = new(nameof(Button).ToLowerInvariant(), nameof(Button));

    public static ButtonType Submit = new(nameof(Submit).ToLowerInvariant(), nameof(Submit));

    public static ButtonType Reset = new(nameof(Reset).ToLowerInvariant(), nameof(Reset));

    public string Code { get; }

    public string Name { get; }

    protected ButtonType(string code, string name)
    {
        Guard.IsNotNullOrWhiteSpace(code, nameof(code));
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));
        Code = code;
        Name = name;
    }

    public static IEnumerable<ButtonType> GetAll()
        => new List<ButtonType> { Button, Submit, Reset };

    public static ButtonType FindByCode(string code)
    {
        Guard.IsNotNullOrWhiteSpace(code, nameof(code));

        ButtonType? found = GetAll().SingleOrDefault(button => button.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (found is null)
        {
            return Button;
        }
        return found;
    }
}
