#nullable enable

using System;

namespace DevToys.MonacoEditor.Extensions
{
    internal sealed class JavaScriptExecutionException : Exception
    {
        public string Script { get; private set; }

        public string? Member { get; private set; }

        public string? FileName { get; private set; }

        public int LineNumber { get; private set; }

        public JavaScriptExecutionException(string? member, string? filename, int line, string script, Exception inner)
            : base("Error Executing JavaScript Code for " + member + "\nLine " + line + " of " + filename + "\n" + script + "\n", inner)
        {
            Member = member;
            FileName = filename;
            LineNumber = line;
            Script = script;
        }
    }
}
