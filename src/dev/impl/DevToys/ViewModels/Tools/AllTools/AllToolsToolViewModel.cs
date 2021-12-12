#nullable enable

using System;
using System.Composition;
using System.Linq;
using DevToys.Api.Tools;
using DevToys.ViewModels.Tools;
using DevToys.Views.Tools.AllTools;

namespace DevToys.ViewModels.AllTools
{
    [Export(typeof(AllToolsToolViewModel))]
    public sealed class AllToolsToolViewModel : GroupToolViewModelBase, IToolViewModel
    {
        public Type View { get; } = typeof(AllToolsToolPage);

        [ImportingConstructor]
        public AllToolsToolViewModel(IToolProviderFactory toolProviderFactory)
        {
            ToolProviders
                = toolProviderFactory
                    .GetAllTools()
                    .Where(tool => tool.ChildrenTools.Count == 0 && tool.Metadata.MenuPlacement != MenuPlacement.Header)
                    .OrderBy(tool => tool.Metadata.MenuPlacement)
                    .ThenBy(tool => tool.ToolProvider.MenuDisplayName)
                    .ThenBy(tool => tool.Metadata.Order ?? int.MaxValue)
                    .ThenBy(item => item.Metadata.Name)
                    .Select(tool => tool.ToolProvider)
                    .ToList();
        }
    }
}
