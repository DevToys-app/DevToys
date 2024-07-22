using DevToys.Api;

namespace DevToys.MacOS.Views;

internal sealed class AppMenuBar : NSMenu
{
    private readonly NSMenuItem _devToysMenu = new();
    private readonly NSMenu _devToysSubmenu = new();
    private readonly NSMenuItem _editMenu = new();
    private readonly NSMenu _editSubmenu;
    private readonly NSMenuItem _quitMenuItem;

    internal AppMenuBar()
    {
        // DevToys menu

        AddItem(_devToysMenu);
        _devToysMenu.Submenu = _devToysSubmenu;

        // add separator
        NSMenuItem separator = NSMenuItem.SeparatorItem;
        _devToysSubmenu.AddItem(separator);

        // add quit menu item
        _quitMenuItem = new NSMenuItem(Strings.AppMenuBar.AppMenuBar.Quit, "q", OnQuit); // TODO: Localize
        _devToysSubmenu.AddItem(_quitMenuItem);

        // Edit menu
        _editSubmenu = new NSMenu(Strings.AppMenuBar.AppMenuBar.Edit);
        _editMenu.Submenu = _editSubmenu;
        AddItem(_editMenu);

        // add paste menu item
        var cutMenuItem = new NSMenuItem(Strings.AppMenuBar.AppMenuBar.Cut, "x", OnCut);
        _editSubmenu.AddItem(cutMenuItem);

        // add paste menu item
        var copyMenuItem = new NSMenuItem(Strings.AppMenuBar.AppMenuBar.Copy, "c", OnCopy);
        _editSubmenu.AddItem(copyMenuItem);

        // add paste menu item
        var pasteMenuItem = new NSMenuItem(Strings.AppMenuBar.AppMenuBar.Paste, "v", OnPaste);
        _editSubmenu.AddItem(pasteMenuItem);

        // add paste menu item
        var selectAllMenuItem = new NSMenuItem(Strings.AppMenuBar.AppMenuBar.SelectAll, "a", OnSelectAll);
        _editSubmenu.AddItem(selectAllMenuItem);
    }

    private void OnQuit(object? sender, EventArgs e)
    {
        MainWindow.Instance.Close();
        NSApplication.SharedApplication.Terminate(_quitMenuItem);
    }

    private void OnCut(object? sender, EventArgs e)
    {
        MainWindow.Instance.CutInWebViewAsync().ForgetSafely();
    }

    private void OnCopy(object? sender, EventArgs e)
    {
        MainWindow.Instance.CopyInWebViewAsync().ForgetSafely();
    }

    private void OnPaste(object? sender, EventArgs e)
    {
        MainWindow.Instance.PasteInWebViewAsync().ForgetSafely();
    }

    private void OnSelectAll(object? sender, EventArgs e)
    {
        MainWindow.Instance.SelectAllInWebViewAsync().ForgetSafely();
    }
}
