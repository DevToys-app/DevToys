namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

public sealed class ParserAndInterpreterMetadata : CultureCodeMetadata, IOrderableMetadata
{
    public IReadOnlyList<string> Before { get; }

    public IReadOnlyList<string> After { get; }

    public string InternalComponentName { get; }

    public ParserAndInterpreterMetadata(IDictionary<string, object> metadata)
        : base(metadata)
    {
        Before = metadata.GetValueOrDefault(nameof(OrderAttribute.Before)) as IReadOnlyList<string> ?? Array.Empty<string>();
        After = metadata.GetValueOrDefault(nameof(OrderAttribute.After)) as IReadOnlyList<string> ?? Array.Empty<string>();
        InternalComponentName = metadata.GetValueOrDefault(nameof(NameAttribute.InternalComponentName)) as string ?? string.Empty;
        Guard.IsNotEmpty(InternalComponentName);

        if (Before.Count > 0)
        {
            var before = new List<string>();
            for (int i = 0; i < Before.Count; i++)
            {
                if (!string.IsNullOrEmpty(Before[i]))
                    before.Add(Before[i]);
            }

            Before = before;
        }

        if (After.Count > 0)
        {
            var after = new List<string>();
            for (int i = 0; i < After.Count; i++)
            {
                if (!string.IsNullOrEmpty(After[i]))
                    after.Add(After[i]);
            }

            After = after;
        }
    }
}
