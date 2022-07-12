#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.StringUtilities
{
    [Export(typeof(IToolProvider))]
    [Name("String Utilities")]
    [Parent(TextGroupToolProvider.InternalName)]
    [ProtocolName("string")]
    [Order(0)]
    [NotScrollable]
    internal sealed class StringUtilitiesToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.StringUtilities.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.StringUtilities.SearchDisplayName;

        public string? Description => LanguageManager.Instance.StringUtilities.Description;

        public string AccessibleName => LanguageManager.Instance.StringUtilities.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.StringUtilities.SearchKeywords;

        public string IconGlyph => "\u0131";

        [ImportingConstructor]
        public StringUtilitiesToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<StringUtilitiesToolViewModel>();
        }
    }
}
