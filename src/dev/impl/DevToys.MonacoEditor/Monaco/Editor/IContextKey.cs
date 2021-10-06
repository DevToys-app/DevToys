#nullable enable

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// https://microsoft.github.io/monaco-editor/api/interfaces/monaco.editor.icontextkey.html
    /// 
    /// Supports bools only as Windows Runtime Type doesn't support generics.
    /// </summary>
    public interface IContextKey
    {
        /// <summary>
        /// Get the current value of the key.
        /// </summary>
        /// <returns></returns>
        bool Get();

        /// <summary>
        /// Resets the key to the default value.
        /// </summary>
        void Reset();

        /// <summary>
        /// Set the key to the specified value.
        /// </summary>
        /// <param name="value"></param>
        void Set(bool value);
    }
}
