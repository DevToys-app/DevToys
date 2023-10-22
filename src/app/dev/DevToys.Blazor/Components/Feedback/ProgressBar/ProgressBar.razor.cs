using System.ComponentModel.DataAnnotations;

namespace DevToys.Blazor.Components;

public partial class ProgressBar : StyledComponentBase
{
    public ProgressBar()
    {
        VerticalAlignment = UIVerticalAlignment.Top;
    }

    [Parameter]
    [Range(0d, 100d, ErrorMessage = $"{nameof(Value)} must be between 0 and 100.")]
    public double Value { get; set; }

    [Parameter]
    public bool IsIndeterminate { get; set; }

    protected override void OnParametersSet()
    {
        if (IsIndeterminate && AdditionalAttributes is not null)
        {
            AdditionalAttributes.Remove("aria-valuemin");
            AdditionalAttributes.Remove("aria-valuemax");
            AdditionalAttributes.Remove("aria-valuenow");
        }

        if (!IsIndeterminate)
        {
            AdditionalAttributes ??= new Dictionary<string, object>();

            AdditionalAttributes.TryAdd("aria-valuemin", 0d);
            AdditionalAttributes.TryAdd("aria-valuemax", 100d);
            AdditionalAttributes.TryAdd("aria-valuenow", Value);
        }

        base.OnParametersSet();
    }
}
