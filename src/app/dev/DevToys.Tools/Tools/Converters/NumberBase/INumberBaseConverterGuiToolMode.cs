using DevToys.Tools.Models.NumberBase;

namespace DevToys.Tools.Tools.Converters.NumberBase;

internal interface INumberBaseConverterGuiToolMode
{
    IUIElement View { get; }

    void OnInputChanged();

    void OnDataReceived(object? parsedData);
}
