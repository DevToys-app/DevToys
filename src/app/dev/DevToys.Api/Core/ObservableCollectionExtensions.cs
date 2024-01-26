using System.Collections.ObjectModel;

namespace DevToys.Api;

/// <summary>
/// Provides extension methods for <see cref="ObservableCollection{T}"/>.
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary> 
    /// Adds the elements of the specified collection to the end of the <see cref="ObservableCollection{T}"/>.
    /// </summary> 
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="origin">The <see cref="ObservableCollection{T}"/> to add elements to.</param>
    /// <param name="newItems">The collection whose elements should be added to the end of the <see cref="ObservableCollection{T}"/>.</param>
    public static void AddRange<T>(this ObservableCollection<T> origin, IEnumerable<T> newItems)
    {
        Guard.IsNotNull(origin);
        if (newItems is null)
        {
            return;
        }

        foreach (T item in newItems)
        {
            origin.Add(item);
        }
    }
}
