using DevToys.Api;

namespace DevToys.Linux.Core;

[Export(typeof(IFontProvider))]
internal sealed partial class FontProvider : IFontProvider
{
    internal Gtk.Window? MainWindow { private get; set; }

    public string[] GetFontFamilies()
    {
        Guard.IsNotNull(MainWindow);
        using Pango.FontMap? families = MainWindow.GetPangoContext().GetFontMap();
        if (families is not null)
        {
            uint fontCount = families.GetNItems();
            var fonts = new List<string>();
            for (uint i = 0; i < fontCount; i++)
            {
                var fontFamily = (Pango.FontFamily)families.GetObject(i)!;
                fonts.Add(fontFamily.GetName()!);
            }

            return fonts.Order().ToArray();
        }

        return Array.Empty<string>();
    }
}
