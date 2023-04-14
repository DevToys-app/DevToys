#nullable enable

using System;
using System.Composition;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using DevToys.ViewModels.Tools.Timestamp;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.Converters.Timestamp
{
    [Export(typeof(IToolProvider))]
    [Name("Timestamp")]
    [Parent(ConvertersGroupToolProvider.InternalName)]
    [ProtocolName("time")]
    [Order(0)]
    internal sealed class TimestampToolProvider : IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.Timestamp.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.Timestamp.SearchDisplayName;

        public string? Description => LanguageManager.Instance.Timestamp.Description;

        public string AccessibleName => LanguageManager.Instance.Timestamp.AccessibleName;

        public string? SearchKeywords => LanguageManager.Instance.Timestamp.SearchKeywords;

        public string IconGlyph => "\u0119";

        [ImportingConstructor]
        public TimestampToolProvider(IMefProvider mefProvider)
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
            else if (DateTime.TryParse(data, out _))
            {
                return true;
            }

            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<TimestampToolViewModel>();
        }
    }
}
