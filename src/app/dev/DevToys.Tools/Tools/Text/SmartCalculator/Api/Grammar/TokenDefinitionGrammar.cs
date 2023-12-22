using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;

[DataContract]
public class TokenDefinitionGrammar
{
    [DataMember(Name = "common_tokens")]
    public Dictionary<string, string[]>? CommonTokens { get; set; }

    public static TokenDefinitionGrammar? Load(string json)
    {
        return JsonConvert.DeserializeObject<TokenDefinitionGrammar?>(json);
    }
}
