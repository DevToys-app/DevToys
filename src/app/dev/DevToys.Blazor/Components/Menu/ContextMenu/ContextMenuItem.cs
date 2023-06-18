namespace DevToys.Blazor.Components;

public sealed class ContextMenuItem : DropDownListItem
{
    internal string? KeyboardShortcut { get; set; } // TODO: Actually handle keyboard shortcuts?
}
