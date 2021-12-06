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

namespace DevToys.Api.Tools
{
    /// <summary>
    /// Represents a tool provider that matched a certain search.
    /// </summary>
    public class MatchedToolProvider : INotifyPropertyChanged
    {
        private readonly List<MatchedToolProvider> _childrenTools = new();
        private MatchSpan[] _matchedSpans = Array.Empty<MatchSpan>();

        /// <summary>
        /// Gets the tool provider.
        /// </summary>
        public IToolProvider ToolProvider { get; }

        /// <summary>
        /// Gets or sets the list of spans that matched the search in the <see cref="IToolProvider.DisplayName"/>.
        /// </summary>
        public MatchSpan[] MatchedSpans
        {
            get => _matchedSpans;
            set
            {
                _matchedSpans = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AnyMatchedSpan)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MatchedSpans)));
            }
        }

        public bool AnyMatchedSpan => MatchedSpans.Length > 0;

        /// <summary>
        /// Gets the metadata of the tool provider.
        /// </summary>
        public ToolProviderMetadata Metadata { get; }

        /// <summary>
        /// Gets whether the tool should be highlighted in the UI following a smart detection that the tool could be useful for the user.
        /// </summary>
        public bool IsRecommended { get; private set; }

        public IReadOnlyList<MatchedToolProvider> ChildrenTools
        {
            get => _childrenTools;
            set
            {
                _childrenTools.Clear();
                if (value is not null)
                {
                    foreach (MatchedToolProvider item in value)
                    {
                        AddChildTool(item);
                    }
                }
            }
        }

        public bool HasRecommandedChildrenTool => ChildrenTools.Any(item => item.IsRecommended || item.HasRecommandedChildrenTool);

        internal TaskCompletionNotifier<IconElement> Icon => (TaskCompletionNotifier<IconElement>)ToolProvider.IconSource;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MatchedToolProvider(ToolProviderMetadata metadata, IToolProvider toolProvider, MatchSpan[]? matchedSpans = null)
        {
            Metadata = Arguments.NotNull(metadata, nameof(metadata));
            ToolProvider = Arguments.NotNull(toolProvider, nameof(toolProvider));
            MatchedSpans = matchedSpans ?? Array.Empty<MatchSpan>();
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

        internal void AddChildTool(MatchedToolProvider child)
        {
            Arguments.NotNull(child, nameof(child));

            _childrenTools.Add(child);

            if (child.IsRecommended)
            {
                RaisePropertyChanged(nameof(HasRecommandedChildrenTool));
            }

            child.PropertyChanged += Child_PropertyChanged;
        }

        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(IsRecommended) || e.PropertyName == nameof(HasRecommandedChildrenTool))
                && HasRecommandedChildrenTool)
            {
                RaisePropertyChanged(nameof(HasRecommandedChildrenTool));
            }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
