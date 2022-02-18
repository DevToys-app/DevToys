#nullable enable

using Microsoft.UI.Xaml.Controls;

namespace DevToys.Models
{
    /// <summary>
    /// Contains information regarding the result of the hash comparison.
    /// </summary>
    internal sealed class HashComparisonResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HashComparisonResult"/> class.
        /// </summary>
        /// <param name="severity">The severity of the comparison result message</param>
        /// <param name="message">The message that can be shown to the user</param>
        internal HashComparisonResult(InfoBarSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        /// <summary>
        /// Gets a value that represents the severity of the comparison result message
        /// </summary>
        public InfoBarSeverity Severity { get; }

        /// <summary>
        /// Gets a value that represents the message can be shown to the user
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the default-empty comparison result.
        /// </summary>
        public static HashComparisonResult? None { get; }
    }
}
