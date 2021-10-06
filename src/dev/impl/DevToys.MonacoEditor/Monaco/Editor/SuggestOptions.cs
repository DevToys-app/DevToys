#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Configuration options for editor suggest widget
    /// </summary>
    public sealed class SuggestOptions
    {
        /// <summary>
        /// Enable graceful matching. Defaults to true.
        /// </summary>
        [JsonProperty("filterGraceful", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FilterGraceful { get; set; }

        /// <summary>
        /// Controls the visibility of the status bar at the bottom of the suggest widget.
        /// </summary>
        [JsonProperty("hideStatusBar", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HideStatusBar { get; set; }

        /// <summary>
        /// Show a highlight when suggestion replaces or keep text after the cursor. Defaults to false.
        /// </summary>
        [JsonProperty("insertHighlight", NullValueHandling = NullValueHandling.Ignore)]
        public bool? InsertHighlight { get; set; }

        /// <summary>
        /// Overwrite word ends on accept. Default to false.
        /// </summary>
        [JsonProperty("insertMode", NullValueHandling = NullValueHandling.Ignore)]
        public InsertMode? InsertMode { get; set; }

        /// <summary>
        /// Favours words that appear close to the cursor.
        /// </summary>
        [JsonProperty("localityBonus", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LocalityBonus { get; set; }

        /// <summary>
        /// Max suggestions to show in suggestions. Defaults to 12.
        /// </summary>
        [JsonProperty("maxVisibleSuggestions", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxVisibleSuggestions { get; set; }

        /// <summary>
        /// Enable using global storage for remembering suggestions.
        /// </summary>
        [JsonProperty("shareSuggestSelections", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShareSuggestSelections { get; set; }

        /// <summary>
        /// Show class-suggestions.
        /// </summary>
        [JsonProperty("showClasses", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowClasses { get; set; }

        /// <summary>
        /// Show color-suggestions.
        /// </summary>
        [JsonProperty("showColors", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowColors { get; set; }

        /// <summary>
        /// Show constant-suggestions.
        /// </summary>
        [JsonProperty("showConstants", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowConstants { get; set; }

        /// <summary>
        /// Show constructor-suggestions.
        /// </summary>
        [JsonProperty("showConstructors", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowConstructors { get; set; }

        /// <summary>
        /// Show enumMember-suggestions.
        /// </summary>
        [JsonProperty("showEnumMembers", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowEnumMembers { get; set; }

        /// <summary>
        /// Show enum-suggestions.
        /// </summary>
        [JsonProperty("showEnums", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowEnums { get; set; }

        /// <summary>
        /// Show event-suggestions.
        /// </summary>
        [JsonProperty("showEvents", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowEvents { get; set; }

        /// <summary>
        /// Show field-suggestions.
        /// </summary>
        [JsonProperty("showFields", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowFields { get; set; }

        /// <summary>
        /// Show file-suggestions.
        /// </summary>
        [JsonProperty("showFiles", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowFiles { get; set; }

        /// <summary>
        /// Show folder-suggestions.
        /// </summary>
        [JsonProperty("showFolders", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowFolders { get; set; }

        /// <summary>
        /// Show function-suggestions.
        /// </summary>
        [JsonProperty("showFunctions", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowFunctions { get; set; }

        /// <summary>
        /// Enable or disable icons in suggestions. Defaults to true.
        /// </summary>
        [JsonProperty("showIcons", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowIcons { get; set; }

        /// <summary>
        /// Show interface-suggestions.
        /// </summary>
        [JsonProperty("showInterfaces", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowInterfaces { get; set; }

        /// <summary>
        /// Show keyword-suggestions.
        /// </summary>
        [JsonProperty("showKeywords", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowKeywords { get; set; }

        /// <summary>
        /// Show method-suggestions.
        /// </summary>
        [JsonProperty("showMethods", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowMethods { get; set; }

        /// <summary>
        /// Show module-suggestions.
        /// </summary>
        [JsonProperty("showModules", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowModules { get; set; }

        /// <summary>
        /// Show operator-suggestions.
        /// </summary>
        [JsonProperty("showOperators", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowOperators { get; set; }

        /// <summary>
        /// Show property-suggestions.
        /// </summary>
        [JsonProperty("showProperties", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowProperties { get; set; }

        /// <summary>
        /// Show reference-suggestions.
        /// </summary>
        [JsonProperty("showReferences", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowReferences { get; set; }

        /// <summary>
        /// Show snippet-suggestions.
        /// </summary>
        [JsonProperty("showSnippets", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowSnippets { get; set; }

        /// <summary>
        /// Show struct-suggestions.
        /// </summary>
        [JsonProperty("showStructs", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowStructs { get; set; }

        /// <summary>
        /// Show typeParameter-suggestions.
        /// </summary>
        [JsonProperty("showTypeParameters", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowTypeParameters { get; set; }

        /// <summary>
        /// Show unit-suggestions.
        /// </summary>
        [JsonProperty("showUnits", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowUnits { get; set; }

        /// <summary>
        /// Show value-suggestions.
        /// </summary>
        [JsonProperty("showValues", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowValues { get; set; }

        /// <summary>
        /// Show variable-suggestions.
        /// </summary>
        [JsonProperty("showVariables", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowVariables { get; set; }

        /// <summary>
        /// Show text-suggestions.
        /// </summary>
        [JsonProperty("showWords", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowWords { get; set; }

        /// <summary>
        /// Prevent quick suggestions when a snippet is active. Defaults to true.
        /// </summary>
        [JsonProperty("snippetsPreventQuickSuggestions", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SnippetsPreventQuickSuggestions { get; set; }
    }
}
