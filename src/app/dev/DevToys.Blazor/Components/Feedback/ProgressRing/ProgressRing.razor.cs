using System.ComponentModel.DataAnnotations;

namespace DevToys.Blazor.Components;

public partial class ProgressRing : StyledComponentBase
{
    private const int CircleElementR = 7;
    private readonly double _circumference = Math.PI * (CircleElementR * 2);

    public ProgressRing()
    {
        VerticalAlignment = UIVerticalAlignment.Center;
        HorizontalAlignment = UIHorizontalAlignment.Center;
        Height = 48;
        Width = 48;
    }

    [Parameter]
    [Range(0.0, 100.0, ErrorMessage = $"{nameof(Value)} must be between 0 and 100.")]
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

            AdditionalAttributes.TryAdd("aria-valuemin", 0);
            AdditionalAttributes.TryAdd("aria-valuemax", 100);
            AdditionalAttributes.TryAdd("aria-valuenow", Value);
        }

        base.OnParametersSet();
    }
}
