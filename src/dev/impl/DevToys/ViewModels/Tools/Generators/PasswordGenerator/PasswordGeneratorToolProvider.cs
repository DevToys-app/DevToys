#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Shared.Api.Core;

namespace DevToys.ViewModels.Tools.Generators.PasswordGenerator
{
    [Export(typeof(IToolProvider))]
    [Name("Password Generator")]
    [Parent(GeneratorsGroupToolProvider.InternalName)]
    [ProtocolName("password")]
    [Order(1)]
    [CompactOverlaySize(width: 400, height: 500)]
    internal class PasswordGeneratorToolProvider : IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.PasswordGenerator.MenuDisplayName;

        public string SearchDisplayName => LanguageManager.Instance.PasswordGenerator.SearchDisplayName;

        public string Description => LanguageManager.Instance.PasswordGenerator.Description;

        public string AccessibleName => LanguageManager.Instance.PasswordGenerator.AccessibleName;

        public string SearchKeywords => LanguageManager.Instance.PasswordGenerator.SearchKeywords;

        public string IconGlyph => "\u0136";

        [ImportingConstructor]
        public PasswordGeneratorToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<PasswordGeneratorToolViewModel>();
        }
    }
}
