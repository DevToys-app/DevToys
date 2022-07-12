#nullable enable

using DevToys.Api.Tools;
using DevToys.Shared.Core;

namespace DevToys.Messages
{
    public sealed class AddToFavoritesMessage
    {
        internal ToolProviderViewItem Tool { get; }

        public AddToFavoritesMessage(ToolProviderViewItem tool)
        {
            Tool = Arguments.NotNull(tool, nameof(tool));
        }
    }
}
