#nullable enable

using DevToys.Api.Tools;
using DevToys.Shared.Core;

namespace DevToys.Messages
{
    public sealed class PinToolToStartMessage
    {
        internal ToolProviderMetadata ToolProviderMetadata { get; }

        public PinToolToStartMessage(ToolProviderMetadata metadata)
        {
            ToolProviderMetadata = Arguments.NotNull(metadata, nameof(metadata));
        }
    }
}
