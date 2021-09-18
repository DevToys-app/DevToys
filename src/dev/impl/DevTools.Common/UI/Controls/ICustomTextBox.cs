#nullable enable

using System.Collections.Generic;

namespace DevTools.Common.UI.Controls
{
    public interface ICustomTextBox
    {
        void SetHighlights(IEnumerable<HighlightSpan> spans);
    }
}
