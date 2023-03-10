namespace DevToys.Business.Models;

// Copy of Windows.UI.Xaml.Controls.AutoSuggestionBoxTextChangeReason
internal enum SearchBoxTextChangedReason
{
    /// <summary>
    /// The user edited the text.
    /// </summary>
    UserInput = 0,

    /// <summary>
    /// The text was changed via code.
    /// </summary>
    ProgrammaticChange = 1,

    /// <summary>
    /// The user selected one of the items in the auto-suggestion box.
    /// </summary>
    SuggestionChosen = 2
}
