using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DevToys.Api;

/// <summary>
/// Record to contain both whether the task was a success and the resulting data
/// </summary>
/// <typeparam name="T">Type of the result</typeparam>
public record ResultInfo<T>
{
    /// <summary>
    /// The resulting data or the task
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Whether the task succeeded
    /// </summary>
    public bool HasSucceeded { get; }

    /// <summary>
    /// Error message to display
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Record to contain both whether the task was a success and the resulting data
    /// </summary>
    /// <param name="data">The resulting data or the task</param>
    /// <param name="hasSucceeded">Whether the task succeeded</param>
    public ResultInfo(T data, bool hasSucceeded = true)
    {
        Data = data;
        HasSucceeded = hasSucceeded;
    }

    /// <summary>
    /// Record to contain both whether the task was a success and the resulting data
    /// </summary>
    /// <param name="data">The resulting data or the task</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="hasSucceeded">Whether the task succeeded</param>
    public ResultInfo(T data, string errorMessage, bool hasSucceeded = false)
    {
        Data = data;
        HasSucceeded = hasSucceeded;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Record to contain both whether the task was a success and the resulting data
/// </summary>
/// <typeparam name="T">Type of the result</typeparam>
/// <typeparam name="U">The severity of the result</typeparam>
public record ResultInfo<T, U> where U : Enum
{
    /// <summary>
    /// The resulting data or the task
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Severity of the result
    /// </summary>
    public U Severity { get; }

    /// <summary>
    /// Error message to display
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Record to contain both whether the task was a success and the resulting data
    /// </summary>
    /// <param name="data">The resulting data or the task</param>
    /// <param name="severity">The severity of the result</param>
    public ResultInfo(T data, U severity)
    {
        Data = data;
        Severity = severity;
    }

    /// <summary>
    /// Record to contain both whether the task was a success and the resulting data
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="severity">The severity of the result</param>
    public ResultInfo(string errorMessage, U severity)
    {
        Severity = severity;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Record to contain both whether the task was a success and the resulting data
    /// </summary>
    /// <param name="data">The resulting data or the task</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="severity">The severity of the result</param>
    public ResultInfo(T data, string errorMessage, U severity)
    {
        Data = data;
        Severity = severity;
        ErrorMessage = errorMessage;
    }
}
