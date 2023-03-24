#if __MACCATALYST__
using Microsoft.UI.Xaml;

namespace DevToys.UI.Framework.Controls;

public sealed partial class BackdropWindow
{
    public BackdropWindow(Window window)
    {
        Guard.IsNotNull(window);
        Window = window;
        IsCompactOverlayModeSupported = false;
    }

    internal Window Window { get; }

    public partial void Resize(int width, int height)
    {
        // throw new NotImplementedException(); // TODO.
    }

    public partial void Move(int x, int y)
    {
        // throw new NotImplementedException(); // TODO.
    }

    public partial void Show()
    {
        // throw new NotImplementedException(); // TODO.

        Window.Activate();
        Shown?.Invoke(this, EventArgs.Empty);
    }

    private void SetStartupWindowLocation()
    {
        // throw new NotImplementedException(); // TODO
        // switch (WindowStartupLocation)
        // {
        //     case WindowStartupLocation.CenterOwner:
        //         // TODO
        //         throw new NotImplementedException();
        //
        //     case WindowStartupLocation.CenterScreen:
        //         // TODO
        //         throw new NotImplementedException();
        //
        //     case WindowStartupLocation.Manual:
        //         // Let the OS decide for us.
        //         break;
        //
        //     default:
        //         throw new NotSupportedException();
        // }
    }

    public partial bool IsInCompactOverlayMode()
    {
        // throw new NotImplementedException(); // TODO.
        return false;
    }

    public partial void TryToggleCompactOverlayMode()
    {
        // throw new NotImplementedException(); // TODO.
        // CompactOverlayModeChanged?.Invoke(this, EventArgs.Empty);
    }

    public partial void SetTitle(string title)
    {
        Window.Title = title;
    }
}
#endif
