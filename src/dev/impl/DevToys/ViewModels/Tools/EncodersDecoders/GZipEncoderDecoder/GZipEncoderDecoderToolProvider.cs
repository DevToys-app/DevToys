#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.GZipEncoderDecoder
{
    [Export(typeof(IToolProvider))]
    [Name("GZip Compress/Decompress")]
    [Parent(EncodersDecodersGroupToolProvider.InternalName)]
    [ProtocolName("gzip")]
    [Order(1)]
    internal sealed class GZipEncoderDecoderToolProvider : ToolProviderBase, IToolProvider
    {
        public string MenuDisplayName => LanguageManager.Instance.GZipEncoderDecoder.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.GZipEncoderDecoder.SearchDisplayName;

        public string? Description => LanguageManager.Instance.GZipEncoderDecoder.Description;

        public string AccessibleName => LanguageManager.Instance.GZipEncoderDecoder.AccessibleName;

        public TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uF435");

        private readonly IMefProvider _mefProvider;

        [ImportingConstructor]
        public GZipEncoderDecoderToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            string? trimmedData = data.Trim();
            return trimmedData.StartsWith("H4sI");
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<GZipEncoderDecoderToolViewModel>();
        }
    }
}
