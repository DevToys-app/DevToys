#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace DevToys.ViewModels.Tools.PngJpgCompressor
{
    [Export(typeof(IToolProvider))]
    [Name("PNG/JPG Compressor")]
    [ProtocolName("imgcomp")]
    [Order(4)]
    internal sealed class PngJpgCompressorToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.PngJpgCompressor.DisplayName;

        public object IconSource
            => new TaskCompletionNotifier<IconElement>(
                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    return Task.FromResult<IconElement>(
                        new FontIcon
                        {
                            Glyph = "\uF488"
                        });
                }));

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
