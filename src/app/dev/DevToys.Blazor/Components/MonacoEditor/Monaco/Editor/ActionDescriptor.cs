using System.Text.Json.Serialization;

///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Description of an action contribution
/// </summary>
public class ActionDescriptor
{
    /// <summary>
    /// An unique identifier of the contributed action.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// A label of the action that will be presented to the user.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Precondition rule.
    /// </summary>
    public string? Precondition { get; set; }

    /// <summary>
    /// An array of keybindings for the action.
    /// </summary>
    public int[]? Keybindings { get; set; }

    /// <summary>
    /// The keybinding rule (condition on top of precondition).
    /// </summary>
    public string? KeybindingContext { get; set; }

    /// <summary>
    /// Control if the action should show up in the context menu and where.
    /// The context menu of the editor has these default:
    ///   navigation - The navigation group comes first in all cases.
    ///   1_modification - This group comes next and contains commands that modify your code.
    ///   9_cutcopypaste - The last default group with the basic editing commands.
    /// You can also create your own group.
    /// Defaults to null (don't show in context menu).
    /// </summary>
    public string? ContextMenuGroupId { get; set; }

    /// <summary>
    /// Control the order in the context menu group.
    /// </summary>
    public float ContextMenuOrder { get; set; }

    /// <summary>
    /// Method that will be executed when the action is triggered.
    /// @param editor The editor instance is passed in as a convenience
    /// </summary>
    [JsonIgnore]
    public Action<RicherMonacoEditorBase>? Run { get; set; }
}
