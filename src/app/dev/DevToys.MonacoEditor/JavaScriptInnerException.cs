namespace DevToys.MonacoEditor;

internal sealed class JavaScriptInnerException : Exception
{
    public string JavaScriptStackTrace { get; private set; } // TODO Use Enum of JS error types https://www.w3schools.com/js/js_errors.asp

    public JavaScriptInnerException(string message, string stack)
        : base(message)
    {
        JavaScriptStackTrace = stack;
    }
}
