using DevToys.Api.Core;

namespace DevToys.MauiBlazor.Components;

public abstract class StyledComponentBase : ComponentBase
{
    private static readonly Random random = new();

    private readonly Lazy<ObservableHashSet<string>> _css;

    /// <summary>
    /// Gets a reference to the HTML element rendered by the component.
    /// </summary>
    public ElementReference Element { get; set; }

    /// <summary>
    /// Gets the value of the <c>id</c> attribute in the DOM of this component.
    /// </summary>
    public string Id { get; } = NewId();

    /// <summary>
    /// Gets or sets additional custom attributes that will be rendered by the component.
    /// </summary>
    /// <value>The attributes.</value>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets a custom CSS class to apply to the component.
    /// </summary>
    [Parameter]
    public string Class { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a custom CSS style to apply to the component.
    /// </summary>
    [Parameter]
    public string Style { get; set; } = string.Empty;

    /// <summary>
    /// Gets a list of CSS class to apply to the component.
    /// </summary>
    protected ObservableHashSet<string> CSS => _css.Value;

    /// <summary>
    /// Gets an aggregation of <see cref="CSS"/>, <see cref="Class"/> and (optionally) <c>class</c> attribute defined in <see cref="AdditionalAttributes"/>.
    /// </summary>
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
    protected static string NewId(int length = 8)
    {
        Guard.IsLessThanOrEqualTo(length, 16);

        if (length <= 8)
        {
            return $"f{random.Next():x}";
        }

        return $"f{random.Next():x}{random.Next():x}"[..length];
    }
}
