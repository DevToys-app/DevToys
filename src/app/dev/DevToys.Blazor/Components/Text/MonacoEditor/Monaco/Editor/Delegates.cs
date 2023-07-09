///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public delegate void CommandHandler(params object[] args);

/// <summary>
/// A callback that can compute the cursor state after applying a series of edit operations.
/// </summary>
public delegate List<Selection> CursorStateComputer(List<ValidEditOperation> inverseEditOperations);
