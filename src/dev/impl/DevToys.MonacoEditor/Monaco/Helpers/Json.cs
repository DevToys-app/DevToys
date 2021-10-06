#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Helpers
{
    static class Json
    {
        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
