namespace DevToys.Blazor.Components;

public sealed class InfoBarSeverity
{
    internal static readonly InfoBarSeverity Error = new("error");
    internal static readonly InfoBarSeverity Informational = new("informational");
    internal static readonly InfoBarSeverity Success = new("success");
    internal static readonly InfoBarSeverity Warning = new("warning");

    private InfoBarSeverity(string className)
    {
        Guard.IsNotNullOrWhiteSpace(className);
        Class = className;
    }

    public string Class { get; }
}
