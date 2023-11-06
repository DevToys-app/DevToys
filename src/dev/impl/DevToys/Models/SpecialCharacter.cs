namespace DevToys.Models
{
    /// <summary>
    /// Special characters. Value corresponds to ASCII code. Exceptions with no ASCII representation have a negative value.
    /// </summary>
    public enum SpecialCharacter
    {
        /// <summary>
        /// \r\n
        /// </summary>
        CarriageReturnLineFeed = -1,
        /// <summary>
        /// \b
        /// </summary>
        Backspace = 8,
        /// <summary>
        /// \t
        /// </summary>
        HorizontalTab = 9,
        /// <summary>
        /// \n
        /// </summary>
        LineFeed = 10,
        /// <summary>
        /// \f
        /// </summary>
        FormFeed = 12,
        /// <summary>
        /// \r
        /// </summary>
        CarriageReturn = 13,
        /// <summary>
        /// "
        /// </summary>
        DoubleQuote = 34,
        /// <summary>
        /// \
        /// </summary>
        Backslash = 92
    }
}


