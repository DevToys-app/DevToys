﻿namespace DevToys.Blazor.Components;

public sealed class ButtonAppearance
{
    internal static readonly ButtonAppearance Neutral = new("neutral");
    internal static readonly ButtonAppearance Accent = new("accent");
    internal static readonly ButtonAppearance Stealth = new("stealth");

    private ButtonAppearance(string className)
    {
        Guard.IsNotNullOrWhiteSpace(className);
        Class = className;
    }

    public string Class { get; }
}
