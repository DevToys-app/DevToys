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
    [ProtocolName("generators")]
    [Order(3)]
    [NotSearchable]
    [NotFavorable]
    [NoCompactOverlaySupport]
    internal sealed class GeneratorsGroupToolProvider : GroupToolProviderBase
    {
        internal const string InternalName = "GeneratorsGroup";

        public override string MenuDisplayName => LanguageManager.Instance.ToolGroups.GeneratorsDisplayName;

        public override string AccessibleName => LanguageManager.Instance.ToolGroups.GeneratorsAccessibleName;

        public override TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uFEE2");

        [ImportingConstructor]
        public GeneratorsGroupToolProvider(IMefProvider mefProvider)
            : base(mefProvider)
        {
        }
    }
}
