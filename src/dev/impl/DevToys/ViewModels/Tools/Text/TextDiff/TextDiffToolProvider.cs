#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;

namespace DevToys.ViewModels.Tools.TextDiff
{
    [Export(typeof(IToolProvider))]
    [Name("Text Diff")]
    [Parent(TextGroupToolProvider.InternalName)]
    [ProtocolName("diff")]
    [Order(2)]
    [NotScrollable]
    internal sealed class TextDiffToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.TextDiff.DisplayName;

        public string AccessibleName => LanguageManager.Instance.TextDiff.AccessibleName;

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
