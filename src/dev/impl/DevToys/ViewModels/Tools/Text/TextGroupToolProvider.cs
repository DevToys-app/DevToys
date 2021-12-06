#nullable enable

using System.Composition;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using DevToys.ViewModels.Tools.TextDiff;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools
{
    [Export(typeof(IToolProvider))]
    [Name(InternalName)]
    [ProtocolName("text")]
    [Order(2)]
    internal sealed class TextGroupToolProvider : ToolProviderBase, IToolProvider
    {
        internal const string InternalName = "Text Group";

        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.ToolGroups.TextDisplayName;

        public string AccessibleName => LanguageManager.Instance.ToolGroups.TextAccessibleName;

        public object IconSource
            => new TaskCompletionNotifier<IconElement>(
                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    return Task.FromResult<IconElement>(
                        new FontIcon
                        {
                            Glyph = "\uF2BB" // TODO
                        });
                }));

        [ImportingConstructor]
        public TextGroupToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            // TODO: Create a view model specific to a group.
            return _mefProvider.Import<TextDiffToolViewModel>();
        }
    }
}
