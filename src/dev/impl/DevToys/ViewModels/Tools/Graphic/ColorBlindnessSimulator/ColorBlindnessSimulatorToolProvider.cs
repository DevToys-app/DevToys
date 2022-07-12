#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.ColorBlindnessSimulator
{
    [Export(typeof(IToolProvider))]
    [Name("Color Blindness Simulator")]
    [Parent(GraphicGroupToolProvider.InternalName)]
    [ProtocolName("colorblind")]
    [Order(0)]
    [NotScrollable]
    internal class ColorBlindnessSimulatorToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.ColorBlindnessSimulator.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.ColorBlindnessSimulator.SearchDisplayName;

        public string? Description => LanguageManager.Instance.ColorBlindnessSimulator.Description;

        public string AccessibleName => LanguageManager.Instance.ColorBlindnessSimulator.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.ColorBlindnessSimulator.SearchKeywords;

        public string IconGlyph => "\u0101";

        [ImportingConstructor]
        public ColorBlindnessSimulatorToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<ColorBlindnessSimulatorToolViewModel>();
        }
    }
}
