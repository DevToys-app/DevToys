namespace DevToys.Core;

public interface IGroup
{
    /// <summary>
    /// Gets all the children items of this group.
    /// </summary>
    IEnumerable<IItem>? ChildrenItems { get; set; }

    /// <summary>
    /// Gets whether the group should be expanded by default.
    /// </summary>
    bool GroupShouldBeExpandedByDefaultInUI { get; }

    /// <summary>
    /// Gets whether the group should be expanded.
    /// </summary>
    public bool GroupShouldBeExpandedInUI { get; }

    /// <summary>
    /// Gets or sets whether the group is expanded in the UI.
    /// </summary>
    public bool GroupIsExpandedInUI { get; set; }
}
