#nullable enable

namespace DevToys.MonacoEditor.Monaco.Languages
{
    /// <summary>
    /// How a suggest provider was triggered.
    /// </summary>
    public enum CompletionTriggerKind
    {
        Invoke = 0,
        TriggerCharacter = 1,
        TriggerForIncompleteCompletions = 2
    }
}
