#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Api.Core;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.CheckSumGenerator
{
    [Export(typeof(IToolProvider))]
    [Name("CheckSum Generator")]
    [Parent(GeneratorsGroupToolProvider.InternalName)]
    [ProtocolName("checksum")]
    [Order(3)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal sealed class CheckSumGeneratorToolProvider : ToolProviderBase, IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.CheckSumGenerator.MenuDisplayName;

        public string? SearchDisplayName => LanguageManager.Instance.CheckSumGenerator.SearchDisplayName;

        public string? Description => LanguageManager.Instance.CheckSumGenerator.Description;

        public string AccessibleName => LanguageManager.Instance.CheckSumGenerator.AccessibleName;

        //TODO: Create svg
        public TaskCompletionNotifier<IconElement> IconSource => CreateFontIcon("checksum.svg");

        [ImportingConstructor]
        public CheckSumGeneratorToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<CheckSumGeneratorToolViewModel>();
        }
    }
}
