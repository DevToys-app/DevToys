using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Linux.Components;

/// <summary>
/// A collection of <see cref="RootComponent"/> items.
/// </summary>
internal sealed class RootComponentsCollection : ObservableCollection<RootComponent>, IJSComponentConfiguration
{
    /// <inheritdoc />
    public JSComponentConfigurationStore JSComponents { get; } = new();
}
