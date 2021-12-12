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
    [ProtocolName("formatters")]
    [Order(2)]
    [NotSearchable]
    [NoCompactOverlaySupport]
    internal sealed class FormattersGroupToolProvider : GroupToolProviderBase
    {
        internal const string InternalName = "FormattersGroup";

        public override string MenuDisplayName => LanguageManager.Instance.ToolGroups.FormattersDisplayName;

        public override string AccessibleName => LanguageManager.Instance.ToolGroups.FormattersAccessibleName;

        public override TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\u01BB");

        [ImportingConstructor]
        public FormattersGroupToolProvider(IMefProvider mefProvider)
            : base(mefProvider)
        {
        }
    }
}
