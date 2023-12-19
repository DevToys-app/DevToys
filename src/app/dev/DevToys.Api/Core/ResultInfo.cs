namespace DevToys.Api.Core;

/// <summary>
/// Record to contain both whether the task was a success and the resulting data
/// </summary>
/// <typeparam name="T">Type of the result</typeparam>
/// <param name="Data">The resulting data or the task</param>
/// <param name="HasSucceeded">Whether the task succeeded</param>
public record ResultInfo<T>(T Data, bool HasSucceeded = true);
