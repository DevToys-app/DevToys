#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DevToys.Shared.Core;

namespace DevToys.Core.Collections
{
    public class ExtendedObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T).
        /// </summary> 
        internal void AddRange(IEnumerable<T> collection)
        {
            Arguments.NotNull(collection, nameof(collection));

            foreach (T item in collection)
            {
                Items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
