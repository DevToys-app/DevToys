#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;
using DevToys.Helpers.JsonYaml;

namespace DevToys.ViewModels.Tools.JsonFormatter
{
    [Export(typeof(IToolProvider))]
    [Name("Json Formatter")]
    [Parent(FormattersGroupToolProvider.InternalName)]
    [ProtocolName("jsonformat")]
    [Order(0)]
    [NotScrollable]
    internal sealed class JsonFormatterToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.JsonFormatter.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.JsonFormatter.SearchDisplayName;

        public string? Description => LanguageManager.Instance.JsonFormatter.Description;

        public string AccessibleName => LanguageManager.Instance.JsonFormatter.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.JsonFormatter.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("JsonFormatter.svg");

        [ImportingConstructor]
        public JsonFormatterToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return JsonHelper.IsValid(data);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<JsonFormatterToolViewModel>();
        }
    }
}
