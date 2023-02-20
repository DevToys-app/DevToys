using System.Reflection;
using System.Resources;
using DevToys.Api;
using DevToys.Core.Tools.Metadata;
using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace DevToys.Core.Tools;

[DebuggerDisplay($"InternalComponentName = {{{nameof(InternalComponentName)}}}")]
public sealed partial class GuiToolInstance
{
    private readonly ILogger _logger;
    private readonly Lazy<IGuiTool, GuiToolMetadata> _guiToolDefinition;
    private readonly Lazy<IGuiTool> _instance;
    private readonly Lazy<ResourceManager?> _resourceManager;

    internal GuiToolInstance(Lazy<IGuiTool, GuiToolMetadata> guiToolDefinition, Assembly? resourceManagerAssembly)
    {
        _logger = this.Log();
        _guiToolDefinition = guiToolDefinition;
        _resourceManager = new(() => GetResourceManager(resourceManagerAssembly));

        _instance = new(() =>
        {
            IGuiTool instance = _guiToolDefinition.Value;
            LogInstanceCreated(_guiToolDefinition.Metadata.InternalComponentName);
            return instance;
        });
        LogInitialized(_guiToolDefinition.Metadata.InternalComponentName);
    }

    public string InternalComponentName => _guiToolDefinition.Metadata.InternalComponentName;

    public string Author => _guiToolDefinition.Metadata.Author;

    public string IconFontName => _guiToolDefinition.Metadata.IconFontName;

    public string IconGlyph => _guiToolDefinition.Metadata.IconGlyph;

    public string ShortDisplayTitle => GetDisplayString(_guiToolDefinition.Metadata.ShortDisplayTitleResourceName);

    public string LongDisplayTitle => GetDisplayString(_guiToolDefinition.Metadata.LongDisplayTitleResourceName);

    public string Description => GetDisplayString(_guiToolDefinition.Metadata.DescriptionResourceName);

    public string AccessibleName => GetDisplayString(_guiToolDefinition.Metadata.AccessibleNameResourceName);

    public string SearchKeywords => GetDisplayString(_guiToolDefinition.Metadata.SearchKeywordsResourceName);

    public bool NotSearchable => _guiToolDefinition.Metadata.NotSearchable;

    public bool NotFavorable => _guiToolDefinition.Metadata.NotFavorable;

    public bool NoCompactOverlaySupport => _guiToolDefinition.Metadata.NoCompactOverlaySupport;

    public int? CompactOverlayHeight => _guiToolDefinition.Metadata.CompactOverlayHeight;

    public int? CompactOverlayWidth => _guiToolDefinition.Metadata.CompactOverlayWidth;

    public string GroupName => _guiToolDefinition.Metadata.GroupName;

    /// <summary>
    /// Gets the instance of the tool.
    /// Calling this property be expensive the first time as it will create the instance.
    /// </summary>
    public IGuiTool ToolInstance => _instance.Value;

    private ResourceManager? GetResourceManager(Assembly? resourceManagerAssembly)
    {
        ResourceManager? resourceManager = null;

        if (resourceManagerAssembly is not null)
        {
            // Load resource manager, if needed.
            resourceManager
                = !string.IsNullOrWhiteSpace(_guiToolDefinition.Metadata.ResourceManagerBaseName)
                ? new ResourceManager(_guiToolDefinition.Metadata.ResourceManagerBaseName, resourceManagerAssembly)
                : null;
        }

        return resourceManager;
    }

    private string GetDisplayString(string resourceName)
    {
        return _resourceManager.Value is not null && !string.IsNullOrWhiteSpace(resourceName)
            ? _resourceManager.Value.GetString(resourceName) ?? string.Empty
            : string.Empty;
    }

    [LoggerMessage(0, LogLevel.Information, "Initialized '{toolName}' tool instance manager.")]
    partial void LogInitialized(string toolName);

    [LoggerMessage(1, LogLevel.Information, "Instance of '{toolName}' tool created.")]
    partial void LogInstanceCreated(string toolName);
}
