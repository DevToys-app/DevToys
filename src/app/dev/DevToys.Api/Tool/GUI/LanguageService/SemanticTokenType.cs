namespace DevToys.Api;

// See https://code.visualstudio.com/api/language-extensions/semantic-highlight-guide#standard-token-types-and-modifiers
/// <summary>
/// Represents a semantic token type.
/// </summary>
public enum SemanticTokenType
{
    /// <summary>
    /// For identifiers that declare or reference a namespace, module, or package.
    /// </summary>
    Namespace = 0,

    /// <summary>
    /// For identifiers that declare or reference a class type.
    /// </summary>
    Class = 1,

    /// <summary>
    /// For identifiers that declare or reference an enumeration type.
    /// </summary>
    Enum = 2,

    /// <summary>
    /// For identifiers that declare or reference an interface type.
    /// </summary>
    Interface = 3,

    /// <summary>
    /// For identifiers that declare or reference a struct type.
    /// </summary>
    Struct = 4,

    /// <summary>
    /// For identifiers that declare or reference a type parameter.
    /// </summary>
    TypeParameter = 5,

    /// <summary>
    /// For identifiers that declare or reference a type that is not covered above.
    /// </summary>
    Type = 6,

    /// <summary>
    /// For identifiers that declare or reference a function or method parameters.
    /// </summary>
    Parameter = 7,

    /// <summary>
    /// For identifiers that declare or reference a local or global variable.
    /// </summary>
    Variable = 8,

    /// <summary>
    /// For identifiers that declare or reference a member property, member field, or member variable.
    /// </summary>
    Property = 9,

    /// <summary>
    /// For identifiers that declare or reference an enumeration property, constant, or member.
    /// </summary>
    EnumMember = 10,

    /// <summary>
    /// For identifiers that declare or reference decorators and annotations.
    /// </summary>
    Decorator = 11,

    /// <summary>
    /// For identifiers that declare an event property.
    /// </summary>
    Event = 12,

    /// <summary>
    /// For identifiers that declare a function.
    /// </summary>
    Function = 13,

    /// <summary>
    /// For identifiers that declare a member function or method.
    /// </summary>
    Method = 14,

    /// <summary>
    /// For identifiers that declare a macro.
    /// </summary>
    Macro = 15,

    /// <summary>
    /// For identifiers that declare a label.
    /// </summary>
    Label = 16,

    /// <summary>
    /// For tokens that represent a comment.
    /// </summary>
    Comment = 17,

    /// <summary>
    /// For tokens that represent a string literal.
    /// </summary>
    String = 18,

    /// <summary>
    /// For tokens that represent a language keyword.
    /// </summary>
    Keyword = 19,

    /// <summary>
    /// For tokens that represent a number literal.
    /// </summary>
    Number = 20,

    /// <summary>
    /// For tokens that represent a regular expression literal.
    /// </summary>
    RegularExpression = 21,

    /// <summary>
    /// For tokens that represent an operator.
    /// </summary>
    Operator = 22
}
