#nullable enable

using DevToys.Api.Core.Injection;
using DevToys.Api.Tools;
using System.Composition;

namespace DevToys.ViewModels.Tools.TextDiff
{
    [Export(typeof(IToolProvider))]
    [Name("Text Diff")]
    [ProtocolName("diff")]
    [Order(2)]
    [NotScrollable]
    internal sealed class TextDiffToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.TextDiff.DisplayName;

        public object IconSource => CreatePathIconFromPath(nameof(TextDiffToolProvider));

        [ImportingConstructor]
        public TextDiffToolProvider(IMefProvider mefProvider)
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
