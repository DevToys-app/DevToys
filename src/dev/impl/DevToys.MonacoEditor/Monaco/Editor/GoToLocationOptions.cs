#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Configuration options for go to location
    /// </summary>
    public sealed class GoToLocationOptions
    {
        [JsonProperty("alternativeDeclarationCommand", NullValueHandling = NullValueHandling.Ignore)]
        public string? AlternativeDeclarationCommand { get; set; }

        [JsonProperty("alternativeDefinitionCommand", NullValueHandling = NullValueHandling.Ignore)]
        public string? AlternativeDefinitionCommand { get; set; }

        [JsonProperty("alternativeImplementationCommand", NullValueHandling = NullValueHandling.Ignore)]
        public string? AlternativeImplementationCommand { get; set; }

        [JsonProperty("alternativeReferenceCommand", NullValueHandling = NullValueHandling.Ignore)]
        public string? AlternativeReferenceCommand { get; set; }

        [JsonProperty("alternativeTypeDefinitionCommand", NullValueHandling = NullValueHandling.Ignore)]
        public string? AlternativeTypeDefinitionCommand { get; set; }

        [JsonProperty("multiple", NullValueHandling = NullValueHandling.Ignore)]
        public Multiple? Multiple { get; set; }

        [JsonProperty("multipleDeclarations", NullValueHandling = NullValueHandling.Ignore)]
        public Multiple? MultipleDeclarations { get; set; }

        [JsonProperty("multipleDefinitions", NullValueHandling = NullValueHandling.Ignore)]
        public Multiple? MultipleDefinitions { get; set; }

        [JsonProperty("multipleImplementations", NullValueHandling = NullValueHandling.Ignore)]
        public Multiple? MultipleImplementations { get; set; }

        [JsonProperty("multipleReferences", NullValueHandling = NullValueHandling.Ignore)]
        public Multiple? MultipleReferences { get; set; }

        [JsonProperty("multipleTypeDefinitions", NullValueHandling = NullValueHandling.Ignore)]
        public Multiple? MultipleTypeDefinitions { get; set; }
    }

}
