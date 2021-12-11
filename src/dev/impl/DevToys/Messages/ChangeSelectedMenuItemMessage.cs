#nullable enable

using DevToys.Api.Tools;
using DevToys.Shared.Core;

namespace DevToys.Messages
{
    public sealed class ChangeSelectedMenuItemMessage
    {
        internal IToolProvider ToolProvider { get; }

        public ChangeSelectedMenuItemMessage(IToolProvider toolProvider)
        {
            ToolProvider = Arguments.NotNull(toolProvider, nameof(toolProvider));
        }
    }
}
