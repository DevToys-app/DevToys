namespace DevToys.Api;

/// <summary>
/// Provides extension methods for dictionaries.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Gets the value at the given key, or a default value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to retrieve the value from.</param>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <returns>The value associated with the specified key, or the default value if the key is not found.</returns>
    public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out TValue? value) ? value : default;
    }
}
