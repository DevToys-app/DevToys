#nullable enable

using System.Composition;
using System.Text.RegularExpressions;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.MarkdownPreview
{
    [Export(typeof(IToolProvider))]
    [Name("Markdown Preview")]
    [Parent(TextGroupToolProvider.InternalName)]
    [ProtocolName("markdown")]
    [Order(4)]
    [NotScrollable]
    internal sealed class MarkdownPreviewToolProvider : ToolProviderBase, IToolProvider
    {
        private static readonly Regex MarkdownLinkDetection = new(@"\[[^]]+\]\(https?:\/\/\S+\)", RegexOptions.Compiled);

        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.MarkdownPreview.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.MarkdownPreview.SearchDisplayName;

        public string? Description => LanguageManager.Instance.MarkdownPreview.Description;

        public string AccessibleName => LanguageManager.Instance.MarkdownPreview.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.MarkdownPreview.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("MarkdownPreview.svg");

        [ImportingConstructor]
        public MarkdownPreviewToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return MarkdownLinkDetection.IsMatch(data);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<MarkdownPreviewToolViewModel>();
        }
    }
}
