﻿#nullable enable

using System.Composition;
using DevToys.Api.Tools;
using DevToys.Shared.Api.Core;

namespace DevToys.ViewModels.AllTools
{
    [Export(typeof(IToolProvider))]
    [Name("All Tools")]
    [ProtocolName("all")]
    [Order(0)]
    [MenuPlacement(MenuPlacement.Header)]
    [NotSearchable]
    [NotFavorable]
    [NoCompactOverlaySupport]
    internal sealed class AllToolsToolProvider : IToolProvider
    {
        private readonly IMefProvider _mefProvider;

        public string MenuDisplayName => LanguageManager.Instance.AllTools.MenuDisplayName;

        public string? SearchDisplayName => MenuDisplayName;

        public string? Description { get; } = null;

        public string AccessibleName => LanguageManager.Instance.AllTools.AccessibleName;

        public string? SearchKeywords => MenuDisplayName;

        public string IconGlyph => "\u0117";

        [ImportingConstructor]
        public AllToolsToolProvider(IMefProvider mefProvider)
        {
            _mefProvider = mefProvider;
        }

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            return _mefProvider.Import<AllToolsToolViewModel>();
        }
    }
}
