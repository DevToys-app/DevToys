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
    [ProtocolName("converters")]
    [Order(0)]
    internal sealed class ConvertersGroupToolProvider : GroupToolProviderBase
    {
        internal const string InternalName = "ConvertersGroup";

        public override string MenuDisplayName => LanguageManager.Instance.ToolGroups.ConvertersDisplayName;

        public override string AccessibleName => LanguageManager.Instance.ToolGroups.ConvertersAccessibleName;

        public override TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("Converters.svg");

        [ImportingConstructor]
        public ConvertersGroupToolProvider(IMefProvider mefProvider)
            : base(mefProvider)
        {
        }
    }
}
