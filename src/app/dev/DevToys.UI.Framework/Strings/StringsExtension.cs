using Microsoft.UI.Xaml.Markup;

namespace DevToys.UI.Framework.Strings;

// Workaround for an issue where Uno Generator can't find this class during build, because Roslyn would run Uno Generator after the ReswPlus generator.
[MarkupExtensionReturnType(ReturnType = typeof(string))]
public partial class ResourcesExtension : MarkupExtension { }
