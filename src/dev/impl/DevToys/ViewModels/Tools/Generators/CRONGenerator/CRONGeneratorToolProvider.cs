#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.GuidGenerator
{
    [Export(typeof(IToolProvider))]
    [Name("CRON Generator")]
    [Parent(GeneratorsGroupToolProvider.InternalName)]
    [ProtocolName("cron")]
    [Order(1)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal sealed class CRONGeneratorToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.GuidGenerator.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.GuidGenerator.SearchDisplayName;

        public string? Description => LanguageManager.Instance.GuidGenerator.Description;

        public string AccessibleName => LanguageManager.Instance.GuidGenerator.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.GuidGenerator.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("Guid.svg");

        [ImportingConstructor]
        public CRONGeneratorToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<CRONGeneratorToolViewModel>();
        }
    }
}
