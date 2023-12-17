// Forked from https://github.com/alexcpendleton/NLipsum

using System.Text;
using System.Xml.Linq;

namespace DevToys.Tools.Helpers.LoremIpsum;

internal static class LipsumUtilities
{
    /// <summary>
    /// Reads raw Xml and grabs the &lt;text&gt; node's inner text.
    /// </summary>
    /// <param name="rawXml">The Xml to be parsed.  
    /// It should follow the lipsum.dtd but really all it needs 
    /// a text node as a child of the document element. 
    /// (&lt;root&gt;&lt;text&gt;your lipsum text&lt;/text&gt;&lt;/root&gt;
    /// </param>
    /// <returns></returns>
    internal static StringBuilder GetTextFromRawXml(string rawXml)
    {
        var text = new StringBuilder();
        XDocument data = LoadXmlDocument(rawXml);
        Guard.IsNotNull(data.Root);
        XElement? textNode = data.Root.Element("text");
        if (textNode != null)
        {
            text.Append(textNode.Value);
        }

        return text;
    }

    /// <summary>
    /// Creates an XDocument from a string.
    /// </summary>
    /// <param name="rawXml">The Xml from which to load the XDocument</param>
    /// <returns></returns>
    private static XDocument LoadXmlDocument(string rawXml)
    {
        var document = XDocument.Parse(rawXml);
        return document;
    }

    /* Is this the best way to do this?
     * It seems smelly to have a static member 
     * for randoms.	
     */
    private static readonly Random rand = new();

    /// <summary>
    /// Get a random integer.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns></returns>
    internal static int RandomInt(int min, int max)
    {
        int i = rand.Next(min, max);
        return i;
    }

    /// <summary>
    /// Gets a random element from a string array.
    /// </summary>
    /// <param name="source">The array from which to retrieve a random element.</param>
    /// <returns></returns>
    internal static string RandomElement(IReadOnlyList<string> source)
    {
        return source[RandomInt(0, source.Count - 1)];
    }

    /// <summary>
    /// Removes empty elements from an array.
    /// </summary>
    /// <param name="source">The array from which to remove empty items.</param>
    /// <returns></returns>
    internal static IReadOnlyList<string> RemoveEmptyElements(string[] source)
    {
        var results = new List<string>();
        int length = source.Length;

        for (int i = 0; i < length; i++)
        {
            if (source[i] != null && source[i].Trim().Length > 0)
            {
                results.Add(source[i]);
            }
        }

        return results;
    }
}
