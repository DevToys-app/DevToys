namespace DevToys.Blazor.Components;

public sealed class ContextMenuItem
{
    internal string? Text { get; set; }

    internal char IconGlyph { get; set; }

    internal string IconFontFamily { get; set; } = "FluentSystemIcons";

    internal string? KeyboardShortcut { get; set; } // TODO: Actually handle keyboard shortcuts?

    internal bool IsEnabled { get; set; } = true;

    internal EventCallback OnClick { get; set; }
}
