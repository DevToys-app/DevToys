namespace DevToys.MauiBlazor.Components;

public partial class NavbarItems : MefLayoutComponentBase
{
    [Parameter]
    public bool IsScrollable { get; set; }

    [Parameter]
    public bool IsScrollableVertical { get; set; }

    [Parameter]
    public bool IsFixedBottom { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void AppendClasses(ClassHelper helper)
    {
        helper.Append("nav-items");
        if (IsScrollable)
        {
            helper.Append("scrollable");
        }
        if (IsScrollableVertical)
        {
            helper.Append("vertical");
        }

        if (IsFixedBottom)
        {
            helper.Append("sticky-bottom");
        }

        base.AppendClasses(helper);
    }
}
