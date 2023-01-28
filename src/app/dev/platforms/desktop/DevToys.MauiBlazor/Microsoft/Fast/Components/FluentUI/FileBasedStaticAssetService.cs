/* 
 SEE https://github.com/microsoft/fast-blazor#blazor-hybrid
 */

using System.Net;
using Microsoft.Fast.Components.FluentUI.Infrastructure;

namespace Microsoft.Fast.Components.FluentUI;

public class FileBasedStaticAssetService : IStaticAssetService
{
    private readonly CacheStorageAccessor _cacheStorageAccessor;

    public FileBasedStaticAssetService(CacheStorageAccessor cacheStorageAccessor)
    {
        _cacheStorageAccessor = cacheStorageAccessor;
    }

    public async Task<string?> GetAsync(string assetUrl, bool useCache = true)
    {
        string? result = null;

        HttpRequestMessage message = CreateMessage(assetUrl);

        if (useCache)
        {
            // Get the result from the cache
            result = await _cacheStorageAccessor.GetAsync(message);
        }

        if (string.IsNullOrEmpty(result))
        {
            //It not in the cache (or cache not used), read the asset from disk
            result = await ReadData(assetUrl);

            if (!string.IsNullOrEmpty(result))
            {
                if (useCache)
                {
                    // If successful, create the response and store in the cache (when used)
                    HttpResponseMessage response = new()
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(result)
                    };

                    await _cacheStorageAccessor.PutAsync(message, response);
                }
            }
        }

        return result;
    }

    private static HttpRequestMessage CreateMessage(string url) => new(HttpMethod.Get, url);

    private static async Task<string?> ReadData(string file)
    {
        using Stream stream = await FileSystem.OpenAppPackageFileAsync($"wwwroot/{file}");
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }
}
