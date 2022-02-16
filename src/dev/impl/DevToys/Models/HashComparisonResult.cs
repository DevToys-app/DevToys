using Microsoft.UI.Xaml.Controls;

namespace DevToys.Models
{
    internal class HashComparisonResult
    {
        internal HashComparisonResult(InfoBarSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public InfoBarSeverity Severity { get; }

        public string Message { get; }

        public static HashComparisonResult None { get; }
    }
}
