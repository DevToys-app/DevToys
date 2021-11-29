#nullable enable

using System.Composition;
using System.Text.RegularExpressions;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;

namespace DevToys.ViewModels.Tools.MarkdownPreview
{
    [Export(typeof(IToolProvider))]
    [Name("Markdown Preview")]
    [ProtocolName("markdown")]
    [Order(4)]
    [NotScrollable]
    internal sealed class MarkdownPreviewToolProvider : ToolProviderBase, IToolProvider
    {
        private static readonly Regex MarkdownLinkDetection = new(@"\[[^]]+\]\(https?:\/\/\S+\)", RegexOptions.Compiled);

        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.MarkdownPreview.DisplayName;

        public string AccessibleName => LanguageManager.Instance.MarkdownPreview.AccessibleName;

        public object IconSource => CreatePathIconFromPath(nameof(MarkdownPreviewToolProvider));

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
