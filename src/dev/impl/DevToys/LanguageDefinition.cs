#nullable enable

using System.Globalization;

namespace DevToys
{
    /// <summary>
    /// Represents un language supported by the app.
    /// </summary>
    public struct LanguageDefinition
    {
        /// <summary>
        /// Unique internal ID used to identify the language.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// The name of the language displayed to the user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The culture to apply.
        /// </summary>
        public CultureInfo Culture { get; set; }
    }
}
