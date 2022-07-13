#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.Graphic.ColorPicker
{
    [Export(typeof(IToolProvider))]
    [Name("Color Picker")]
    [Parent(GraphicGroupToolProvider.InternalName)]
    [ProtocolName("color")]
    [Order(0)]
    internal class ColorPickerToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.ColorPicker.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.ColorPicker.SearchDisplayName;

        public string? Description => LanguageManager.Instance.ColorPicker.Description;

        public string AccessibleName => LanguageManager.Instance.ColorPicker.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.ColorPicker.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uF2F5");

        [ImportingConstructor]
        public ColorPickerToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<ColorPickerToolViewModel>();
        }
    }
}
