namespace DevToys.Api;

/// <summary>
/// Record to contain both whether the task was a success and the resulting data
/// </summary>
/// <typeparam name="T">Type of the result</typeparam>
/// <param name="Data">The resulting data or the task</param>
/// <param name="HasSucceeded">Whether the task succeeded</param>
public record ResultInfo<T>(T Data, bool HasSucceeded = true);

/// <summary>
/// Record to contain both whether the task was a success and the resulting data
/// </summary>
/// <typeparam name="T">Type of the result</typeparam>
/// <typeparam name="U">Type of the severity</typeparam>
/// <param name="Data">The resulting data or the task</param>
/// <param name="Severity">The severity of the result</param>
public record ResultInfo<T, U>(T Data, U Severity);
