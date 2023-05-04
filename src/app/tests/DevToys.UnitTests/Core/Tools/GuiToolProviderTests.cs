using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;

namespace DevToys.UnitTests.Core.Tools;

public class GuiToolProviderTests : MefBasedTest
{
    [Fact]
    public void TitleResourceResolution()
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();
        GuiToolInstance toolInstance = guiToolProvider.AllTools.FirstOrDefault(t => t.InternalComponentName == "MockTool");

        Assert.Equal("Sample title", toolInstance.LongDisplayTitle);
        Assert.Equal("Sample title", toolInstance.ShortDisplayTitle);
    }

    [Fact]
    public void ToolOrdering()
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();

        Assert.Equal("MockTool", guiToolProvider.AllTools[0].InternalComponentName);
        Assert.Equal("MockTool2", guiToolProvider.AllTools[1].InternalComponentName);
    }

    [Fact]
    public void ToolsMenu()
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();

        ReadOnlyObservableCollection<INotifyPropertyChanged> menuItems = guiToolProvider.HeaderAndBodyToolViewItems;
        Assert.Equal(3, menuItems.Count);
        Assert.IsType<GroupViewItem>(menuItems[0]);
        Assert.IsType<SeparatorViewItem>(menuItems[1]);
        Assert.IsType<GroupViewItem>(menuItems[2]);

        Assert.Equal("All tools", ((GroupViewItem)menuItems[0]).DisplayTitle);
        Assert.Null(((GroupViewItem)menuItems[0]).Children);
        Assert.Equal("Group title", ((GroupViewItem)menuItems[2]).DisplayTitle);
        Assert.Equal(4, ((GroupViewItem)menuItems[2]).Children.Count);
        Assert.Equal("MockTool", ((GroupViewItem)menuItems[2]).Children[0].ToolInstance.InternalComponentName);
        Assert.Equal("MockTool2", ((GroupViewItem)menuItems[2]).Children[1].ToolInstance.InternalComponentName);
        Assert.Equal("MockTool3-XMLFormatter", ((GroupViewItem)menuItems[2]).Children[2].ToolInstance.InternalComponentName);
        Assert.Equal("MockTool4-XMLValidator", ((GroupViewItem)menuItems[2]).Children[3].ToolInstance.InternalComponentName);
    }

    [Fact]
    public void ToolsMenuRecent()
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();

        ReadOnlyObservableCollection<INotifyPropertyChanged> menuItems = guiToolProvider.HeaderAndBodyToolViewItems;
        Assert.Equal(3, menuItems.Count);
        Assert.IsType<GroupViewItem>(menuItems[0]);
        Assert.IsType<SeparatorViewItem>(menuItems[1]);
        Assert.IsType<GroupViewItem>(menuItems[2]);

        guiToolProvider.SetMostRecentUsedTool(((GroupViewItem)menuItems[2]).Children[0].ToolInstance);

        // Menu should be unchanged.
        Assert.Equal(3, menuItems.Count);
        Assert.IsType<GroupViewItem>(menuItems[0]);
        Assert.IsType<SeparatorViewItem>(menuItems[1]);
        Assert.IsType<GroupViewItem>(menuItems[2]);
    }

    [Fact]
    public void ToolsMenuFavorites()
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();
        ReadOnlyObservableCollection<INotifyPropertyChanged> menuItems = guiToolProvider.HeaderAndBodyToolViewItems;

        GuiToolViewItem toolToSetAsFavorite = ((GroupViewItem)menuItems[2]).Children[0];

        // At first, no favorites at all.
        Assert.Equal(3, menuItems.Count);
        Assert.IsType<GroupViewItem>(menuItems[0]);
        Assert.IsType<SeparatorViewItem>(menuItems[1]);
        Assert.IsType<GroupViewItem>(menuItems[2]);

        // Set item as favorite. The menu should then have "Favorites" group.
        guiToolProvider.SetToolIsFavorite(toolToSetAsFavorite.ToolInstance, isFavorite: true);
        Assert.Equal(4, menuItems.Count);
        Assert.IsType<GroupViewItem>(menuItems[0]);
        Assert.IsType<SeparatorViewItem>(menuItems[1]);
        Assert.IsType<GroupViewItem>(menuItems[2]);
        Assert.IsType<GroupViewItem>(menuItems[3]);
        Assert.Equal("Favorites", ((GroupViewItem)menuItems[2]).DisplayTitle);
        Assert.Equal("Group title", ((GroupViewItem)menuItems[3]).DisplayTitle);
        Assert.Single(((GroupViewItem)menuItems[2]).Children);
        Assert.Equal(toolToSetAsFavorite.ToolInstance, ((GroupViewItem)menuItems[2]).Children[0].ToolInstance);
        Assert.NotEqual(toolToSetAsFavorite, ((GroupViewItem)menuItems[2]).Children[0]);

        // Unfavorite the item. The menu should remove "Favorite" group because the group is now empty.
        guiToolProvider.SetToolIsFavorite(toolToSetAsFavorite.ToolInstance, isFavorite: false);
        Assert.Equal(3, menuItems.Count);
        Assert.IsType<GroupViewItem>(menuItems[0]);
        Assert.IsType<SeparatorViewItem>(menuItems[1]);
        Assert.IsType<GroupViewItem>(menuItems[2]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("BOO", "NoSearchResults")]
    [InlineData("XML", "MockTool3-XMLFormatter", "MockTool4-XMLValidator")]
    [InlineData("XML Validator", "MockTool4-XMLValidator")]
    [InlineData("XML Valid", "MockTool4-XMLValidator")]
    public async Task ToolSearch(string searchQuery, params string[] toolNames)
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();
        var results = new ObservableCollection<GuiToolViewItem>();
        guiToolProvider.SearchTools(searchQuery, results);

        Assert.Equal(toolNames.Length, results.Count);
        for (int i = 0; i < toolNames.Length; i++)
        {
            Assert.Equal(toolNames[i], results[i].ToolInstance.InternalComponentName);
        }
    }
}
