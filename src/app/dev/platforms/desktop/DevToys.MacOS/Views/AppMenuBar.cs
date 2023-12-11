namespace DevToys.MacOS.Views;

internal sealed class AppMenuBar : NSMenu
{
    private readonly NSMenuItem _devToysMenu = new();
    private readonly NSMenu _devToysSubmenu = new();
    private readonly NSMenuItem _quitMenuItem;

    // TODO: Add About, Settings, Show All, Hide DevToys, Hide Others and more...
    internal AppMenuBar()
    {
        _quitMenuItem = new NSMenuItem($"Quit DevToys", "q", OnQuit); // TODO: Localize

        AddItem(_devToysMenu);
        _devToysMenu.Submenu = _devToysSubmenu;

        // add separator
        NSMenuItem separator = NSMenuItem.SeparatorItem;
        _devToysSubmenu.AddItem(separator);

        // add quit menu item
        _devToysSubmenu.AddItem(_quitMenuItem);
    }

    private void OnQuit(object? sender, EventArgs e)
    {
        MainWindow.Instance.Close();
        NSApplication.SharedApplication.Terminate(_quitMenuItem);
    }
}
