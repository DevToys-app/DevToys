namespace DevToys.Tools.Models;

internal record ToolResult<T>(T Data, bool HasSucceeded = true);
