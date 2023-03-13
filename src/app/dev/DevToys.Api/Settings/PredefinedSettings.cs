namespace DevToys.Api;

public static class PredefinedSettings
{
    /// <summary>
    /// Whether the compact mode is enabled or not.
    /// </summary>
    public static readonly SettingDefinition<bool> CompactMode
        = new(
            name: nameof(CompactMode),
            defaultValue: false);
}
