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
    [ProtocolName("encodersdecoders")]
    [Order(1)]
    [NotSearchable]
    [NotFavorable]
    [NoCompactOverlaySupport]
    internal sealed class EncodersDecodersGroupToolProvider : GroupToolProviderBase
    {
        internal const string InternalName = "EncodersDecodersGroup";

        public override string MenuDisplayName => LanguageManager.Instance.ToolGroups.EncodersDecodersDisplayName;

        public override string AccessibleName => LanguageManager.Instance.ToolGroups.EncodersDecodersAccessibleName;

        public override TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("EncodersDecoders.svg");

        [ImportingConstructor]
        public EncodersDecodersGroupToolProvider(IMefProvider mefProvider)
            : base(mefProvider)
        {
        }
    }
}
