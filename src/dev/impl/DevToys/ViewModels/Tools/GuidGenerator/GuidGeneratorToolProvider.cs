#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;

namespace DevToys.ViewModels.Tools.GuidGenerator
{
    [Export(typeof(IToolProvider))]
    [Name("UUID Generator")]
    [ProtocolName("uuid")]
    [Order(3)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal sealed class GuidGeneratorToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.GuidGenerator.DisplayName;

        public string AccessibleName => LanguageManager.Instance.GuidGenerator.AccessibleName;

        public object IconSource => CreatePathIconFromPath(nameof(GuidGeneratorToolProvider));

        [ImportingConstructor]
        public GuidGeneratorToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<GuidGeneratorToolViewModel>();
        }
    }
}
