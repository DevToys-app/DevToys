using Microsoft.UI.Xaml.Controls;

namespace DevToys.Models
{
    public class InfoBarData
    {
        public InfoBarData(InfoBarSeverity severity, string message)
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
    }
}
