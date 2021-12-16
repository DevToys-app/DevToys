#nullable enable

using DevToys.Shared.Core;

namespace DevToys.Models
{
    internal sealed class TileIconSizeDefinition
    {
        public TileIconSizeDefinition(string iconName, int size, double toolIconRatio)
        {
            IconName = Arguments.NotNullOrWhiteSpace(iconName, nameof(iconName));
            Size = size;
            ToolIconRatio = toolIconRatio;
        }

        internal int Size { get; }

        internal double ToolIconRatio { get; }

        internal string IconName { get; }
    }
}
