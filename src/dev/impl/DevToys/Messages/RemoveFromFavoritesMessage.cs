#nullable enable

using DevToys.Api.Tools;
using DevToys.Shared.Core;

namespace DevToys.Messages
{
    public sealed class RemoveFromFavoritesMessage
    {
        internal ToolProviderViewItem Tool { get; }

        public RemoveFromFavoritesMessage(ToolProviderViewItem tool)
        {
            Tool = Arguments.NotNull(tool, nameof(tool));
        }
    }
}
