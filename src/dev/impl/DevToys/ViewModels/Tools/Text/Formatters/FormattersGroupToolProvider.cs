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
    [Parent(TextGroupToolProvider.InternalName)]
    [ProtocolName("formatters")]
    [Order(2)]
    internal sealed class FormattersGroupToolProvider : ToolProviderBase, IToolProvider
    {
        internal const string InternalName = "Formatters Group";

        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.ToolGroups.FormattersDisplayName;

        public string AccessibleName => LanguageManager.Instance.ToolGroups.FormattersAccessibleName;

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
        public FormattersGroupToolProvider(IMefProvider mefProvider)
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
