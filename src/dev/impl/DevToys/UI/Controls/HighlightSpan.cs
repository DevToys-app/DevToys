#nullable enable

using System.Collections.Generic;
using Windows.UI;

namespace DevToys.UI.Controls
{
    public struct HighlightSpan
    {
        public int StartIndex { get; set; }

        public int Length { get; set; }

        public Color ForegroundColor { get; set; }

        public Color BackgroundColor { get; set; }

        public override bool Equals(object obj)
        {
            return obj is HighlightSpan span &&
                   StartIndex == span.StartIndex &&
                   Length == span.Length &&
                   EqualityComparer<Color>.Default.Equals(ForegroundColor, span.ForegroundColor) &&
                   EqualityComparer<Color>.Default.Equals(BackgroundColor, span.BackgroundColor);
        }

        public override int GetHashCode()
        {
            int hashCode = -1153736223;
            hashCode = (hashCode * -1521134295) + StartIndex.GetHashCode();
            hashCode = (hashCode * -1521134295) + Length.GetHashCode();
            hashCode = (hashCode * -1521134295) + ForegroundColor.GetHashCode();
            hashCode = (hashCode * -1521134295) + BackgroundColor.GetHashCode();
            return hashCode;
        }
    }
}
