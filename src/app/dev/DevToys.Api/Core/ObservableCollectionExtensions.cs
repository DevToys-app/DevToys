using System.Collections.ObjectModel;

namespace DevToys.Api;

public static class ObservableCollectionExtensions
{
    /// <summary> 
    /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T).
    /// </summary> 
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
