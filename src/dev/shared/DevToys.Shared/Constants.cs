#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.Shared
{
    internal static class Constants
    {
        internal const string AppServiceName = "DevToysOOPService";

        internal const int AppServiceBufferSize = 2048;

        internal static readonly TimeSpan AppServiceTimeout = TimeSpan.FromSeconds(10);

        internal static readonly JsonSerializerSettings AppServiceJsonSerializerSettings 
            = new()
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
    }
}
