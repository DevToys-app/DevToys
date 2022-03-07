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
    [ProtocolName("text")]
    [Order(4)]
    [NotSearchable]
    [NotFavorable]
    [NoCompactOverlaySupport]
    internal sealed class TextGroupToolProvider : GroupToolProviderBase
    {
        internal const string InternalName = "TextGroup";

        public override string MenuDisplayName => LanguageManager.Instance.ToolGroups.TextDisplayName;

        public override string AccessibleName => LanguageManager.Instance.ToolGroups.TextDisplayName;

        public override TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uF7E4");

        [ImportingConstructor]
        public TextGroupToolProvider(IMefProvider mefProvider)
            : base(mefProvider)
        {
        }
    }
}
