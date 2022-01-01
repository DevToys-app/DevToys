using System.Composition;
using Windows.UI.Xaml.Controls;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;

#nullable enable

namespace DevToys.ViewModels.Tools.LoremIpsumGenerator
{
    [Export(typeof(IToolProvider))]
    [Name("Lorem Ipsum Generator")]
    [Parent(GeneratorsGroupToolProvider.InternalName)]
    [ProtocolName("loremipsum")]
    [Order(2)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal sealed class LoremIpsumGeneratorToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.LoremIpsumGenerator.MenuDisplayName;
        public string? SearchDisplayName => LanguageManager.Instance.LoremIpsumGenerator.SearchDisplayName;
        public string? Description => LanguageManager.Instance.LoremIpsumGenerator.Description;
        public string AccessibleName => LanguageManager.Instance.LoremIpsumGenerator.AccessibleName;
        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("LoremIpsum.svg");
        
        [ImportingConstructor]
        public LoremIpsumGeneratorToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<LoremIpsumGeneratorToolViewModel>();
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }
    }
}
