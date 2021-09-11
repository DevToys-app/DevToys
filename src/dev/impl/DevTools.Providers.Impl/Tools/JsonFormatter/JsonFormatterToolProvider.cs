#nullable enable

using DevTools.Core.Threading;
using DevTools.Common;
using System;
using System.Composition;
using Windows.ApplicationModel.DataTransfer;

namespace DevTools.Providers.Impl.Tools.JsonFormatter
{
    [Export(typeof(IToolProvider))]
    [Name("Json Formatter")]
    [ProtocolName("jsonformat")]
    [Order(0)]
    internal sealed class JsonFormatterToolProvider : ToolProviderBase, IToolProvider
    {
        public string DisplayName => LanguageManager.Instance.JsonFormatter.DisplayName;

        public object IconSource { get; }

        [ImportingConstructor]
        public JsonFormatterToolProvider(IThread thread)
            : base(thread)
        {
            IconSource = CreatePathIconFromPath(nameof(JsonFormatterToolProvider));
        }

        public bool CanBeTreatedByTool(string data)
        {
            // TODO.
            return false;
        }

        public IToolViewModel CreateTool()
        {
            throw new NotImplementedException();
        }
    }
}
