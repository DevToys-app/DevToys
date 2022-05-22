#nullable enable

using System;
using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using DevToys.ViewModels.Tools.CronParser;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.Converters.CronParser
{
    [Export(typeof(IToolProvider))]
    [Name("CronParser")]
    [Parent(ConvertersGroupToolProvider.InternalName)]
    [ProtocolName("cronparser")]
    [Order(4)]
    internal sealed class CronParserToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.CRONParser.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.CRONParser.SearchDisplayName;

        public string? Description => LanguageManager.Instance.CRONParser.Description;

        public string AccessibleName => LanguageManager.Instance.CRONParser.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.CRONParser.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uF823");

        [ImportingConstructor]
        public CronParserToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {            
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<CronParserToolViewModel>();
        }
    }
}
