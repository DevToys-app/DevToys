#nullable enable

using DevTools.Common;
using DevTools.Core.Injection;
using DevTools.Core.Threading;
using System.Composition;

namespace DevTools.Providers.Impl.Tools.JsonFormatter
{
    [Export(typeof(IToolProvider))]
    [Name("Json Formatter")]
    [ProtocolName("jsonformat")]
    [Order(0)]
    internal sealed class JsonFormatterToolProvider : ToolProviderBase, IToolProvider
    {
        public string DisplayName => LanguageManager.Instance.JsonFormatter.DisplayName;

        public object IconSource => CreatePathIconFromPath(nameof(JsonFormatterToolProvider));

        private readonly IMefProvider _mefProvider;

        [ImportingConstructor]
        public JsonFormatterToolProvider(IThread thread, IMefProvider mefProvider)
            : base(thread)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            // TODO.
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<JsonFormatterToolViewModel>();
        }
    }
}
