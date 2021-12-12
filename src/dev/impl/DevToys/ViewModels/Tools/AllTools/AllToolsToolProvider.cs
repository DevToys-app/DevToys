#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using DevToys.ViewModels.Tools;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.AllTools
{
    [Export(typeof(IToolProvider))]
    [Name("All Tools")]
    [ProtocolName("all")]
    [Order(0)]
    [MenuPlacement(MenuPlacement.Header)]
    [NotSearchable]
    internal sealed class AllToolsToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.AllTools.MenuDisplayName;

        public string? SearchDisplayName => MenuDisplayName;

        public string? Description { get; } = null;

        public string AccessibleName => LanguageManager.Instance.AllTools.AccessibleName;

        public TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uF480");

        [ImportingConstructor]
        public AllToolsToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<AllToolsToolViewModel>();
        }
    }
}
