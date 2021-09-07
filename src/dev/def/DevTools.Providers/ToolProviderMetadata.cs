using System.ComponentModel;

namespace DevTools.Providers
{
    public sealed class ToolProviderMetadata
    {
        [DefaultValue("Unnamed")]
        public string? Name { get; set; }

        [DefaultValue(int.MaxValue)]
        public int? Order { get; set; }

        [DefaultValue(false)]
        public bool IsFooterItem { get; set; }
    }
}
