#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Shared.Api.Core;
using DevToys.ViewModels.Tools.Converters.NumberBaseConverter;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.NumberBaseConverter
{
    [Export(typeof(IToolProvider))]
    [Name("Number Base Converter")]
    [Parent(ConvertersGroupToolProvider.InternalName)]
    [ProtocolName("baseconverter")]
    [Order(1)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal sealed class NumberBaseConverterToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.NumberBaseConverter.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.NumberBaseConverter.SearchDisplayName;

        public string? Description => LanguageManager.Instance.NumberBaseConverter.Description;

        public string AccessibleName => LanguageManager.Instance.NumberBaseConverter.AccessibleName;

        public TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uFE2C");

        [ImportingConstructor]
        public NumberBaseConverterToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            data = NumberBaseFormatter.RemoveFormatting(data).ToString();
            return NumberBaseHelper.IsValidBinary(data) || NumberBaseHelper.IsValidHexadecimal(data);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<NumberBaseConverterToolViewModel>();
        }
    }
}
