namespace DevToys.Api;

public record UIHoverTooltip
{
    public string Word { get; }

    public string Description { get; }

    public UIHoverTooltip(string word, string description)
    {
        Word = word;
        Description = description;
    }
}
