namespace DevToys.MauiBlazor.Core.Enums;

public class Utils
{
    public static readonly Utils True = new(nameof(True).ToLowerInvariant(), nameof(True));

    public static readonly Utils False = new(nameof(False).ToLowerInvariant(), nameof(False));

    public string Code { get; }

    public string Name { get; }

    protected Utils(string code, string name)
    {
        Guard.IsNotNullOrWhiteSpace(code);
        Guard.IsNotNullOrWhiteSpace(name);
        Code = code;
        Name = name;
    }
}

