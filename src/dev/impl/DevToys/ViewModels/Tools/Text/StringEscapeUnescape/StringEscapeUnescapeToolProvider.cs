#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.StringEscapeUnescape
{
    [Export(typeof(IToolProvider))]
    [Name("String Escape/Unescape")]
    [Parent(TextGroupToolProvider.InternalName)]
    [ProtocolName("escape")]
    [Order(0)]
    internal sealed class StringEscapeUnescapeToolProvider : IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.StringEscapeUnescape.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.StringEscapeUnescape.SearchDisplayName;

        public string? Description => LanguageManager.Instance.StringEscapeUnescape.Description;

        public string AccessibleName => LanguageManager.Instance.StringEscapeUnescape.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.StringEscapeUnescape.SearchKeywords;

        public string IconGlyph => "\u0130";

        [ImportingConstructor]
        public StringEscapeUnescapeToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return StringManipulationHelper.HasEscapeCharacters(data);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<StringEscapeUnescapeToolViewModel>();
        }
    }
}
