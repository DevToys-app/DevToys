namespace DevToys.Api;

public interface IDataGridRow<T> where T : class
{
    /// <summary>
    /// Gets the element to display as detail information in the row.
    /// </summary>
    T? Details { get; }
}
