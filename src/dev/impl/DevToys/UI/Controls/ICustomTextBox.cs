#nullable enable

using System.Collections.Generic;

namespace DevToys.UI.Controls
{
    public interface ICustomTextBox
    {
        void SetHighlights(IEnumerable<HighlightSpan> spans);
    }
}
