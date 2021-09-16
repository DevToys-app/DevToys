#nullable enable

using DevTools.Common;
using DevTools.Core.Injection;
using DevTools.Core.Threading;
using System.Composition;

namespace DevTools.Providers.Impl.Tools.TextDiff
{
    [Export(typeof(IToolProvider))]
    [Name("Text Diff")]
    [ProtocolName("diff")]
    [Order(0)]
    [NotScrollable]
    internal sealed class TextDiffToolProvider : ToolProviderBase, IToolProvider
    {
        public string DisplayName => LanguageManager.Instance.TextDiff.DisplayName;

        public object IconSource => CreatePathIconFromPath(nameof(TextDiffToolProvider));

        private readonly IMefProvider _mefProvider;

        [ImportingConstructor]
        public TextDiffToolProvider(IThread thread, IMefProvider mefProvider)
            : base(thread)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<TextDiffToolViewModel>();
        }
    }
}
