#nullable enable

using System;
using System.Collections.Generic;
using DevToys.Api.Tools;
using DevToys.Views.Tools;

namespace DevToys.ViewModels.Tools
{
    public sealed class GroupToolViewModel : GroupToolViewModelBase, IToolViewModel
    {
        public Type View { get; } = typeof(GroupToolPage);

        public GroupToolViewModel(IEnumerable<ToolProviderViewItem>? toolProviders)
            : base()
        {
            ToolProviders = toolProviders;
        }
    }
}
