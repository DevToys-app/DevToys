using System.Globalization;
using DevToys.Api;
using Microsoft.Fast.Components.FluentUI;

namespace DevToys.MauiBlazor.Components;

public abstract class MefLayoutComponentBase : LayoutComponentBase
{
    protected HashSet<string>? Classes = null;

    [Inject]
    protected IMefProvider MefProvider { get; set; } = default!;

    protected ClassHelper ClassHelper { get; private set; }

    public ElementReference Element { get; set; }

    [Parameter]
    public string Id { get; set; } = Identifier.NewId();

    [Parameter]
    public string Class { get; set; } = string.Empty;

    public string CssClasses => ClassHelper.Classes;

    [Parameter(CaptureUnmatchedValues = true)]
    public virtual IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected MefLayoutComponentBase()
    {
        ClassHelper = new ClassHelper(AppendClasses);
    }

    protected override void OnInitialized()
    {
        MefProvider.SatisfyImports(this);
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(Class))
        {
            Classes = new HashSet<string>(Class.Split(' '), StringComparer.InvariantCultureIgnoreCase);
        }

        base.OnParametersSet();
    }

    protected virtual void ClassesHasChanged()
        => ClassHelper.HasChanged();

    protected virtual void AppendClasses(ClassHelper helper)
    {
        if (Classes is not null)
        {
            helper.Append(Classes);
        }
    }
}
