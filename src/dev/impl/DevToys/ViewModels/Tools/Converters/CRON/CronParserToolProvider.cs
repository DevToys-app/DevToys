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
    [Order(0)]
    internal sealed class CronParserToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.CRONParser.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.Timestamp.SearchDisplayName;

        public string? Description => LanguageManager.Instance.Timestamp.Description;

        public string AccessibleName => LanguageManager.Instance.Timestamp.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.Timestamp.SearchKeywords;

        public TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("\uF823");

        [ImportingConstructor]
        public CronParserToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            if (long.TryParse(data, out long potentialTimestamp))
            {
                try
                {
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(potentialTimestamp);
                    return true;
                }
                catch
                {
                }
            }
            else if (DateTime.TryParse(data, out DateTime potentialDateTime))
            {
                return true;
            }

            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<CronParserToolViewModel>();
        }
    }
}
