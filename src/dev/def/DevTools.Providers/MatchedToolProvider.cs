using System;

namespace DevTools.Providers
{
    public class MatchedToolProvider
    {
        public IToolProvider ToolProvider { get; }

        public MatchSpan[] MatchedSpans { get; }

        public MatchedToolProvider(IToolProvider toolProvider, MatchSpan[] matchedSpans)
        {
            ToolProvider = toolProvider ?? throw new ArgumentNullException();
            MatchedSpans = matchedSpans ?? throw new ArgumentNullException();
        }
    }
}
