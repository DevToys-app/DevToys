#nullable enable

using DevToys.Api.Core.Injection;
using DevToys.Api.Tools;
using DevToys.Helpers;
using System.Composition;

namespace DevToys.ViewModels.Tools.JsonYaml
{
    [Export(typeof(IToolProvider))]
    [Name("Json <> Yaml")]
    [ProtocolName("jsonyaml")]
    [Order(0)]
    [NotScrollable]
    internal sealed class JsonYamlToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.JsonYaml.DisplayName;

        public object IconSource => CreatePathIconFromPath(nameof(JsonYamlToolProvider));

        [ImportingConstructor]
        public JsonYamlToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return JsonHelper.IsValidJson(data) || YamlHelper.IsValidYaml(data);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<JsonYamlToolViewModel>();
        }
    }
}
