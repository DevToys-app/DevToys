#nullable enable

using System;
using System.IO;
using System.Text;
using System.Xml;
using DevToys.Core;
using DevToys.Models;

namespace DevToys.Helpers
{
    internal static class XmlHelper
    {
        /// <summary>
        /// Detects whether the given string is a valid Xml or not.
        /// </summary>
        internal static bool IsValid(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            input = input!.Trim();

            if (input.StartsWith("<") && input.EndsWith(">"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Format a string to the specified Xml format.
        /// </summary>
        internal static string Format(string? input, Indentation indentationMode, bool newLineOnAttributes)
        {
            if (!IsValid(input))
            {
                return string.Empty;
            }

            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(new StringReader(input));

                var xmlWriterSettings = new XmlWriterSettings()
                {
                    Async = true,
                    NewLineOnAttributes = newLineOnAttributes,
                };

                switch (indentationMode)
                {
                    case Indentation.TwoSpaces:
                        xmlWriterSettings.Indent = true;
                        xmlWriterSettings.IndentChars = "  ";
                        break;
                    case Indentation.FourSpaces:
                        xmlWriterSettings.Indent = true;
                        xmlWriterSettings.IndentChars = "    ";
                        break;
                    case Indentation.OneTab:
                        xmlWriterSettings.Indent = true;
                        xmlWriterSettings.IndentChars = "\t";
                        break;
                    case Indentation.Minified:
                        xmlWriterSettings.Indent = false;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                var stringBuilder = new StringBuilder();
                using (var xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings))
                {
                    xmlDocument.Save(xmlWriter);
                }

                return stringBuilder.ToString();
            }
            catch (XmlException ex)
            {
                return ex.Message;
            }
            catch (Exception ex) //some other exception
            {
                Logger.LogFault("Xml formatter", ex, $"Indentation: {indentationMode}");
                return ex.Message;
            }
        }
    }
}
