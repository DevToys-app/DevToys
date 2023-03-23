namespace DevToys.Core.Tools;

public class SmartDetectedTool
{
    public SmartDetectedTool(GuiToolInstance toolInstance, string dataTypeName, object? parsedData)
    {
        Guard.IsNotNull(toolInstance);
        Guard.IsNotNullOrEmpty(dataTypeName);
        ToolInstance = toolInstance;
        DataTypeName = dataTypeName;
        ParsedData = parsedData;
    }

    public GuiToolInstance ToolInstance { get; }

    public string DataTypeName { get; }

    public object? ParsedData { get; }
}
