#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;
using DevToys.Helpers.JsonYaml;

namespace DevToys.ViewModels.Tools.JsonYaml
{
    [Export(typeof(IToolProvider))]
    [Name("Json <> Yaml")]
    [Parent(ConvertersGroupToolProvider.InternalName)]
    [ProtocolName("jsonyaml")]
    [Order(0)]
    [NotScrollable]
    internal sealed class JsonYamlToolProvider : IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.JsonYaml.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.JsonYaml.SearchDisplayName;

        public string? Description => LanguageManager.Instance.JsonYaml.Description;

        public string AccessibleName => LanguageManager.Instance.JsonYaml.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.JsonYaml.SearchKeywords;

        public string IconGlyph => "\u0109";

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
