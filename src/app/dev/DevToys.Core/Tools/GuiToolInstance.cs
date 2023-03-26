using System.Reflection;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using DevToys.Api;
using DevToys.Core.Tools.Metadata;
using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace DevToys.Core.Tools;

[DebuggerDisplay($"InternalComponentName = {{{nameof(InternalComponentName)}}}")]
public sealed partial class GuiToolInstance : ObservableObject
{
    private readonly ILogger _logger;
    private readonly Lazy<IGuiTool, GuiToolMetadata> _guiToolDefinition;
    private readonly Lazy<IGuiTool> _instance;
    private readonly Lazy<IUIElement> _view;
    private readonly Lazy<ResourceManager?> _resourceManager;
    private readonly Lazy<string> _shortDisplayTitle;
    private readonly Lazy<string> _longDisplayTitle;
    private readonly Lazy<string> _descriptionDisplayTitle;
    private readonly Lazy<string> _accessibleNameDisplayTitle;
    private readonly Lazy<string> _searchKeywordsDisplayTitle;

    internal GuiToolInstance(Lazy<IGuiTool, GuiToolMetadata> guiToolDefinition, Assembly? resourceManagerAssembly)
    {
        _logger = this.Log();
        _guiToolDefinition = guiToolDefinition;
        _resourceManager = new(() => GetResourceManager(resourceManagerAssembly));

        _shortDisplayTitle = new(() => GetDisplayString(_guiToolDefinition.Metadata.ShortDisplayTitleResourceName));
        _longDisplayTitle = new(() => GetDisplayString(_guiToolDefinition.Metadata.LongDisplayTitleResourceName));
        _descriptionDisplayTitle = new(() => GetDisplayString(_guiToolDefinition.Metadata.DescriptionResourceName));
        _accessibleNameDisplayTitle = new(() => GetDisplayString(_guiToolDefinition.Metadata.AccessibleNameResourceName));
        _searchKeywordsDisplayTitle = new(() => GetDisplayString(_guiToolDefinition.Metadata.SearchKeywordsResourceName));

        _instance = new(() =>
        {
            IGuiTool instance = _guiToolDefinition.Value;
            LogInstanceCreated(_guiToolDefinition.Metadata.InternalComponentName);
            return instance;
        });

        _view = new(() => _instance.Value.View); // TODO: Try Catch and log?

        LogInitialized(_guiToolDefinition.Metadata.InternalComponentName);
    }

    public string InternalComponentName => _guiToolDefinition.Metadata.InternalComponentName;

    public string IconFontName => _guiToolDefinition.Metadata.IconFontName;

    public string IconGlyph => _guiToolDefinition.Metadata.IconGlyph;

    public string ShortDisplayTitle => _shortDisplayTitle.Value;

    public string LongDisplayTitle => _longDisplayTitle.Value;

    public string LongOrShortDisplayTitle => string.IsNullOrWhiteSpace(_longDisplayTitle.Value) ? ShortDisplayTitle : _longDisplayTitle.Value;

    public string Description => _descriptionDisplayTitle.Value;

    public string AccessibleName => _accessibleNameDisplayTitle.Value;

    public string SearchKeywords => _searchKeywordsDisplayTitle.Value;

    public bool NotSearchable => _guiToolDefinition.Metadata.NotSearchable;

    public bool NotFavorable => _guiToolDefinition.Metadata.NotFavorable;

    public bool NoCompactOverlaySupport => _guiToolDefinition.Metadata.NoCompactOverlaySupport;

    public int? CompactOverlayHeight => _guiToolDefinition.Metadata.CompactOverlayHeight;

    public int? CompactOverlayWidth => _guiToolDefinition.Metadata.CompactOverlayWidth;

    public string GroupName => _guiToolDefinition.Metadata.GroupName;

    public bool IsFooterTool => _guiToolDefinition.Metadata.MenuPlacement == MenuPlacement.Footer;

    public IReadOnlyList<string> AcceptedDataTypeNames => _guiToolDefinition.Metadata.AcceptedDataTypeNames;

    /// <summary>
    /// Gets the view of the tool.
    /// Calling this property is expensive the first time as it will create the instance of the tool and the instance of the view.
    /// </summary>
    public IUIElement View => _view.Value;

    /// <summary>
    /// Send data coming from Smart Detection to the tool.
    /// </summary>
    public void PassSmartDetectedData(string dataTypeName, object? parsedData)
    {
        _instance.Value.OnDataReceived(dataTypeName, parsedData);
    }

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
