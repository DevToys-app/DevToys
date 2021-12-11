#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.TextDiff
{
    [Export(typeof(IToolProvider))]
    [Name("Text Diff")]
    [Parent(TextGroupToolProvider.InternalName)]
    [ProtocolName("diff")]
    [Order(1)]
    [NotScrollable]
    internal sealed class TextDiffToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.TextDiff.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.TextDiff.SearchDisplayName;

        public string? Description => LanguageManager.Instance.TextDiff.Description;

        public string AccessibleName => LanguageManager.Instance.TextDiff.AccessibleName;

        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("TextDiff.svg");

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
