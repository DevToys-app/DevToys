#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DevToys.Shared.Core.Threading;
using DevToys.Core.Threading;
using DevToys.Shared.Core;
using Windows.UI.Xaml.Controls;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Represents a tool provider that matched a certain search.
    /// </summary>
    public class ToolProviderViewItem : INotifyPropertyChanged
    {
        private readonly List<ToolProviderViewItem> _childrenTools = new();
        private MatchSpan[] _matchedSpans = Array.Empty<MatchSpan>();
        private bool _isBeingProgrammaticallySelected;
        private bool _isFavorite;

        /// <summary>
        /// Gets the tool provider.
        /// </summary>
        public IToolProvider ToolProvider { get; }

        /// <summary>
        /// Gets or sets the list of spans that matched the search in the <see cref="IToolProvider.SearchDisplayName"/>.
        /// </summary>
        public MatchSpan[] MatchedSpans
        {
            get => _matchedSpans;
            set
            {
                _matchedSpans = value;
                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    RaisePropertyChanged(nameof(MatchedSpans));
                }).Forget();
            }
        }

        /// <summary>
        /// Gets or sets the total amount of match in after a search (which can be different from <see cref="MatchedSpans"/>).
        /// </summary>
        public int TotalMatchCount { get; set; }

        /// <summary>
        /// Gets the metadata of the tool provider.
        /// </summary>
        public ToolProviderMetadata Metadata { get; }

        /// <summary>
        /// Gets whether the tool should be highlighted in the UI following a smart detection that the tool could be useful for the user.
        /// </summary>
        public bool IsRecommended { get; private set; }

        /// <summary>
        /// Gets whether the tool
        /// </summary>
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                _isFavorite = value;
                RaisePropertyChanged(nameof(IsFavorite));
            }
        }

        /// <summary>
        /// Gets the name of the tool that will be displayed in the main menu of the app.
        /// </summary>
        public string MenuDisplayName { get; private set; }

        public IReadOnlyList<ToolProviderViewItem> ChildrenTools
        {
            get => _childrenTools;
            set
            {
                _childrenTools.Clear();
                if (value is not null)
                {
                    foreach (ToolProviderViewItem item in value)
                    {
                        AddChildTool(item);
                    }
                }
            }
        }

        public bool MenuItemShouldBeExpanded
            => _isBeingProgrammaticallySelected
            || ChildrenTools.Any(item => item.IsRecommended || item.MenuItemShouldBeExpanded);

        internal string IconGlyph => ToolProvider.IconGlyph;

        public event PropertyChangedEventHandler? PropertyChanged;

        internal static ToolProviderViewItem CreateToolProviderViewItemWithLongMenuDisplayName(ToolProviderViewItem item)
        {
            var newItem = new ToolProviderViewItem(item.Metadata, item.ToolProvider, item.IsFavorite);
            newItem.MenuDisplayName = item.ToolProvider.SearchDisplayName ?? item.ToolProvider.MenuDisplayName;
            return newItem;
        }

        public ToolProviderViewItem(
            ToolProviderMetadata metadata,
            IToolProvider toolProvider,
            bool isFavorite)
        {
            Metadata = Arguments.NotNull(metadata, nameof(metadata));
            ToolProvider = Arguments.NotNull(toolProvider, nameof(toolProvider));
            MatchedSpans = Array.Empty<MatchSpan>();
            IsFavorite = isFavorite;
            MenuDisplayName = toolProvider.MenuDisplayName;
        }

        internal async Task UpdateIsRecommendedAsync(string clipboardContent)
        {
            await TaskScheduler.Default;

            IsRecommended = ToolProvider.CanBeTreatedByTool(clipboardContent);

            ThreadHelper.RunOnUIThreadAsync(() =>
            {
                RaisePropertyChanged(nameof(IsRecommended));
            }).Forget();
        }

        internal void AddChildTool(ToolProviderViewItem child)
        {
            Arguments.NotNull(child, nameof(child));

            _childrenTools.Add(child);

            if (child.IsRecommended)
            {
                RaisePropertyChanged(nameof(MenuItemShouldBeExpanded));
            }

            child.PropertyChanged += Child_PropertyChanged;
        }

        internal IDisposable ForceMenuItemShouldBeExpanded()
        {
            // Notify that parents menu item should be expanded if they're not yet.
            _isBeingProgrammaticallySelected = true;
            RaisePropertyChanged(nameof(MenuItemShouldBeExpanded));
            return new SelectMenuItemProgrammaticallyResult(this);
        }

        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(IsRecommended) || e.PropertyName == nameof(MenuItemShouldBeExpanded))
                && MenuItemShouldBeExpanded)
            {
                RaisePropertyChanged(nameof(MenuItemShouldBeExpanded));
            }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class SelectMenuItemProgrammaticallyResult : IDisposable
        {
            private readonly ToolProviderViewItem _toolProviderViewItem;

            public SelectMenuItemProgrammaticallyResult(ToolProviderViewItem ToolProviderViewItem)
            {
                _toolProviderViewItem = ToolProviderViewItem;
            }

            public void Dispose()
            {
                _toolProviderViewItem._isBeingProgrammaticallySelected = false;
            }
        }
    }
}
