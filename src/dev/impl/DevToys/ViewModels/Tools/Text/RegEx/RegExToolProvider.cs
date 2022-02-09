#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.RegEx
{
    [Export(typeof(IToolProvider))]
    [Name("Regular Expression Tester")]
    [Parent(TextGroupToolProvider.InternalName)]
    [ProtocolName("regex")]
    [Order(1)]
    [NotScrollable]
    internal sealed class RegExToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.RegEx.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.RegEx.SearchDisplayName;

        public string? Description => LanguageManager.Instance.RegEx.Description;

        public string AccessibleName => LanguageManager.Instance.RegEx.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.RegEx.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("RegexTester.svg");

        [ImportingConstructor]
        public RegExToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<RegExToolViewModel>();
        }
    }
}
