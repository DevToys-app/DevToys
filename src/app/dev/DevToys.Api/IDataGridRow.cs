namespace DevToys.Api;

/// <summary>
/// Represents a data grid row with optional detail information.
/// </summary>
/// <typeparam name="T">The type of the detail information.</typeparam>
public interface IDataGridRow<T> where T : class
{
    /// <summary>
    /// Gets the element to display as detail information in the row.
    /// </summary>
    T? Details { get; }
}
