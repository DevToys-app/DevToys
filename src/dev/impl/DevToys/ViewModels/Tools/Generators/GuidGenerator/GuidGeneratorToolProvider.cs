#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.GuidGenerator
{
    [Export(typeof(IToolProvider))]
    [Name("UUID Generator")]
    [Parent(GeneratorsGroupToolProvider.InternalName)]
    [ProtocolName("uuid")]
    [Order(1)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal sealed class GuidGeneratorToolProvider : IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.GuidGenerator.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.GuidGenerator.SearchDisplayName;

        public string? Description => LanguageManager.Instance.GuidGenerator.Description;

        public string AccessibleName => LanguageManager.Instance.GuidGenerator.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.GuidGenerator.SearchKeywords;

        public string IconGlyph => "\u0106";

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
