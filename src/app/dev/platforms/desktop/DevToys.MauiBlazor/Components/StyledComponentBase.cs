using DevToys.Api.Core;

namespace DevToys.MauiBlazor.Components;

public abstract class StyledComponentBase : ComponentBase
{
    private static readonly Random random = new();

    private readonly Lazy<ObservableHashSet<string>> _css;

    public ElementReference Element { get; set; }

    public string Id { get; } = NewId();

    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string Style { get; set; } = string.Empty;

    protected ObservableHashSet<string> CSS => _css.Value;

    public string FinalCssClasses { get; private set; } = string.Empty;

    protected StyledComponentBase()
    {
        _css = new(() =>
        {
            var collection = new ObservableHashSet<string>();
            collection.CollectionChanged += CSS_CollectionChanged;
            return collection;
        });
    }

    private void CSS_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        BuildClassAttribute();
    }

    private void BuildClassAttribute()
    {
        var cssBuilder = new CssBuilder();
        if (_css.IsValueCreated)
        {
            foreach (string cssClass in _css.Value)
            {
                cssBuilder.AddClass(cssClass);
            }
        }

        if (AdditionalAttributes is not null)
        {
            cssBuilder.AddClassFromAttributes(AdditionalAttributes);
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            cssBuilder.AddClass(Class);
        }

        FinalCssClasses = cssBuilder.ToString();
    }

    protected override void OnParametersSet()
    {
        BuildClassAttribute();

        base.OnParametersSet();
    }

    /// <summary>
    /// Returns a new small Id.
    /// HTML id must start with a letter.
    /// Example: f127d9edf14385adb
    /// </summary>
    /// <returns></returns>
    private static string NewId(int length = 8)
    {
        Guard.IsLessThanOrEqualTo(length, 16);

        if (length <= 8)
        {
            return $"f{random.Next():x}";
        }

        return $"f{random.Next():x}{random.Next():x}"[..length];
    }
}
