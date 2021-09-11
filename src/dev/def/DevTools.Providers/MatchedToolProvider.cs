using DevTools.Core;
using System;
using System.ComponentModel;

namespace DevTools.Providers
{
    /// <summary>
    /// Represents a tool provider that matched a certain search.
    /// </summary>
    public class MatchedToolProvider : INotifyPropertyChanged
    {
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MatchedSpans)));
            }
        }

        /// <summary>
        /// Gets the metadata of the tool provider.
        /// </summary>
        public ToolProviderMetadata Metadata { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MatchedToolProvider(ToolProviderMetadata metadata, IToolProvider toolProvider, MatchSpan[] matchedSpans)
        {
            Metadata = Arguments.NotNull(metadata, nameof(metadata));
            ToolProvider = Arguments.NotNull(toolProvider, nameof(toolProvider));
            MatchedSpans = Arguments.NotNull(matchedSpans, nameof(matchedSpans));
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
