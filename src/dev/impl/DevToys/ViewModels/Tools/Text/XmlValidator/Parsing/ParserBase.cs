using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    internal abstract class ParserBase<TResult>
    {
        protected readonly string Source;
        private readonly List<string> _validationWarnings;
        private readonly List<string> _validationErrors;

        protected bool HasValidationSucceeded { get; private set; }

        protected ParserBase(string source)
        {
            HasValidationSucceeded = true;

            Source = source;
            _validationWarnings = new List<string>();
            _validationErrors = new List<string>();
        }

        internal abstract TResult Parse(string content);

        internal void ValidationErrorCallBack(object sender, ValidationEventArgs e)
        {
            string information =
                FormatErrorMessage(Source, e.Exception.LineNumber, e.Exception.LinePosition, e.Message);

            switch (e.Severity)
            {
                case XmlSeverityType.Warning:
                    _validationWarnings.Add("Warning " + information);
                    break;
                case XmlSeverityType.Error:
                    _validationErrors.Add("Error " + information);
                    HasValidationSucceeded = false;
                    break;
            }
        }

        internal static string FormatErrorMessage(string source, int lineNumber, int linePosition, string message)
        {
            string information =
                $"[Source: {source}, line: {lineNumber}, position: {linePosition}]: {message}";
            return information;
        }

        protected string GetValidationMessages()
        {
            string messages = string.Join(Environment.NewLine, _validationErrors);
            messages += string.Join(Environment.NewLine, _validationWarnings);
            return messages;
        }
    }
}
