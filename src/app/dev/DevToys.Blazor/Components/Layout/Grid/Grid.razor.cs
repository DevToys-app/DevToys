using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DevToys.Blazor.Components;

public partial class Grid : StyledComponentBase
{
    protected string? StyleValue => new StyleBuilder()
        .AddStyle("display", "grid")
        .AddStyle("grid-template-columns", GetGridTemplate(Columns))
        .AddStyle("grid-template-rows", GetGridTemplate(Rows))
        .AddStyle("gap", $"{RowSpacing.ToPx()} {ColumnSpacing.ToPx()}")
        .AddImportantStyle("height", "inherit", VerticalAlignment == UIVerticalAlignment.Stretch)
        .AddImportantStyle("height", "fit-content", VerticalAlignment != UIVerticalAlignment.Stretch)
        .AddImportantStyle("width", "inherit", HorizontalAlignment == UIHorizontalAlignment.Stretch)
        .AddStyle(Style)
        .Build();

    /// <summary>
    /// Gets or sets the list of columns in the grid.
    /// </summary>
    [Parameter]
    public IEnumerable<UIGridLength>? Columns { get; set; }

    /// <summary>
    /// Gets or sets the list of rows in the grid.
    /// </summary>
    [Parameter]
    public IEnumerable<UIGridLength>? Rows { get; set; }

    /// <summary>
    /// Gets or sets the uniform distance (in pixels) between grid columns.
    /// </summary>
    [Parameter]
    [Range(0, int.MaxValue, ErrorMessage = $"{nameof(ColumnSpacing)} must be positive and zero-based.")]
    public int ColumnSpacing { get; set; }

    /// <summary>
    /// Gets or sets the uniform distance (in pixels) between grid rows.
    /// </summary>
    [Parameter]
    [Range(0, int.MaxValue, ErrorMessage = $"{nameof(RowSpacing)} must be positive and zero-based.")]
    public int RowSpacing { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private static string GetGridTemplate(IEnumerable<UIGridLength>? collection)
    {
        if (collection is null || !collection.Any())
        {
            return "1fr";
        }

        var builder = new StringBuilder();
        foreach (UIGridLength item in collection)
        {
            if (item.IsFraction)
            {
                builder.Append($" {item.Value}fr");
            }
            else if (item.IsAbsolute)
            {
                builder.Append($" {item.Value.ToPx()}");
            }
            else if (item.IsAuto)
            {
                builder.Append($" minmax(min-content, auto)");
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        return builder.ToString();
    }
}
