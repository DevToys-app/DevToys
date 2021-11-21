#nullable enable

using System.Composition;
using System.Threading.Tasks;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.StringUtilities
{
    [Export(typeof(IToolProvider))]
    [Name("String Utilities")]
    [ProtocolName("string")]
    [Order(2)]
    [NotScrollable]
    internal sealed class StringUtilitiesToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string? DisplayName => LanguageManager.Instance.StringUtilities.DisplayName;

        public object IconSource
            => new TaskCompletionNotifier<IconElement>(
                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    return Task.FromResult<IconElement>(
                        new FontIcon
                        {
                            Glyph = "\uF793"
                        });
                }));

        [ImportingConstructor]
        public StringUtilitiesToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<StringUtilitiesToolViewModel>();
        }
    }
}
