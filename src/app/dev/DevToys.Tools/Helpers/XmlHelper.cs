using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DevToys.Tools.Models;
using Markdig.Helpers;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers;

internal static class XmlHelper
{
    /// <summary>
    /// Detects whether the given string is a valid Xml or not.
    /// </summary>
    internal static bool IsValid(string? input, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (!ValidateFirstAndLastCharOfXml(input))
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid data detected 'input'", input);
            return false;
        }
    }

    /// <summary>
    /// Format a string to the specified Xml format.
    /// </summary>
    internal static ResultInfo<string> Format(string? input, Indentation indentationMode, bool newLineOnAttributes, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new(string.Empty, false);
        }

        try
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(input);

            if (xmlDocument.FirstChild == null)
            {
                return new(string.Empty, false);
            }

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
            return new(stringBuilder.ToString());
        }
        catch (XmlException ex)
        {
            return new(ex.Message, false);
        }
        catch (Exception ex) // some other exception
        {
            logger.LogError(ex, "Xml formatter", $"Indentation: {indentationMode}");
            return new(ex.Message, false);
        }
    }

    /// <summary>
    /// Validate that the XML starts with "<" and ends with ">", ignoring whitespace
    /// </summary>
    private static bool ValidateFirstAndLastCharOfXml(string input)
    {
        for (int i = 0; i < input.Length; ++i)
        {
            if (!input[i].IsWhitespace())
            {
                if (input[i] == '<')
                {
                    break;
                }
                return false;
            }
        }

        for (int i = input.Length - 1; i >= 0; --i)
        {
            if (!input[i].IsWhitespace())
            {
                if (input[i] == '>')
                {
                    return true;
                }
                return false;
            }
        }

        return false;
    }
}
