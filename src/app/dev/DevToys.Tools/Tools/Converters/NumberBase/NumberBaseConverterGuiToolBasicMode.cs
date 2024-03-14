using DevToys.Tools.Helpers;
using DevToys.Tools.Models.NumberBase;
using Decimal = DevToys.Tools.Models.NumberBase.Decimal;

namespace DevToys.Tools.Tools.Converters.NumberBase;

internal sealed class NumberBaseConverterGuiToolBasicMode : INumberBaseConverterGuiToolMode
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly Action<string> _onErrorCallback;
    private readonly IUISingleLineTextInput _hexadecimalText = SingleLineTextInput();
    private readonly IUISingleLineTextInput _decimalText = SingleLineTextInput();
    private readonly IUISingleLineTextInput _octalText = SingleLineTextInput();
    private readonly IUISingleLineTextInput _binaryText = SingleLineTextInput();

    private bool _ignoreInputChanges;
    private (string number, INumberBaseDefinition<long> baseDefinition)? _inputValue;

    internal NumberBaseConverterGuiToolBasicMode(ISettingsProvider settingsProvider, Action<string> onErrorCallback)
    {
        Guard.IsNotNull(settingsProvider);
        Guard.IsNotNull(onErrorCallback);
        _settingsProvider = settingsProvider;
        _onErrorCallback = onErrorCallback;
    }

    public IUIElement View
        => Stack()
            .Vertical()
            .WithChildren(

                _hexadecimalText
                    .Title(NumberBaseConverter.Hexadecimal)
                    .OnTextChanged(OnHexadecimalChanged)
                    .CanCopyWhenEditable(),

                _decimalText
                    .Title(NumberBaseConverter.Decimal)
                    .OnTextChanged(OnDecimalChanged)
                    .CanCopyWhenEditable(),

                _octalText
                    .Title(NumberBaseConverter.Octal)
                    .OnTextChanged(OnOctalChanged)
                    .CanCopyWhenEditable(),

                _binaryText
                    .Title(NumberBaseConverter.Binary)
                    .OnTextChanged(OnBinaryChanged)
                    .CanCopyWhenEditable());

    public void OnDataReceived(object? parsedData)
    {
        if (parsedData is (string dataString, INumberBaseDefinition<long> numberBaseDefinition))
        {
            if (numberBaseDefinition == Hexadecimal.Instance)
            {
                _hexadecimalText.Text(dataString);
            }
            else if (numberBaseDefinition == Decimal.Instance)
            {
                _decimalText.Text(dataString);
            }
            else if (numberBaseDefinition == Octal.Instance)
            {
                _octalText.Text(dataString);
            }
            else if (numberBaseDefinition == Binary.Instance)
            {
                _binaryText.Text(dataString);
            }
        }
    }

    public void OnInputChanged()
    {
        if (_ignoreInputChanges)
        {
            return;
        }

        Guard.IsNotNull(_inputValue);

        bool format = _settingsProvider.GetSetting(NumberBaseConverterGuiTool.formatted);

        bool succeeded
            = NumberBaseHelper.TryConvertNumberBase(
                _inputValue.Value.number,
                _inputValue.Value.baseDefinition,
                format,
                out string hexadecimal,
                out string @decimal,
                out string octal,
                out string binary,
                out string error);

        UpdateUI(succeeded, hexadecimal, @decimal, octal, binary, error);
    }

    private void UpdateUI(
        bool succeeded,
        string hexadecimal,
        string @decimal,
        string octal,
        string binary,
        string error)
    {
        Guard.IsNotNull(_inputValue);

        _ignoreInputChanges = true;

        _onErrorCallback.Invoke(error);

        if (succeeded)
        {
            _hexadecimalText.Text(hexadecimal);
            _decimalText.Text(@decimal);
            _octalText.Text(octal);
            _binaryText.Text(binary);
        }
        else
        {
            if (_inputValue.Value.baseDefinition == Hexadecimal.Instance)
            {
                _decimalText.Text(string.Empty);
                _octalText.Text(string.Empty);
                _binaryText.Text(string.Empty);
            }
            else if (_inputValue.Value.baseDefinition == Decimal.Instance)
            {
                _hexadecimalText.Text(string.Empty);
                _octalText.Text(string.Empty);
                _binaryText.Text(string.Empty);
            }
            else if (_inputValue.Value.baseDefinition == Octal.Instance)
            {
                _hexadecimalText.Text(string.Empty);
                _decimalText.Text(string.Empty);
                _binaryText.Text(string.Empty);
            }
            else if (_inputValue.Value.baseDefinition == Binary.Instance)
            {
                _hexadecimalText.Text(string.Empty);
                _decimalText.Text(string.Empty);
                _octalText.Text(string.Empty);
            }
        }

        _ignoreInputChanges = false;
    }

    private void OnHexadecimalChanged(string number)
    {
        _inputValue = new(number, Hexadecimal.Instance);
        OnInputChanged();
    }

    private void OnDecimalChanged(string number)
    {
        _inputValue = new(number, Decimal.Instance);
        OnInputChanged();
    }

    private void OnOctalChanged(string number)
    {
        _inputValue = new(number, Octal.Instance);
        OnInputChanged();
    }

    private void OnBinaryChanged(string number)
    {
        _inputValue = new(number, Binary.Instance);
        OnInputChanged();
    }
}
