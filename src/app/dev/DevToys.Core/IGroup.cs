namespace DevToys.Core;

public interface IGroup
{
    IEnumerable<IItem>? ChildrenItems { get; set; }
}
