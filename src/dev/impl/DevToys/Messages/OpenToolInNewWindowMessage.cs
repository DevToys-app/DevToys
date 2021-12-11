#nullable enable

using DevToys.Api.Tools;
using DevToys.Shared.Core;

namespace DevToys.Messages
{
    public sealed class OpenToolInNewWindowMessage
    {
        internal IToolProvider ToolProvider { get; }

        public OpenToolInNewWindowMessage(IToolProvider toolProvider)
        {
            ToolProvider = Arguments.NotNull(toolProvider, nameof(toolProvider));
        }
    }
}
