#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.PngJpgCompressor
{
    [Export(typeof(IToolProvider))]
    [Name("PNG/JPG Compressor")]
    [Parent(GraphicGroupToolProvider.InternalName)]
    [ProtocolName("imgcomp")]
    [Order(0)]
    internal sealed class PngJpgCompressorToolProvider : IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.PngJpgCompressor.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.PngJpgCompressor.SearchDisplayName;

        public string? Description => LanguageManager.Instance.PngJpgCompressor.Description;

        public string AccessibleName => LanguageManager.Instance.PngJpgCompressor.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.PngJpgCompressor.SearchKeywords;

        public string IconGlyph => "\u0128";

        [ImportingConstructor]
        public PngJpgCompressorToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<PngJpgCompressorToolViewModel>();
        }
    }
}
