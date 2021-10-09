#nullable enable

using System;

namespace DevToys.MonacoEditor.Extensions
{
    internal sealed class JavaScriptInnerException : Exception
    {
        public string JavaScriptStackTrace { get; private set; } // TODO Use Enum of JS error types https://www.w3schools.com/js/js_errors.asp

        public JavaScriptInnerException(string message, string stack)
            : base(message + "\r\n" + stack)
        {
            JavaScriptStackTrace = stack;
        }
    }
}
