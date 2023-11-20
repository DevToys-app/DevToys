namespace DevToys.Blazor.Components.UIElements;

public partial class UILabelPresenter : ComponentBase, IDisposable
{
    [Parameter]
    public IUILabel UILabel { get; set; } = default!;

    [Parameter]
    public TextBlockAppearance Appearance { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ApplyStyle();
        UILabel.StyleChanged += UILabel_StyleChanged;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        ApplyStyle();
    }

    public void Dispose()
    {
        UILabel.StyleChanged -= UILabel_StyleChanged;
    }

    private void UILabel_StyleChanged(object? sender, EventArgs e)
    {
        ApplyStyle();
    }

    private void ApplyStyle()
    {
        switch (UILabel.Style)
        {
            case UILabelStyle.Caption:
                Appearance = TextBlockAppearance.Caption;
                break;

            case UILabelStyle.Body:
                Appearance = TextBlockAppearance.Body;
                break;

            case UILabelStyle.BodyStrong:
                Appearance = TextBlockAppearance.BodyStrong;
                break;

            case UILabelStyle.BodyLarge:
                Appearance = TextBlockAppearance.BodyLarge;
                break;

            case UILabelStyle.Subtitle:
                Appearance = TextBlockAppearance.Subtitle;
                break;

            case UILabelStyle.Title:
                Appearance = TextBlockAppearance.Title;
                break;

            case UILabelStyle.TitleLarge:
                Appearance = TextBlockAppearance.TitleLarge;
                break;

            case UILabelStyle.Display:
                Appearance = TextBlockAppearance.Display;
                break;

            default:
                ThrowHelper.ThrowNotSupportedException($"{nameof(UILabelStyle)}.{UILabel.Style} is not supported by {nameof(UILabelPresenter)}.");
                break;
        }
    }
}
