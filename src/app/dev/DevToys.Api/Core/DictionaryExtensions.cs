namespace DevToys.Api;

public static class DictionaryExtensions
{
    /// <summary>
    /// Gets the value at the given key, or a default value.
    /// </summary>
    public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out TValue? value) ? value : default;
    }
}
