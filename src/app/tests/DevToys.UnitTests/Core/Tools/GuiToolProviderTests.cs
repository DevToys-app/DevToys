using System.Collections.ObjectModel;
using System.ComponentModel;
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

        toolInstance.LongDisplayTitle.Should().Be("Sample title");
        toolInstance.ShortDisplayTitle.Should().Be("Sample title");
    }

    [Fact]
    public void ToolOrdering()
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();

        guiToolProvider.AllTools[0].InternalComponentName.Should().Be("MockTool");
        guiToolProvider.AllTools[1].InternalComponentName.Should().Be("MockTool2");
    }

    [Fact]
    public void ToolsMenu()
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();

        ReadOnlyObservableCollection<INotifyPropertyChanged> menuItems = guiToolProvider.HeaderAndBodyToolViewItems;

        menuItems.Should().HaveCount(3);
        menuItems[0].Should().BeOfType<GroupViewItem>();
        menuItems[1].Should().BeOfType<SeparatorViewItem>();
        menuItems[2].Should().BeOfType<GroupViewItem>();

        ((GroupViewItem)menuItems[0]).DisplayTitle.Should().Be("All tools");
        ((GroupViewItem)menuItems[0]).Children.Should().BeNull();

        var item3 = (GroupViewItem)menuItems[2];
        item3.DisplayTitle.Should().Be("Group title");
        item3.Children.Should().HaveCount(4);
        item3.Children[0].ToolInstance.InternalComponentName.Should().Be("MockTool");
        item3.Children[1].ToolInstance.InternalComponentName.Should().Be("MockTool2");
        item3.Children[2].ToolInstance.InternalComponentName.Should().Be("MockTool3-XMLFormatter");
        item3.Children[3].ToolInstance.InternalComponentName.Should().Be("MockTool4-XMLValidator");
    }

    [Fact]
    public void ToolsMenuRecent()
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();

        ReadOnlyObservableCollection<INotifyPropertyChanged> menuItems = guiToolProvider.HeaderAndBodyToolViewItems;
        menuItems.Should().HaveCount(3);
        menuItems[0].Should().BeOfType<GroupViewItem>();
        menuItems[1].Should().BeOfType<SeparatorViewItem>();
        menuItems[2].Should().BeOfType<GroupViewItem>();

        guiToolProvider.SetMostRecentUsedTool(((GroupViewItem)menuItems[2]).Children[0].ToolInstance);

        // Menu should be unchanged.
        menuItems.Should().HaveCount(3);
        menuItems[0].Should().BeOfType<GroupViewItem>();
        menuItems[1].Should().BeOfType<SeparatorViewItem>();
        menuItems[2].Should().BeOfType<GroupViewItem>();
    }

    [Fact]
    public void ToolsMenuFavorites()
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();
        ReadOnlyObservableCollection<INotifyPropertyChanged> menuItems = guiToolProvider.HeaderAndBodyToolViewItems;

        GuiToolViewItem toolToSetAsFavorite = ((GroupViewItem)menuItems[2]).Children[0];

        // At first, no favorites at all.
        menuItems.Should().HaveCount(3);
        menuItems[0].Should().BeOfType<GroupViewItem>();
        menuItems[1].Should().BeOfType<SeparatorViewItem>();
        menuItems[2].Should().BeOfType<GroupViewItem>();

        // Set item as favorite. The menu should then have "Favorites" group.
        guiToolProvider.SetToolIsFavorite(toolToSetAsFavorite.ToolInstance, isFavorite: true);
        menuItems.Should().HaveCount(4);
        menuItems[0].Should().BeOfType<GroupViewItem>();
        menuItems[1].Should().BeOfType<SeparatorViewItem>();
        menuItems[2].Should().BeOfType<GroupViewItem>();
        menuItems[3].Should().BeOfType<GroupViewItem>();
        ((GroupViewItem)menuItems[2]).DisplayTitle.Should().Be("Favorites");
        ((GroupViewItem)menuItems[3]).DisplayTitle.Should().Be("Group title");
        ((GroupViewItem)menuItems[2]).Children.Should().ContainSingle();
        ((GroupViewItem)menuItems[2]).Children[0].ToolInstance.Should().Be(toolToSetAsFavorite.ToolInstance);
        ((GroupViewItem)menuItems[2]).Children[0].Should().NotBe(toolToSetAsFavorite);

        // Unfavorite the item. The menu should remove "Favorite" group because the group is now empty.
        guiToolProvider.SetToolIsFavorite(toolToSetAsFavorite.ToolInstance, isFavorite: false);
        menuItems.Should().HaveCount(3);
        menuItems[0].Should().BeOfType<GroupViewItem>();
        menuItems[1].Should().BeOfType<SeparatorViewItem>();
        menuItems[2].Should().BeOfType<GroupViewItem>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("BOO", "NoSearchResults")]
    [InlineData("XML", "MockTool3-XMLFormatter", "MockTool4-XMLValidator")]
    [InlineData("XML Validator", "MockTool4-XMLValidator")]
    [InlineData("XML Valid", "MockTool4-XMLValidator")]
    public void ToolSearch(string searchQuery, params string[] toolNames)
    {
        GuiToolProvider guiToolProvider = MefProvider.Import<GuiToolProvider>();
        var results = new ObservableCollection<GuiToolViewItem>();
        guiToolProvider.SearchTools(searchQuery, results);

        results.Should().HaveSameCount(toolNames);
        for (int i = 0; i < toolNames.Length; i++)
        {
            results[i].ToolInstance.InternalComponentName.Should().Be(toolNames[i]);
        }
    }
}
