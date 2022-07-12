#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.SqlFormatter
{
    [Export(typeof(IToolProvider))]
    [Name("SQL Formatter")]
    [Parent(FormattersGroupToolProvider.InternalName)]
    [ProtocolName("sqlformat")]
    [Order(0)]
    [NotScrollable]
    internal sealed class SqlFormatterToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.SqlFormatter.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.SqlFormatter.SearchDisplayName;

        public string? Description => LanguageManager.Instance.SqlFormatter.Description;

        public string AccessibleName => LanguageManager.Instance.SqlFormatter.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.SqlFormatter.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("SqlFormatter.svg");

        [ImportingConstructor]
        public SqlFormatterToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<SqlFormatterToolViewModel>();
        }
    }
}
