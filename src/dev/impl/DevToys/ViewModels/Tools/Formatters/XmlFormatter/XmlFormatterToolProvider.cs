#nullable enable

using System.Composition;
using DevToys.Shared.Api.Core;
using DevToys.Api.Tools;
using DevToys.Helpers;
using DevToys.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.Formatters.XmlFormatter
{
    [Export(typeof(IToolProvider))]
    [Name("Xml Formatter")]
    [Parent(FormattersGroupToolProvider.InternalName)]
    [ProtocolName("xmlformat")]
    [Order(0)]
    [NotScrollable]
    internal sealed class XmlFormatterToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => "Xml"; // LanguageManager.Instance.XmlFormatter.MenuDisplayName;

        public string? SearchDisplayName => "XML Formatter"; //LanguageManager.Instance.XmlFormatter.SearchDisplayName;

        public string? Description => "Indent or minify XML data"; //LanguageManager.Instance.XmlFormatter.Description;

        public string AccessibleName => "XML Formatter tool"; //LanguageManager.Instance.XmlFormatter.AccessibleName;

        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("JsonFormatter.svg");

        [ImportingConstructor]
        public XmlFormatterToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return XmlHelper.IsValid(data);
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<XmlFormatterToolViewModel>();
        }
    }
}
