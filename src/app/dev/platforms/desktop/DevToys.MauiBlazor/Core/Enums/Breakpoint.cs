namespace DevToys.MauiBlazor.Core.Enums;

public class Breakpoint
{
    public static readonly Breakpoint Small = new(nameof(Small).ToLowerInvariant(), nameof(Small), 0);

    public static readonly Breakpoint Medium = new(nameof(Medium).ToLowerInvariant(), nameof(Medium), 1);

    public static readonly Breakpoint Wide = new(nameof(Wide).ToLowerInvariant(), nameof(Wide), 2);

    public static readonly List<Breakpoint> All = new() { Small, Medium, Wide };

    public string Code { get; }

    public string Name { get; }

    public int Value { get; }

    protected Breakpoint(string code, string name, int value)
    {
        Guard.IsNotNullOrWhiteSpace(code);
        Guard.IsNotNullOrWhiteSpace(name);
        Code = code;
        Name = name;
        Value = value;
    }

    public static Breakpoint FindByCode(string code)
    {
        Guard.IsNotNullOrWhiteSpace(code, nameof(code));

        foreach (Breakpoint breakPoint in All)
        {
            if (breakPoint.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return breakPoint;
            }
        }
        return Small;
    }
}
