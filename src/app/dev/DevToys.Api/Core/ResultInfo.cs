using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DevToys.Api;

/// <summary>
/// Record to contain both whether the task was a success and the resulting data
/// </summary>
/// <typeparam name="T">Type of the result</typeparam>
public record ResultInfo<T>
{
    public T? Data { get; }

    public bool HasSucceeded { get; }

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
public record ResultInfo<T, ResultInfoSeverity>
{
    public T? Data { get; }

    public ResultInfoSeverity Severity { get; }

    public string? ErrorMessage { get; }

    /// <summary>
    /// Record to contain both whether the task was a success and the resulting data
    /// </summary>
    /// <param name="data">The resulting data or the task</param>
    /// <param name="severity">The severity of the result</param>
    public ResultInfo(T data, ResultInfoSeverity severity)
    {
        Data = data;
        Severity = severity;
    }

    /// <summary>
    /// Record to contain both whether the task was a success and the resulting data
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="severity">The severity of the result</param>
    public ResultInfo(string errorMessage, ResultInfoSeverity severity)
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
    public ResultInfo(T data, string errorMessage, ResultInfoSeverity severity)
    {
        Data = data;
        Severity = severity;
        ErrorMessage = errorMessage;
    }
}
