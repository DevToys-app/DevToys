using System.Globalization;

namespace DevToys.MauiBlazor.Components;

public partial class Button : MefLayoutComponentBase
{
    [Parameter]
    public ButtonType Type { get; set; } = ButtonType.Button;

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public bool IconOnly { get; set; } = false;

    [Parameter]
    public ButtonAppearance Appearance { get; set; } = ButtonAppearance.Neutral;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void AppendClasses(ClassHelper helper)
    {
        helper.Append("button");
        helper.Append(Appearance.Code);

        if (IconOnly)
        {
            helper.Append("icon");
        }

        base.AppendClasses(helper);
    }

    protected override void OnParametersSet()
    {
        if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out object? obj))
        {
            string classes = Convert.ToString(obj, CultureInfo.InvariantCulture) ?? string.Empty;
            Classess = new HashSet<string>(classes.Split(' '));
        }

        base.OnParametersSet();
    }
}
