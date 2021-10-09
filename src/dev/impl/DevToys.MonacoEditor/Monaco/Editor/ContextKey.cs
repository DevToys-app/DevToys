#nullable enable

using DevToys.MonacoEditor.CodeEditorControl;
using Newtonsoft.Json;
using System;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    public sealed class ContextKey : IContextKey
    {
        [JsonIgnore]
        private readonly WeakReference<CodeEditorCore> _editor;

        [JsonProperty("key")]
        public string Key { get; private set; }
        [JsonProperty("defaultValue")]
        public bool DefaultValue { get; private set; }
        [JsonProperty("value")]
        public bool Value { get; private set; }

        internal ContextKey(CodeEditorCore editor, string key, bool defaultValue)
        {
            _editor = new WeakReference<CodeEditorCore>(editor);

            Key = key;
            DefaultValue = defaultValue;
        }

        private async void UpdateValueAsync()
        {
            if (_editor.TryGetTarget(out CodeEditorCore editor))
            {
                await editor.InvokeScriptAsync("updateContext", new object[] { Key, Value });
            }
        }

        public bool Get()
        {
            return Value;
        }

        public void Reset()
        {
            Value = DefaultValue;

            UpdateValueAsync();
        }

        public void Set(bool value)
        {
            Value = value;

            UpdateValueAsync();
        }
    }
}
