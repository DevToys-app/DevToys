#nullable enable

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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

            if (!input.StartsWith("<") || !input.EndsWith(">"))
            {
                return false;
            }

            try
            {
                var xmlDocument = new XmlDocument();

                // If loading failed, it's not valid Xml.
                xmlDocument.LoadXml(input);

                return true;
            }
            catch (XmlException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Format a string to the specified Xml format.
        /// </summary>
        internal static string Format(string? input, Indentation indentationMode, bool newLineOnAttributes)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            input = input!.Trim();

            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(input);
                  
                var xmlWriterSettings = new XmlWriterSettings()
                {
                    Async = true,
                    OmitXmlDeclaration = xmlDocument.FirstChild.NodeType != XmlNodeType.XmlDeclaration,
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

                if (xmlDocument.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
                {
                    Match match = Regex.Match(xmlDocument.FirstChild.InnerText, @"(?<=encoding\s*=\s*"")[^""]*", RegexOptions.None);
                    if (match.Success)
                    {
                        stringBuilder = stringBuilder.Replace("utf-16", match.Value);
                    }
                    else
                    {
                        stringBuilder = stringBuilder.Replace("encoding=\"utf-16\"", "");
                    }
                }
                return stringBuilder.ToString();
            }
            catch (XmlException ex)
            {
                return ex.Message;
            }
            catch (Exception ex) // some other exception
            {
                Logger.LogFault("Xml formatter", ex, $"Indentation: {indentationMode}");
                return ex.Message;
            }
        }
    }
}
