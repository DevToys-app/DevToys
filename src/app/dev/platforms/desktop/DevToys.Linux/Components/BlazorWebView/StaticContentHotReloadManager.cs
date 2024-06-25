using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using DevToys.Api;
using DevToys.Linux.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using IComponent = Microsoft.AspNetCore.Components.IComponent;

// This is important in order to get StaticContentHotReloadManager.UpdateContent method invoked.
[assembly: MetadataUpdateHandler(typeof(StaticContentHotReloadManager))]

namespace DevToys.Linux.Components;


internal static partial class StaticContentHotReloadManager
{
    private delegate void ContentUpdatedHandler(string assemblyName, string relativePath);

    private static event ContentUpdatedHandler? OnContentUpdated;

    // If the current platform can't tell us the application entry assembly name, we can use a placeholder name
    private static readonly string applicationAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? "__application_assembly__";

    private static readonly Dictionary<(string AssemblyName, string RelativePath), (string? ContentType, byte[] Content)> updatedContent
        = new()
        {
            {
                (applicationAssemblyName, "_framework/static-content-hot-reload.js"),
                ("text/javascript", Encoding.UTF8.GetBytes(
                    @"
                        export function notifyCssUpdated() {
                        	const allLinkElems = Array.from(document.querySelectorAll('link[rel=stylesheet]'));
                        	allLinkElems.forEach(elem => elem.href += '');
                        }
                    ")
                )
            }
        };

    /// <summary>
    /// MetadataUpdateHandler event. This is invoked by the hot reload host via reflection.
    /// </summary>
    public static void UpdateContent(string assemblyName, bool isApplicationProject, string relativePath, byte[] contents)
    {
        if (isApplicationProject)
        {
            // Some platforms don't know the name of the application entry assembly (e.g., Android) so in
            // those cases we have a placeholder name for it. The tooling does know the real name, but we
            // need to use our placeholder so the lookups work later.
            assemblyName = applicationAssemblyName;
        }

        updatedContent[(assemblyName, relativePath)] = (ContentType: null, Content: contents);
        OnContentUpdated?.Invoke(assemblyName, relativePath);
    }

    public static void AttachToWebViewManagerIfEnabled(WebViewManager manager)
    {
        if (MetadataUpdater.IsSupported)
        {
            manager.AddRootComponentAsync(typeof(StaticContentChangeNotifier), "body::after", ParameterView.Empty);
        }
    }

    public static bool TryReplaceResponseContent(
        string contentRootRelativePath,
        string requestAbsoluteUri,
        ref int responseStatusCode,
        ref Stream responseContent,
        IDictionary<string, string> responseHeaders)
    {
        if (MetadataUpdater.IsSupported)
        {
            (string assemblyName, string relativePath) = GetAssemblyNameAndRelativePath(requestAbsoluteUri, contentRootRelativePath);
            if (updatedContent.TryGetValue((assemblyName, relativePath), out (string? ContentType, byte[] Content) values))
            {
                responseStatusCode = 200;
                responseContent.Close();
                responseContent = new MemoryStream(values.Content);
                if (!string.IsNullOrEmpty(values.ContentType))
                {
                    responseHeaders["Content-Type"] = values.ContentType;
                }

                return true;
            }
        }

        return false;
    }

    private static (string AssemblyName, string RelativePath) GetAssemblyNameAndRelativePath(string requestAbsoluteUri,
        string appContentRoot)
    {
        string requestPath = new Uri(requestAbsoluteUri).AbsolutePath.Substring(1);
        if (ContentUrlRegex().Match(requestPath) is { Success: true } match)
        {
            // For RCLs (i.e., URLs of the form _content/assembly/path), we assume the content root within the
            // RCL to be "wwwroot" since we have no other information. If this is not the case, content within
            // that RCL will not be hot-reloadable.
            return (match.Groups["AssemblyName"].Value, $"wwwroot/{match.Groups["RelativePath"].Value}");
        }

        if (requestPath.StartsWith("_framework/", StringComparison.Ordinal))
        {
            return (applicationAssemblyName, requestPath);
        }

        return (applicationAssemblyName, Path.Combine(appContentRoot, requestPath).Replace('\\', '/'));
    }

    // To provide a consistent way of transporting the data across all platforms,
    // we can use the existing IJSRuntime. In turn we can get an instance of this
    // that's always attached to the currently-loaded page (if it's a Blazor page)
    // by injecting this headless root component.
    private sealed class StaticContentChangeNotifier : IComponent, IDisposable
    {
        private readonly ILogger _logger;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = default!;

        public StaticContentChangeNotifier()
        {
            _logger = this.Log();
        }

        public void Attach(RenderHandle renderHandle)
        {
            OnContentUpdated += NotifyContentUpdated;
        }

        public void Dispose()
        {
            OnContentUpdated -= NotifyContentUpdated;
        }

        private void NotifyContentUpdated(string assemblyName, string relativePath)
        {
            // It handles its own errors
            _ = NotifyContentUpdatedAsync(assemblyName, relativePath);
        }

        private async Task NotifyContentUpdatedAsync(string assemblyName, string relativePath)
        {
            try
            {
                await using IJSObjectReference module =
                    await JsRuntime.InvokeAsync<IJSObjectReference>(
                        "import",
                        "./_framework/static-content-hot-reload.js");

                // In the future we might want to hot-reload other content types such as images, but currently the tooling is
                // only expected to notify about CSS files. If it notifies us about something else, we'd need different JS logic.
                if (string.Equals(".css", Path.GetExtension(relativePath), StringComparison.Ordinal))
                {
                    // We could try to supply the URL of the modified file, so the JS-side logic could only update the affected
                    // stylesheet. This would reduce flicker. However, this involves hard coding further details about URL conventions
                    // (e.g., _content/AssemblyName/Path) and accounting for configurable content roots. To reduce the chances of
                    // CSS hot reload being broken by customizations, we'll have the JS-side code refresh all stylesheets.
                    await module.InvokeVoidAsync("notifyCssUpdated");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Failed to notify about static content update to {relativePath}.", relativePath);
            }
        }

        public Task SetParametersAsync(ParameterView parameters)
            => Task.CompletedTask;
    }

    [GeneratedRegex("^_content/(?<AssemblyName>[^/]+)/(?<RelativePath>.*)", RegexOptions.Compiled)]
    private static partial Regex ContentUrlRegex();
}
