#nullable enable

using System.ComponentModel;

namespace DevToys.Api.Tools
{
    public sealed class ToolProviderMetadata
    {
        /// <summary>
        /// Gets or sets the internal non-localized name of the provider.
        /// </summary>
        [DefaultValue("Unnamed")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the internal non-localized name of the parent provider.
        /// </summary>
        [DefaultValue("")]
        public string Parent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the order in which this tool should appear.
        /// </summary>
        [DefaultValue(int.MaxValue)]
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets where the tool should be displayed in navigation view.
        /// </summary>
        [DefaultValue(MenuPlacement.Body)]
        public MenuPlacement MenuPlacement { get; set; }

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

        /// <summary>
        /// Gets or sets whether the tool can be searched.
        /// </summary>
        [DefaultValue(false)]
        public bool NotSearchable { get; set; }
    }
}
