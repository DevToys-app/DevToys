#if WINDOWS_UWP
#nullable enable
using Windows.UI.Xaml.Markup;
#else
using Microsoft.UI.Xaml.Markup;
#endif

using Windows.ApplicationModel.Resources;

namespace DevToys.UI.Framework.MarkupExtensions;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public sealed class StringResource : MarkupExtension
{
    private static readonly ResourceLoader resourceLoader = new("DevToys.UI.Framework/Strings");

    public StringResource() { }

    public string Key { get; set; } = "";

    protected override object ProvideValue()
    {
        return resourceLoader.GetString(Key);
    }
}
