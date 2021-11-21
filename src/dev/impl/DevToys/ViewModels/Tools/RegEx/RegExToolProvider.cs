#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;

namespace DevToys.ViewModels.Tools.RegEx
{
    [Export(typeof(IToolProvider))]
    [Name("Regular Expression Tester")]
    [ProtocolName("regex")]
    [Order(2)]
    [NotScrollable]
    internal sealed class RegExToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string? DisplayName => LanguageManager.Instance.RegEx.DisplayName;

        public object IconSource => CreatePathIconFromPath(nameof(RegExToolProvider));

        [ImportingConstructor]
        public RegExToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<RegExToolViewModel>();
        }
    }
}
