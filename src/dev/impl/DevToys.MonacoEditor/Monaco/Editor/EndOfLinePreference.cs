#nullable enable

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// https://github.com/Microsoft/vscode/blob/master/src/vs/editor/common/editorCommon.ts#L228
    /// </summary>
    public enum EndOfLinePreference
    {
        TextDefined = 0,
        LF = 1,
        CRLF = 2
    }
}
