#nullable enable

using System.Composition;
using System.Threading.Tasks;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.HashGenerator
{
    [Export(typeof(IToolProvider))]
    [Name("Hash Generator")]
    [Parent(GeneratorsGroupToolProvider.InternalName)]
    [ProtocolName("hash")]
    [Order(0)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal sealed class HashGeneratorToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.HashGenerator.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.HashGenerator.SearchDisplayName;

        public string? Description => LanguageManager.Instance.HashGenerator.Description;

        public string AccessibleName => LanguageManager.Instance.HashGenerator.AccessibleName;

        public TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uF409");

        [ImportingConstructor]
        public HashGeneratorToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<HashGeneratorToolViewModel>();
        }
    }
}
