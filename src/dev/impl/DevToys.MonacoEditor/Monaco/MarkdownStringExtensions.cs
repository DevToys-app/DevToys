#nullable enable

using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Metadata;

namespace DevToys.MonacoEditor.Monaco
{
    public static class MarkdownStringExtensions
    {
        [DefaultOverload]
        public static MarkdownString ToMarkdownString(this string svalue)
        {
            return ToMarkdownString(svalue, false);
        }

        [DefaultOverload]
        public static MarkdownString ToMarkdownString(this string svalue, bool isTrusted)
        {
            return new MarkdownString(svalue, isTrusted);
        }

        public static MarkdownString[] ToMarkdownString([ReadOnlyArray] this string[] values)
        {
            return ToMarkdownString(values, false);
        }

        public static MarkdownString[] ToMarkdownString([ReadOnlyArray] this string[] values, bool isTrusted)
        {
            return values.Select(value => new MarkdownString(value, isTrusted)).ToArray();
        }
    }
}
