using System.Resources;
using DevToys.Localization.Strings.ToolPage;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

namespace DevToys.UI.Framework.Extensions;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public partial class StringsExtension : MarkupExtension
{
    public enum KeyEnum
    {
        __Undefined = 0,
        Clear,
        Copy,
        OpenFile,
        Paste,
        SaveAs,
        SendToSmartDetection,
        ToggleSwitchOff,
        ToggleSwitchOn,
    }

    private static readonly ResourceManager resourceManager = new ResourceManager("DevToys.Localization.Strings.ToolPage.ToolPage", typeof(ToolPage).Assembly);

    public KeyEnum Key { get; set; }

    public IValueConverter? Converter { get; set; }

    public object? ConverterParameter { get; set; }

    protected override object ProvideValue()
    {
        string res;
        if (Key == KeyEnum.__Undefined)
            res = "";
        else
        {
            res = resourceManager.GetString(Key.ToString(), culture: null) ?? string.Empty;
        }
        return Converter == null ? res : Converter.Convert(res, typeof(string), ConverterParameter, null);
    }
}
