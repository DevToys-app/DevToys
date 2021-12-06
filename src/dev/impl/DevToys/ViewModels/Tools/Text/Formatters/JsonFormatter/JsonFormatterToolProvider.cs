#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Helpers;

namespace DevToys.ViewModels.Tools.JsonFormatter
{
    [Export(typeof(IToolProvider))]
    [Name("Json Formatter")]
    [Parent(FormattersGroupToolProvider.InternalName)]
    [ProtocolName("jsonformat")]
    [Order(1)]
    [NotScrollable]
    internal sealed class JsonFormatterToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.JsonFormatter.DisplayName;

        public string AccessibleName => LanguageManager.Instance.JsonFormatter.AccessibleName;

        public object IconSource => CreatePathIconFromPath(nameof(JsonFormatterToolProvider));

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
