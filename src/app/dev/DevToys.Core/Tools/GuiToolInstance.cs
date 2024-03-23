using System.Reflection;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using DevToys.Core.Tools.Metadata;
using Microsoft.Extensions.Logging;

namespace DevToys.Core.Tools;

[DebuggerDisplay($"InternalComponentName = {{{nameof(InternalComponentName)}}}")]
public sealed partial class GuiToolInstance : ObservableObject, IDisposable
{
    private Lazy<UIToolView?> _view;
    private readonly ILogger _logger;
    private readonly Lazy<IGuiTool, GuiToolMetadata> _guiToolDefinition;
    private readonly Lazy<IGuiTool> _instance;
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
            LogInstanceCreated(InternalComponentName);
            return instance;
        });

        RebuildView();
        Guard.IsNotNull(_view);

        LogInitialized(InternalComponentName);
    }

    public string InternalComponentName => _guiToolDefinition.Metadata.InternalComponentName;

    public string IconFontName => _guiToolDefinition.Metadata.IconFontName;

    public char IconGlyph => _guiToolDefinition.Metadata.IconGlyph;

    public string ShortDisplayTitle => _shortDisplayTitle.Value;

    public string LongDisplayTitle => _longDisplayTitle.Value;

    public string LongOrShortDisplayTitle => string.IsNullOrWhiteSpace(_longDisplayTitle.Value) ? ShortDisplayTitle : _longDisplayTitle.Value;

    public string Description => _descriptionDisplayTitle.Value;

    public string AccessibleName => _accessibleNameDisplayTitle.Value;

    public string SearchKeywords => _searchKeywordsDisplayTitle.Value;

    public bool NotSearchable => _guiToolDefinition.Metadata.NotSearchable;

    public bool NotFavorable => _guiToolDefinition.Metadata.NotFavorable;

    public bool NoCompactOverlaySupport => _guiToolDefinition.Metadata.NoCompactOverlaySupport;

    public string GroupName => _guiToolDefinition.Metadata.GroupName;

    public bool IsFooterTool => _guiToolDefinition.Metadata.MenuPlacement == MenuPlacement.Footer;

    public IReadOnlyList<string> AcceptedDataTypeNames => _guiToolDefinition.Metadata.AcceptedDataTypeNames;

    /// <summary>
    /// Gets the view of the tool.
    /// Calling this property is expensive the first time as it will create the instance of the tool and the instance of the view.
    /// </summary>
    public UIToolView? View => _view.Value;

    public void Dispose()
    {
        if (_instance.IsValueCreated && _instance.Value is IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch (NotImplementedException) { }
            catch (Exception ex)
            {
                LogDisposingToolFailed(ex, InternalComponentName);
            }
        }
    }

    /// <summary>
    /// Send data coming from Smart Detection to the tool.
    /// </summary>
    public void PassSmartDetectedData(string dataTypeName, object? parsedData)
    {
        // Ensure the view is created. We do this because `OnDataReceived` may assume the UI is instantiated.
        _ = View;

        try
        {
            // Send the data to the tool.
            _instance.Value.OnDataReceived(dataTypeName, parsedData);
        }
        catch (NotImplementedException) { }
        catch (Exception ex)
        {
            LogPassSmartDetectedDataFailed(ex, InternalComponentName);
        }
    }

    /// <summary>
    /// Re-create the <see cref="View"/> instance.
    /// </summary>
    public void RebuildView()
    {
        if (_view is not null && _view.IsValueCreated)
        {
            _view.Value?.CurrentOpenedDialog?.Dispose();
        }

        _view
            = new(() =>
            {
                Exception? exception = null;
                try
                {
                    return _instance.Value?.View;
                }
                catch (NotImplementedException) { }
                catch (Exception ex)
                {
                    exception = ex;
                    LogGetToolViewFailed(ex, InternalComponentName);
                }

                return new UIToolView(GUI.Label("unable-load-tool-view", $"Failed to load '{InternalComponentName}'. " + exception?.Message));
            });
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

    [LoggerMessage(2, LogLevel.Warning, "Unexpectedly failed to pass smart detection data to '{toolName}'.")]
    partial void LogPassSmartDetectedDataFailed(Exception ex, string toolName);

    [LoggerMessage(3, LogLevel.Error, "Unexpectedly failed to get the view for the tool '{toolName}'.")]
    partial void LogGetToolViewFailed(Exception ex, string toolName);

    [LoggerMessage(4, LogLevel.Error, "Unexpectedly failed to dispose '{toolName}'.")]
    partial void LogDisposingToolFailed(Exception ex, string toolName);
}
