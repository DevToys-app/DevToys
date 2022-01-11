#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.ImageConverter
{
    [Export(typeof(IToolProvider))]
    [Name("Image Converter")]
    [Parent(GraphicGroupToolProvider.InternalName)]
    [ProtocolName("imageconverter")]
    [Order(1)]
    [NotScrollable]
    internal sealed class ImageConverterToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.ImageConverter.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.ImageConverter.SearchDisplayName;

        public string? Description => LanguageManager.Instance.ImageConverter.Description;

        public string AccessibleName => LanguageManager.Instance.ImageConverter.AccessibleName;

        public TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uF48D");

        [ImportingConstructor]
        public ImageConverterToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<ImageConverterToolViewModel>();
        }
    }
}
