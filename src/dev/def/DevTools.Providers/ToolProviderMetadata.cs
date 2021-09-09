using System.ComponentModel;

namespace DevTools.Providers
{
    public sealed class ToolProviderMetadata
    {
        /// <summary>
        /// Gets the internal non-localized name of the provider.
        /// </summary>
        [DefaultValue("Unnamed")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the order in which this tool should appear.
        /// </summary>
        [DefaultValue(int.MaxValue)]
        public int? Order { get; set; }

        /// <summary>
        /// Gets whether the tool should be displayed in the footer of the menu.
        /// </summary>
        [DefaultValue(false)]
        public bool IsFooterItem { get; set; }

        /// <summary>
        /// Gets or sets the tool name used through URI Activation Protocol to access this tool.
        /// </summary>
        [DefaultValue("")]
        public string ProtocolName { get; set; } = string.Empty;
    }
}
