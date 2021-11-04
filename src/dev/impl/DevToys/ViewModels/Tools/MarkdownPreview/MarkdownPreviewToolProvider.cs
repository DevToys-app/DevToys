#nullable enable

using DevToys.Api.Core.Injection;
using DevToys.Api.Tools;
using System.Composition;
using System.Text.RegularExpressions;

namespace DevToys.ViewModels.Tools.MarkdownPreview
{
    [Export(typeof(IToolProvider))]
    [Name("Markdown Preview")]
    [ProtocolName("markdown")]
    [Order(4)]
    [NotScrollable]
    internal sealed class MarkdownPreviewToolProvider : ToolProviderBase, IToolProvider
    {
        private static readonly Regex MarkdownLinkDetection = new Regex(@"\[[^]]+\]\(https?:\/\/\S+\)", RegexOptions.Compiled);

        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.MarkdownPreview.DisplayName;

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
