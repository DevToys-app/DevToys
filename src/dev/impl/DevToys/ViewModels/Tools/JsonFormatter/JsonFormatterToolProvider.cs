#nullable enable

using DevToys.Api.Core.Injection;
using DevToys.Api.Tools;
using DevToys.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Composition;

namespace DevToys.ViewModels.Tools.JsonFormatter
{
    [Export(typeof(IToolProvider))]
    [Name("Json Formatter")]
    [ProtocolName("jsonformat")]
    [Order(0)]
    [NotScrollable]
    internal sealed class JsonFormatterToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string DisplayName => LanguageManager.Instance.JsonFormatter.DisplayName;

        public object IconSource => CreatePathIconFromPath(nameof(JsonFormatterToolProvider));

        [ImportingConstructor]
        public JsonFormatterToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return IsValidJson(data);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<JsonFormatterToolViewModel>();
        }

        private bool IsValidJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input.Trim();

            if ((input.StartsWith("{") && input.EndsWith("}")) //For object
                || (input.StartsWith("[") && input.EndsWith("]"))) //For array
            {
                try
                {
                    JToken? jtoken = JToken.Parse(input);
                    return jtoken is not null;
                }
                catch (JsonReaderException)
                {
                    // Exception in parsing json. It likely mean the text isn't a JSON.
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Logger.LogFault("Check is string if JSON", ex);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
