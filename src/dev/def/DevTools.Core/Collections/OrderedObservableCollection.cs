using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DevTools.Core.Collections
{
    public class OrderedObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Update the difference between the current items in the collection and the <paramref name="newItems"/>.
        /// </summary>
        public void Update(IEnumerable<T> newItems)
        {
            // First, remove the items that aren't part of the new list items.
            List<T> oldToolsMenuItems = this.ToList();
            for (int i = 0; i < oldToolsMenuItems.Count; i++)
            {
                T item = oldToolsMenuItems[i];
                if (!newItems.Contains(item))
                {
                    Remove(item);
                }
            }

            // Then:
            // 1. If an item from newItems already exist in the collection, but at a different position, move it to the desired index.
            // 2. If an item from newItems doesn't exist in the collection, insert it with respect of the position of older items in the collection.
            var insertionIndex = 0;
            foreach (var item in newItems)
            {
                int indexOfItemInOldMenu = IndexOf(item);
                if (indexOfItemInOldMenu > -1)
                {
                    if (indexOfItemInOldMenu != insertionIndex)
                    {
                        Move(indexOfItemInOldMenu, insertionIndex);
                    }
                }
                else
                {
                    Insert(insertionIndex, item);
                }

                insertionIndex++;
            }
        }
    }
}
