namespace DevToys.Api;

/// <summary>
/// Represents the kind of an auto-completion item.
/// See also https://microsoft.github.io/monaco-editor/typedoc/enums/languages.CompletionItemKind.html
/// </summary>
public enum AutoCompletionItemKind
{
    /// <summary>
    /// Represents a method.
    /// </summary>
    Method = 0,

    /// <summary>
    /// Represents a function.
    /// </summary>
    Function = 1,

    /// <summary>
    /// Represents a constructor.
    /// </summary>
    Constructor = 2,

    /// <summary>
    /// Represents a field.
    /// </summary>
    Field = 3,

    /// <summary>
    /// Represents a variable.
    /// </summary>
    Variable = 4,

    /// <summary>
    /// Represents a class.
    /// </summary>
    Class = 5,

    /// <summary>
    /// Represents a struct.
    /// </summary>
    Struct = 6,

    /// <summary>
    /// Represents an interface.
    /// </summary>
    Interface = 7,

    /// <summary>
    /// Represents a module.
    /// </summary>
    Module = 8,

    /// <summary>
    /// Represents a property.
    /// </summary>
    Property = 9,

    /// <summary>
    /// Represents an event.
    /// </summary>
    Event = 10,

    /// <summary>
    /// Represents an operator.
    /// </summary>
    Operator = 11,

    /// <summary>
    /// Represents a unit.
    /// </summary>
    Unit = 12,

    /// <summary>
    /// Represents a value.
    /// </summary>
    Value = 13,

    /// <summary>
    /// Represents a constant.
    /// </summary>
    Constant = 14,

    /// <summary>
    /// Represents an enum.
    /// </summary>
    Enum = 15,

    /// <summary>
    /// Represents an enum member.
    /// </summary>
    EnumMember = 16,

    /// <summary>
    /// Represents a keyword.
    /// </summary>
    Keyword = 17,

    /// <summary>
    /// Represents text.
    /// </summary>
    Text = 18,

    /// <summary>
    /// Represents a color.
    /// </summary>
    Color = 19,

    /// <summary>
    /// Represents a file.
    /// </summary>
    File = 20,

    /// <summary>
    /// Represents a reference.
    /// </summary>
    Reference = 21,

    /// <summary>
    /// Represents a custom color.
    /// </summary>
    CustomColor = 22,

    /// <summary>
    /// Represents a folder.
    /// </summary>
    Folder = 23,

    /// <summary>
    /// Represents a type parameter.
    /// </summary>
    TypeParameter = 24,

    /// <summary>
    /// Represents a user.
    /// </summary>
    User = 25,

    /// <summary>
    /// Represents an issue.
    /// </summary>
    Issue = 26,

    /// <summary>
    /// Represents a snippet.
    /// </summary>
    Snippet = 27
}
