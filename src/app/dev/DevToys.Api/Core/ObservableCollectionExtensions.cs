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

    // TODO: Write unit tests
    /// <summary>
    /// Update the difference between the current items in the collection and the <paramref name="newItems"/>.
    /// </summary>
    public static void Update<T>(this ObservableCollection<T> origin, IEnumerable<T> newItems)
    {
        Guard.IsNotNull(origin);
        if (newItems is null)
        {
            return;
        }

        // First, remove the items that aren't part of the new list items.
        var oldToolsMenuItems = origin.ToList();
        for (int i = 0; i < oldToolsMenuItems.Count; i++)
        {
            T item = oldToolsMenuItems[i];
            if (!newItems.Contains(item))
            {
                origin.Remove(item);
            }
        }

        // Then:
        // 1. If an item from newItems already exist in the collection, but at a different position, move it to the desired index.
        // 2. If an item from newItems doesn't exist in the collection, insert it with respect of the position of older items in the collection.
        int insertionIndex = 0;
        foreach (T? item in newItems)
        {
            int indexOfItemInOldMenu = origin.IndexOf(item);
            if (indexOfItemInOldMenu > -1 && insertionIndex < origin.Count)
            {
                if (indexOfItemInOldMenu != insertionIndex)
                {
                    origin.Move(indexOfItemInOldMenu, insertionIndex);
                }
            }
            else
            {
                origin.Insert(insertionIndex, item);
            }

            insertionIndex++;
        }
    }
}
