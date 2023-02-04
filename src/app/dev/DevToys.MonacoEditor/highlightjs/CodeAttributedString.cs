#if __MACCATALYST__

using DevToys.UI.Framework.Threading;
using Microsoft.UI.Dispatching;

namespace DevToys.MonacoEditor.HighlightJs;

/// <summary>
/// NSTextStorage subclass. Can be used to dynamically highlight code.
/// </summary>
internal sealed class CodeAttributedString : NSTextStorage
{
    private readonly DispatcherQueue _dispatcherQueue;

    private string _language = "json";

    /// <summary>
    /// Internal storage.
    /// </summary>
    private readonly NSTextStorage _stringStorage = new();

    // HighlightR instance used internally for highlighting. Use this for configuring the theme.
    private readonly Highlighter _highlightr;

    public CodeAttributedString(DispatcherQueue dispatcherQueue, Highlighter? highlighter = null)
    {
        Guard.IsNotNull(dispatcherQueue);
        _dispatcherQueue = dispatcherQueue;

        _highlightr = highlighter ?? new Highlighter();
    }

    internal string Language
    {
        get => _language;
        set
        {
            _language = value.ToLower();
            _ = HighlightAsync(new NSRange(0, _stringStorage.Length));
        }
    }

    public override nint LowLevelValue => _stringStorage.LowLevelValue;

    public override NSMutableString MutableString => _stringStorage.MutableString;

    public new string Value => _stringStorage.Value;

    public override void SetString(NSAttributedString attrString)
    {
        _stringStorage.SetString(attrString);
        Edited(NSTextStorageEditActions.Characters, new NSRange(0, attrString.Length), delta: 0);
    }

    public override void Append(NSAttributedString attrString)
    {
        base.Append(attrString);
    }

    public override void Insert(NSAttributedString attrString, nint location)
    {
        base.Insert(attrString, location);
    }

    public override void Replace(NSRange range, NSAttributedString value)
    {
        base.Replace(range, value);
    }

    public override void DeleteRange(NSRange range)
    {
        base.DeleteRange(range);
    }

    /// <summary>
    /// Replaces the characters at the given range with the provided string.
    /// </summary>
    public override void Replace(NSRange range, string newValue)
    {
        _stringStorage.Replace(range, newValue);
        Edited(NSTextStorageEditActions.Characters, range, delta: newValue.Length - range.Length);
    }

    /// <summary>
    /// Returns the attributes for the character at a given index.
    /// </summary>
    public override nint LowLevelGetAttributes(nint location, nint effectiveRange)
    {
        return _stringStorage.LowLevelGetAttributes(location, effectiveRange);
    }

    /// <summary>
    /// Sets the attributes for the characters in the specified range to the given attributes.
    /// </summary>
    public override void LowLevelSetAttributes(nint dictionaryAttrsHandle, NSRange range)
    {
        _stringStorage.LowLevelSetAttributes(dictionaryAttrsHandle, range);
        Edited(NSTextStorageEditActions.Attributes, range, delta: 0);
    }

    /// <summary>
    /// Called internally everytime the string is modified.
    /// </summary>
    public override void ProcessEditing()
    {
        base.ProcessEditing();
        if (!string.IsNullOrEmpty(Language))
        {
            if ((EditedMask & NSTextStorageEditActions.Characters) == NSTextStorageEditActions.Characters)
            {
                NSRange rangeToHighlight = MutableString.GetParagraphRange(EditedRange);
                _ = HighlightAsync(rangeToHighlight);
            }
        }
    }

    private async Task HighlightAsync(NSRange range)
    {
        if (string.IsNullOrEmpty(Language))
        {
            return;
        }

        NSMutableString text = MutableString;

        // TODO: Use spans instead??
        string line = text.ToString().Substring((int)range.Location, (int)range.Length);

        NSAttributedString? colorizedString = await _highlightr.HighlightAsync(line, Language, fastRender: true);

        await _dispatcherQueue.RunOnUIThreadAsync(() =>
        {
            //Checks to see if this highlighting is still valid.
            if (range.Location + range.Length > _stringStorage.Length)
            {
                return;
            }

            if (!string.Equals(colorizedString?.Value, _stringStorage.Substring(range.Location, range.Length).Value, StringComparison.Ordinal))
            {
                return;
            }

            BeginEditing();

            colorizedString?.EnumerateAttributes(
                new NSRange(0, colorizedString.Length),
                NSAttributedStringEnumeration.None,
                (NSDictionary attrs, NSRange locRange, ref bool _) =>
                {
                    var fixedRange = new NSRange(range.Location + locRange.Location, locRange.Length);
                    fixedRange.Length = fixedRange.Location + fixedRange.Length < text.Length ? fixedRange.Length : text.Length - fixedRange.Location;
                    fixedRange.Length = fixedRange.Length < 0 ? 0 : fixedRange.Length;
                    _stringStorage.LowLevelSetAttributes(attrs.Handle, fixedRange);
                });

            EndEditing();
            Edited(NSTextStorageEditActions.Attributes, range, delta: 0);
        });
    }
}

#endif
