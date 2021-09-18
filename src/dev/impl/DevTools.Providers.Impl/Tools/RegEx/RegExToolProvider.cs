#nullable enable

using DevTools.Common;
using DevTools.Core.Injection;
using DevTools.Core.Threading;
using System.Composition;

namespace DevTools.Providers.Impl.Tools.RegEx
{
    [Export(typeof(IToolProvider))]
    [Name("Regular Expression Tester")]
    [ProtocolName("regex")]
    [Order(0)]
    [NotScrollable]
    internal sealed class RegExToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IThread _thread;
        private readonly IMefProvider _mefProvider;

        public string? DisplayName => LanguageManager.Instance.RegEx.DisplayName;

        public object IconSource => CreatePathIconFromPath(nameof(RegExToolProvider));

        [ImportingConstructor]
        public RegExToolProvider(IThread thread, IMefProvider mefProvider)
            : base(thread)
        {
            _thread = thread;
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
