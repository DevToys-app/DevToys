#nullable enable

using DevTools.Common;
using DevTools.Core.Injection;
using DevTools.Core.Threading;
using System.Composition;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace DevTools.Providers.Impl.Tools.HashGenerator
{
    [Export(typeof(IToolProvider))]
    [Name("Hash Generator")]
    [ProtocolName("hash")]
    [Order(0)]
    internal sealed class HashGeneratorToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IThread _thread;
        private readonly IMefProvider _mefProvider;

        public string? DisplayName => LanguageManager.Instance.HashGenerator.DisplayName;

        public object IconSource
            => new TaskCompletionNotifier<IconElement>(
                _thread,
                _thread.RunOnUIThreadAsync(() =>
                {
                    return Task.FromResult<IconElement>(
                        new FontIcon
                        {
                            Glyph = "\uF409"
                        });
                }));

        [ImportingConstructor]
        public HashGeneratorToolProvider(IThread thread, IMefProvider mefProvider)
            : base(thread)
        {
            _thread = thread;
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
