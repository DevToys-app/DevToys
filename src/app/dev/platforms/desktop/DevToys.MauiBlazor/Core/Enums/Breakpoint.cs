namespace DevToys.MauiBlazor.Core.Enums;

public class Breakpoint
{
    public static Breakpoint Small = new(nameof(Small).ToLowerInvariant(), nameof(Small), 0);

    public static Breakpoint Medium = new(nameof(Medium).ToLowerInvariant(), nameof(Medium), 1);

    public static Breakpoint Wide = new(nameof(Wide).ToLowerInvariant(), nameof(Wide), 2);

    public string Code { get; }

    public string Name { get; }

    public int Value { get; }

    protected Breakpoint(string code, string name, int value)
    {
        Guard.IsNotNullOrWhiteSpace(code, nameof(code));
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));
        Code = code;
        Name = name;
        Value = value;
    }

    public static IEnumerable<Breakpoint> GetAll()
        => new List<Breakpoint> { Small, Medium, Wide };

    public static Breakpoint FindByCode(string code)
    {
        Guard.IsNotNullOrWhiteSpace(code, nameof(code));

        Breakpoint? found = GetAll().SingleOrDefault(breakpoint => breakpoint.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (found is null)
            return Small;
        return found;
    }
}
