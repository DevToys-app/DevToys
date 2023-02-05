#if __WASM__
using Microsoft.UI.Xaml;

namespace DevToys.UI.Framework.Controls;

public sealed partial class BackdropWindow
{
    public BackdropWindow(Window window)
    {
        Guard.IsNotNull(window);
        Window = window;
    }

    internal Window Window { get; }

    public partial void Resize(int width, int height)
    {
        // Has no effect in WASM.
    }

    public partial void Move(int x, int y)
    {
        // Has no effect in WASM.
    }

    public partial void Show()
    {
        Window.Activate();
        Shown?.Invoke(this, EventArgs.Empty);
    }
}
#endif
