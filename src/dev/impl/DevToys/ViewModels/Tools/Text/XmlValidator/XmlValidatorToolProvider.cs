# nullable enable

using System.Composition;
using Windows.UI.Xaml.Controls;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;

namespace DevToys.ViewModels.Tools.XmlValidator
{
    [Export(typeof(IToolProvider))]
    [Name("Xml Validator")]
    [Parent(TextGroupToolProvider.InternalName)]
    [ProtocolName("xmlvalidator")]
    [Order(1)]
    [NotScrollable]
    internal sealed class XmlValidatorToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.XmlValidator.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.XmlValidator.SearchDisplayName;

        public string? Description => LanguageManager.Instance.XmlValidator.Description;

        public string AccessibleName => LanguageManager.Instance.XmlValidator.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.XmlValidator.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateSvgIcon("XmlValidator.svg");

        [ImportingConstructor]
        public XmlValidatorToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<XmlValidatorToolViewModel>();
        }
    }
}
