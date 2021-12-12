#nullable enable

using DevToys.Api.Tools;
using DevToys.Shared.Core;

namespace DevToys.Messages
{
    public sealed class PinToolToStartMessage
    {
        internal IToolProvider ToolProvider { get; }

        public PinToolToStartMessage(IToolProvider toolProvider)
        {
            ToolProvider = Arguments.NotNull(toolProvider, nameof(toolProvider));
        }
    }
}
