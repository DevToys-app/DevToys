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
    /// Message to display
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Severity of the result
    /// </summary>
    public ResultInfoSeverity Severity { get; }

    /// <summary>
    /// Record to contain both whether the task was a success and the resulting data
    /// </summary>
    /// <param name="data">The resulting data or the task</param>
    /// <param name="hasSucceeded">Whether the task succeeded</param>
    public ResultInfo(T data, bool hasSucceeded = true)
    {
        Data = data;
        HasSucceeded = hasSucceeded;
        Severity = hasSucceeded ? ResultInfoSeverity.Success : ResultInfoSeverity.Error;
    }

    /// <summary>
    /// Record to contain both whether the task was a success and the resulting data
    /// </summary>
    /// <param name="data">The resulting data or the task</param>
    /// <param name="severity">The severity of the result</param>
    /// <param name="message">The message</param>
    public ResultInfo(T? data, ResultInfoSeverity severity, string message)
    {
        Data = data;
        Severity = severity;
        HasSucceeded = severity is not ResultInfoSeverity.Error;
        Message = message;
    }

    /// <summary>
    /// Implicit conversion operator from type <typeparamref name="T"/> to <see cref="ResultInfo{T}"/>.
    /// This allows assigning a value of type <typeparamref name="T"/> directly to <see cref="ResultInfo{T}"/> without explicitly calling the constructor.
    /// </summary>
    /// <param name="data">The resulting data or the task</param>
    /// <returns>A new instance of <see cref="ResultInfo{T}"/> with the provided data</returns>
    public static implicit operator ResultInfo<T>(T data) => new(data, true);

    /// <summary>
    /// Creates a new instance of <see cref="ResultInfo{T}"/> with the provided data and sets the result as successful.
    /// </summary>
    /// <param name="data">The resulting data or the task</param>
    public static ResultInfo<T> Success(T data) => new(data, true);

    /// <summary>
    /// Creates a new instance of <see cref="ResultInfo{T}"/> with the provided message and sets the result as an error.
    /// </summary>
    /// <param name="message">The error message</param>
    public static ResultInfo<T> Error(string message) => new(default, ResultInfoSeverity.Error, message);

    /// <summary>
    /// Creates a new instance of <see cref="ResultInfo{T}"/> with the provided data, message, and severity set to warning.
    /// </summary>
    /// <param name="data">The resulting data or the task</param>
    /// <param name="message">The warning message</param>
    public static ResultInfo<T> Warning(T? data, string message) => new(data, ResultInfoSeverity.Warning, message);
}
