namespace DevToys.MauiBlazor.Components;

public class GridWrapper
{
    private readonly GridTemplateConverter _columns = new();
    private readonly GridTemplateConverter _rows = new();

    private double? _width;
    private double? _height;

    public string Css
        => $"display: grid; {Width()}{Height()}{GenerateTemplateColumnsIfAny()}{GenerateTemplateRowsIfAny()}".TrimEnd();

    private string GenerateTemplateColumnsIfAny()
        => _columns.Any()
        ? $"grid-template-columns: {string.Join(" ", _columns)}; "
        : string.Empty;

    private string GenerateTemplateRowsIfAny()
        => _rows.Any()
        ? $"grid-template-rows: {string.Join(" ", _rows)};"
        : string.Empty;

    private string Width()
        => $"width: {(_width.HasValue ? _width.Value.ToString() + "px;" : "100%;")} ";
    private string Height()
        => $"height: {(_height.HasValue ? _height.Value.ToString() + "px;" : "100%;")} ";

    public void AddColumn(string width, string? min = null, string? max = null)
        => _columns.AddData(width, min, max);

    public void AddRow(string height, string? min = null, string? max = null)
        => _rows.AddData(height, min, max);

    public void SetWidth(double? value) => _width = value;
    public void SetHeight(double? value) => _height = value;

    public string RowGap(double gap)
        => gap > 0 ? $"grid-row-gap: {gap}px;" : string.Empty;

    public string ColumnGap(double gap)
        => gap > 0 ? $"grid-column-gap: {gap}px;" : string.Empty;
}
