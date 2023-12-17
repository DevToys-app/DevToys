// Forked from https://github.com/alexcpendleton/NLipsum

namespace DevToys.Tools.Helpers.LoremIpsum;

/// <summary>
/// Represents a part or section of text, such as a paragraph or sentence.
/// </summary>
internal abstract class TextFeature
{
    private uint _minimumValue = 0;
    private uint _maximumValue = 0;
    private string _formatString = string.Empty;

    private string _delimiter = " ";

    /// <summary>
    /// Gets or sets the minimum amount of sub features in this feature.
    /// </summary>
    protected uint MinimumValue
    {
        get => _minimumValue;
        set => _minimumValue = value;
    }

    /// <summary>
    /// Gets or sets the maximum amount of sub features in this feature.
    /// </summary>
    protected uint MaximumValue
    {
        get => _maximumValue;
        set => _maximumValue = value;
    }

    /// <summary>
    /// Gets or sets how this feature should be rendered.  By default: "{0}." (ends with a period.)  
    /// For an html tag you could use "&lt;div&gt;{0}&lt;/div&gt;".  
    /// You get the picture.
    /// </summary>
    internal string FormatString
    {
        get => _formatString;
        set => _formatString = value;
    }

    /// <summary>
    /// Gets or sets the delimiter between the subparts.
    /// </summary>
    internal string Delimiter
    {
        get => _delimiter;
        set => _delimiter = value;
    }

    /// <summary>
    /// Formats this feature based on its FormatString.
    /// </summary>
    /// <param name="text">The text with which to format the string.</param>
    /// <returns></returns>
    internal virtual string Format(string text)
    {
        return string.Format(FormatString, text);
    }

    /* This kind of smells */
    /// <summary>
    /// Gets the minimum sub feature value.
    /// </summary>
    /// <returns></returns>
    internal uint GetMinimum() { return MinimumValue; }

    /// <summary>
    /// Gets the maximum sub feature value.
    /// </summary>
    /// <returns></returns>
    internal uint GetMaximum() { return MaximumValue; }
}
