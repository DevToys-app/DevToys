#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools
{
    [Export(typeof(IToolProvider))]
    [Name(InternalName)]
    [ProtocolName("graphic")]
    [Order(5)]
    internal sealed class GraphicGroupToolProvider : GroupToolProviderBase
    {
        internal const string InternalName = "GraphicGroup";

        public override string MenuDisplayName => LanguageManager.Instance.ToolGroups.GraphicDisplayName;

        public override string AccessibleName => LanguageManager.Instance.ToolGroups.GraphicAccessibleName;

        public override TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uF2BB");

        [ImportingConstructor]
        public GraphicGroupToolProvider(IMefProvider mefProvider)
            : base(mefProvider)
        {
        }
    }
}
