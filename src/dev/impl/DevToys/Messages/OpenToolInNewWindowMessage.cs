#nullable enable

using DevToys.Api.Tools;
using DevToys.Shared.Core;

namespace DevToys.Messages
{
    public sealed class OpenToolInNewWindowMessage
    {
        internal ToolProviderMetadata ToolProviderMetadata { get; }

        public OpenToolInNewWindowMessage(ToolProviderMetadata metadata)
        {
            ToolProviderMetadata = Arguments.NotNull(metadata, nameof(metadata));
        }
    }
}
