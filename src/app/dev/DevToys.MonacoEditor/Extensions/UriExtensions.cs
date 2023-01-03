namespace DevToys.MonacoEditor.Extensions;

internal static class UriExtensions
{
    private static readonly string UNO_BOOTSTRAP_APP_BASE = Environment.GetEnvironmentVariable(nameof(UNO_BOOTSTRAP_APP_BASE)) ?? string.Empty;
    private static readonly string UNO_BOOTSTRAP_WEBAPP_BASE_PATH = Environment.GetEnvironmentVariable(nameof(UNO_BOOTSTRAP_WEBAPP_BASE_PATH)) ?? "";

    internal static string AbsoluteUriString(this Uri uri)
    {
        string target;
        if (uri.IsAbsoluteUri)
        {
#if __WASM__
            Debugger.Launch(); // TODO
            if (uri.Scheme == "file" || uri.Scheme == "ms-appx-web")
            {
                // Local files are assumed as coming from the remoter server
                target
                    = UNO_BOOTSTRAP_APP_BASE is null
                    ? uri.PathAndQuery
                    : UNO_BOOTSTRAP_WEBAPP_BASE_PATH + UNO_BOOTSTRAP_APP_BASE + uri.PathAndQuery;
            }
            else
            {
                target = uri.AbsoluteUri;
            }
#else
            target = uri.AbsoluteUri;
#endif
        }
        else
        {
            target
                = UNO_BOOTSTRAP_APP_BASE is null
                ? uri.OriginalString
                : UNO_BOOTSTRAP_WEBAPP_BASE_PATH + UNO_BOOTSTRAP_APP_BASE + "/" + uri.OriginalString;
        }
        return target;
    }
}
