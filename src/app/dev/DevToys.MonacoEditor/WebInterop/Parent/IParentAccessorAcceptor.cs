namespace DevToys.MonacoEditor.WebInterop.Parent;

//// TODO: Find better approach than this.
/// <summary>
/// Interface used on objects to be accessed.
/// </summary>
public interface IParentAccessorAcceptor
{
    /// <summary>
    /// Property to tell object the value is being set by ParentAccessor.
    /// </summary>
    bool IsSettingValue { get; set; }
}
