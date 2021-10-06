#nullable enable

using DevToys.MonacoEditor.Helpers;
using DevToys.MonacoEditor.Monaco.Editor;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DevToys.MonacoEditor
{
    public interface ICodeEditor
    {
        ParentAccessor? ParentAccessor { get; }

        IModel? GetModel();

        Task InvokeScriptAsync(
            string method,
            object arg,
            bool serialize = true,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0);

        Task InvokeScriptAsync(
            string method,
            object[] args,
            bool serialize = true,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0);

        Task<T?> InvokeScriptAsync<T>(
            string method,
            object arg,
            bool serialize = true,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0);

        Task<T?> InvokeScriptAsync<T>(
            string method,
            object[] args,
            bool serialize = true,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0);

        Task SendScriptAsync(string script,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0);

        Task<T?> SendScriptAsync<T>(string script,
            [CallerMemberName] string? member = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0);

        bool TriggerKeyDown(WebKeyEventArgs args);
    }
}
