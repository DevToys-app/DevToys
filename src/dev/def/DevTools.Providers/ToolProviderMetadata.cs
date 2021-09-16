using System.ComponentModel;

namespace DevTools.Providers
{
    public sealed class ToolProviderMetadata
    {
        /// <summary>
        /// Gets or sets the internal non-localized name of the provider.
        /// </summary>
        [DefaultValue("Unnamed")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the order in which this tool should appear.
        /// </summary>
        [DefaultValue(int.MaxValue)]
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets whether the tool should be displayed in the footer of the menu.
        /// </summary>
        [DefaultValue(false)]
        public bool IsFooterItem { get; set; }

        /// <summary>
        /// Gets or sets the tool name used through URI Activation Protocol to access this tool.
        /// </summary>
        [DefaultValue("")]
        public string ProtocolName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the default height the Compact Overlay should take when using this tool provider.
        /// </summary>
        /// <remarks>
        /// Windows will limit the size to the system-defined max value.
        /// </remarks>
        [DefaultValue(500)]
        public int CompactOverlayHeight { get; set; }

        /// <summary>
        /// Gets or sets the default width the Compact Overlay should take when using this tool provider.
        /// </summary>
        /// <remarks>
        /// Windows will limit the size to the system-defined max value.
        /// </remarks>
        [DefaultValue(500)]
        public int CompactOverlayWidth { get; set; }

        /// <summary>
        /// Gets or sets whether the tool view can be scrolled.
        /// </summary>
        [DefaultValue(false)]
        public bool NotScrollable { get; set; }
    }
}
