namespace DevToys.Core.Tools.Metadata;

[DebuggerDisplay($"InternalComponentName = {{{nameof(InternalComponentName)}}}")]
public sealed class GuiToolGroupMetadata : IOrderableMetadata
{
    public string InternalComponentName { get; }

    public IReadOnlyList<string> Before { get; }

    public IReadOnlyList<string> After { get; }

    public GuiToolGroupMetadata(IDictionary<string, object> metadata)
    {
        InternalComponentName = metadata.GetValueOrDefault(nameof(NameAttribute.InternalComponentName)) as string ?? string.Empty;
        Before = metadata.GetValueOrDefault(nameof(OrderAttribute.Before)) as IReadOnlyList<string> ?? Array.Empty<string>();
        After = metadata.GetValueOrDefault(nameof(OrderAttribute.After)) as IReadOnlyList<string> ?? Array.Empty<string>();
        Guard.IsNotNullOrWhiteSpace(InternalComponentName);

        if (string.Equals(ReservedGuiToolGroupNames.AllTools, InternalComponentName, StringComparison.OrdinalIgnoreCase))
        {
            ThrowHelper.ThrowInvalidDataException($"The tool group name '{ReservedGuiToolGroupNames.AllTools}' is reserved.");
        }

        if (string.Equals(ReservedGuiToolGroupNames.FavoriteTools, InternalComponentName, StringComparison.OrdinalIgnoreCase))
        {
            ThrowHelper.ThrowInvalidDataException($"The tool group name '{ReservedGuiToolGroupNames.FavoriteTools}' is reserved.");
        }

        if (Before.Count > 0)
        {
            var before = new List<string>();
            for (int i = 0; i < Before.Count; i++)
            {
                if (!string.IsNullOrEmpty(Before[i]))
                {
                    before.Add(Before[i]);
                }
            }

            Before = before;
        }

        if (After.Count > 0)
        {
            var after = new List<string>();
            for (int i = 0; i < After.Count; i++)
            {
                if (!string.IsNullOrEmpty(After[i]))
                {
                    after.Add(After[i]);
                }
            }

            After = after;
        }
    }
}
