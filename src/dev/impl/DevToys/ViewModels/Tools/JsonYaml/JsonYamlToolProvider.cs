#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Helpers;

namespace DevToys.ViewModels.Tools.JsonYaml
{
    [Export(typeof(IToolProvider))]
    [Name("Json <> Yaml")]
    [ProtocolName("jsonyaml")]
    [Order(1)]
    [NotScrollable]
    internal sealed class JsonYamlToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.JsonYaml.DisplayName;

        public string AccessibleName => LanguageManager.Instance.JsonYaml.AccessibleName;

        public object IconSource => CreatePathIconFromPath(nameof(JsonYamlToolProvider));

        [ImportingConstructor]
        public JsonYamlToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return JsonHelper.IsValid(data) || YamlHelper.IsValidYaml(data);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<JsonYamlToolViewModel>();
        }
    }
}
