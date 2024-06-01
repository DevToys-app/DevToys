namespace DevToys.Api;

// See https://code.visualstudio.com/api/language-extensions/semantic-highlight-guide#standard-token-types-and-modifiers
/// <summary>
/// Represents a semantic token modifier.
/// </summary>
public enum SemanticTokenModifier
{
    /// <summary>
    /// For declarations of symbols.
    /// </summary>
    Declaration = 0,

    /// <summary>
    /// For definitions of symbols, for example, in header files.
    /// </summary>
    Definition = 1,

    /// <summary>
    /// For readonly variables and member fields (constants).
    /// </summary>
    Readonly = 2,

    /// <summary>
    /// For class members (static members).
    /// </summary>
    Static = 3,

    /// <summary>
    /// For symbols that should no longer be used.
    /// </summary>
    Deprecated = 4,

    /// <summary>
    /// For types and member functions that are abstract.
    /// </summary>
    Abstract = 5,

    /// <summary>
    /// For functions that are marked async.
    /// </summary>
    Async = 6,

    /// <summary>
    /// For variable references where the variable is assigned to.
    /// </summary>
    Modification = 7,

    /// <summary>
    /// For occurrences of symbols in documentation.
    /// </summary>
    Documentation = 8,

    /// <summary>
    /// For symbols that are part of the standard library.
    /// </summary>
    DefaultLibrary = 9
}
