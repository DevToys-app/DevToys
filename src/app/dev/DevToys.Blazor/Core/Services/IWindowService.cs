namespace DevToys.Blazor.Core.Services;

public interface IWindowService
{
    event EventHandler<EventArgs>? WindowLostFocus;

    event EventHandler<EventArgs>? WindowLocationChanged;

    event EventHandler<EventArgs>? WindowSizeChanged;

    event EventHandler<EventArgs>? WindowClosing;
}
