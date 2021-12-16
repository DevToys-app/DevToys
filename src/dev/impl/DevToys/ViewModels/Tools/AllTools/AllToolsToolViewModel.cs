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
                    .Where(tool => tool.ChildrenTools.Count == 0 && tool.Metadata.MenuPlacement != MenuPlacement.Header) // Don't show tool groups and tools displayed in the menu header.
                    .OrderBy(tool => tool.Metadata.MenuPlacement)                                                        // Show first items from the Body, then the Footer of the menu.
                    .ThenBy(tool => tool.ToolProvider.MenuDisplayName)                                                   // Order items alphabetically
                    .ThenBy(tool => tool.Metadata.Order ?? int.MaxValue)                                                 // Then by defined order.
                    .ThenBy(item => item.Metadata.Name)                                                                  // Then by internal name
                    .Select(tool => tool.ToolProvider)
                    .ToList();
        }
    }
}
