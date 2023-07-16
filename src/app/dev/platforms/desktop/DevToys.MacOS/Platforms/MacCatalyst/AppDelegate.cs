using Foundation;
using UIKit;

namespace DevToys.MacOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    private readonly MauiProgram _program = new();

    protected override MauiApp CreateMauiApp() => _program.CreateMauiApp();

    public override void BuildMenu(IUIMenuBuilder builder)
    {
        base.BuildMenu(builder);

        // Remove "File" from app menu
        builder.RemoveMenu(UIMenuIdentifier.File.GetConstant());

        // Remove "Edit" from app menu
        builder.RemoveMenu(UIMenuIdentifier.Edit.GetConstant());

        // Remove "Format" from app menu
        builder.RemoveMenu(UIMenuIdentifier.Format.GetConstant());

        // Remove "Help" from app menu
        builder.RemoveMenu(UIMenuIdentifier.Help.GetConstant());

        // Remove "About" from app menu
        builder.RemoveMenu(UIMenuIdentifier.About.GetConstant());

        // TODO: Add custom "About" that navigates to Settings page.
        // TODO: Add a Settings menu that navigates to Settings page.
        // TODO: Add a Edit menu that works well (default one doesn't do much?).
        // TODO: Add a custom "Help" that offers to open feedback
    }
}
