#nullable enable

using DevToys.Api.Core.Injection;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using System.Composition;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.HashGenerator
{
    [Export(typeof(IToolProvider))]
    [Name("Hash Generator")]
    [ProtocolName("hash")]
    [Order(0)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal sealed class HashGeneratorToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string? DisplayName => LanguageManager.Instance.HashGenerator.DisplayName;

        public object IconSource
            => new TaskCompletionNotifier<IconElement>(
                ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    return Task.FromResult<IconElement>(
                        new FontIcon
                        {
                            Glyph = "\uF409"
                        });
                }));

        [ImportingConstructor]
        public HashGeneratorToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<HashGeneratorToolViewModel>();
        }
    }
}
