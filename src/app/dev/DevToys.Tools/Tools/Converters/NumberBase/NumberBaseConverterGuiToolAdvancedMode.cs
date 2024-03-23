using DevToys.Tools.Helpers;
using DevToys.Tools.Models.NumberBase;

namespace DevToys.Tools.Tools.Converters.NumberBase;

internal sealed class NumberBaseConverterGuiToolAdvancedMode : INumberBaseConverterGuiToolMode
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly Action<string> _onErrorCallback;
    private readonly IUISelectDropDownList _inputDictionarySelectDropDownList = SelectDropDownList();
    private readonly IUISelectDropDownList _outputDictionarySelectDropDownList = SelectDropDownList();
    private readonly IUISingleLineTextInput _inputCustomFormat = SingleLineTextInput();
    private readonly IUISingleLineTextInput _outputCustomFormat = SingleLineTextInput();
    private readonly IUISingleLineTextInput _inputText = SingleLineTextInput();
    private readonly IUISingleLineTextInput _outputText = SingleLineTextInput();

    private readonly bool _ignoreInputChanges;

    internal NumberBaseConverterGuiToolAdvancedMode(ISettingsProvider settingsProvider, Action<string> onErrorCallback)
    {
        Guard.IsNotNull(settingsProvider);
        Guard.IsNotNull(onErrorCallback);
        _settingsProvider = settingsProvider;
        _onErrorCallback = onErrorCallback;

        _ignoreInputChanges = true;
        InitializeInputAndOutputDictionarySelectDropDownList(_inputDictionarySelectDropDownList, OnSelectedInputDictionaryChanged);
        InitializeInputAndOutputDictionarySelectDropDownList(_outputDictionarySelectDropDownList, OnSelectedOutputDictionaryChanged);
        _ignoreInputChanges = false;
    }

    public IUIElement View
        => Stack()
            .Vertical()
            .LargeSpacing()
            .WithChildren(

                // Input
                Stack()
                    .Vertical()
                    .SmallSpacing()
                    .WithChildren(

                        SettingGroup()
                            .Icon("FluentSystemIcons", '\uF2B0')
                            .Title(NumberBaseConverter.InputDictionary)
                            .InteractiveElement(_inputDictionarySelectDropDownList)
                            .WithChildren(
                                _inputCustomFormat
                                    .Title(NumberBaseConverter.CustomFormat)
                                    .HideCommandBar()
                                    .OnTextChanged(OnInputTextChanged)),

                        _inputText
                            .Title(NumberBaseConverter.Input)
                            .OnTextChanged(OnInputTextChanged)),

                // Output
                Stack()
                    .Vertical()
                    .SmallSpacing()
                    .WithChildren(

                        SettingGroup()
                            .Icon("FluentSystemIcons", '\uF2AA')
                            .Title(NumberBaseConverter.OutputDictionary)
                            .InteractiveElement(_outputDictionarySelectDropDownList)
                            .WithChildren(
                                _outputCustomFormat
                                    .Title(NumberBaseConverter.CustomFormat)
                                    .HideCommandBar()
                                    .OnTextChanged(OnInputTextChanged)),

                        _outputText
                            .Title(NumberBaseConverter.Output)
                            .ReadOnly()));

    public void OnDataReceived(object? parsedData)
    {
        throw new NotImplementedException();
    }

    public void OnInputChanged()
    {
        if (_ignoreInputChanges)
        {
            return;
        }

        // Retrieve the input number base definition.
        INumberBaseDefinition<ulong> inputNumberBase;
        if (_inputDictionarySelectDropDownList.SelectedItem?.Text == NumberBaseConverter.CustomFormat)
        {
            if (_inputCustomFormat.Text.Length < 2)
            {
                _onErrorCallback.Invoke(NumberBaseConverter.CustomDictionaryTooSmall);
                _outputText.Text(string.Empty);
                return;
            }

            inputNumberBase = new CustomUnsigned(_inputCustomFormat.Text);
        }
        else
        {
            Guard.IsNotNull(_inputDictionarySelectDropDownList.SelectedItem);
            Guard.IsNotNull(_inputDictionarySelectDropDownList.SelectedItem.Value);
            Guard.IsAssignableToType(_inputDictionarySelectDropDownList.SelectedItem.Value, typeof(INumberBaseDefinition<ulong>));
            inputNumberBase = (INumberBaseDefinition<ulong>)_inputDictionarySelectDropDownList.SelectedItem.Value;
        }

        // Retrieve the output number base definition.
        INumberBaseDefinition<ulong> outputNumberBase;
        if (_outputDictionarySelectDropDownList.SelectedItem?.Text == NumberBaseConverter.CustomFormat)
        {
            if (_outputCustomFormat.Text.Length < 2)
            {
                _onErrorCallback.Invoke(NumberBaseConverter.CustomDictionaryTooSmall);
                _outputText.Text(string.Empty);
                return;
            }

            outputNumberBase = new CustomUnsigned(_outputCustomFormat.Text);
        }
        else
        {
            Guard.IsNotNull(_outputDictionarySelectDropDownList.SelectedItem);
            Guard.IsNotNull(_outputDictionarySelectDropDownList.SelectedItem.Value);
            Guard.IsAssignableToType(_outputDictionarySelectDropDownList.SelectedItem.Value, typeof(INumberBaseDefinition<ulong>));
            outputNumberBase = (INumberBaseDefinition<ulong>)_outputDictionarySelectDropDownList.SelectedItem.Value;
        }

        // Convert the number.
        bool format = _settingsProvider.GetSetting(NumberBaseConverterGuiTool.formatted);
        bool succeeded
            = NumberBaseHelper.TryConvertNumberBase(
                _inputText.Text,
                inputNumberBase,
                outputNumberBase,
                format,
                out string result,
                out string error);

        // Display result.
        _onErrorCallback.Invoke(error);

        if (succeeded)
        {
            _outputText.Text(result);
        }
        else
        {
            _outputText.Text(string.Empty);
        }
    }

    private void OnInputTextChanged(string _)
    {
        OnInputChanged();
    }

    private void OnSelectedInputDictionaryChanged()
    {
        if (_inputDictionarySelectDropDownList.SelectedItem?.Text == NumberBaseConverter.CustomFormat)
        {
            _inputCustomFormat.Enable();
        }
        else
        {
            _inputCustomFormat.Disable();
        }

        OnInputChanged();
    }

    private void OnSelectedOutputDictionaryChanged()
    {
        if (_outputDictionarySelectDropDownList.SelectedItem?.Text == NumberBaseConverter.CustomFormat)
        {
            _outputCustomFormat.Enable();
        }
        else
        {
            _outputCustomFormat.Disable();
        }

        OnInputChanged();
    }

    private static void InitializeInputAndOutputDictionarySelectDropDownList(IUISelectDropDownList selectDropDownList, Action onSelectedItemChanged)
    {
        selectDropDownList
            .WithItems(
                Item(RFC4648Base16.Instance.DisplayName, RFC4648Base16.Instance),
                Item(RFC4648Base32.Instance.DisplayName, RFC4648Base32.Instance),
                Item(RFC4648Base32ExtendedHex.Instance.DisplayName, RFC4648Base32ExtendedHex.Instance),
                Item(RFC4648Base64.Instance.DisplayName, RFC4648Base64.Instance),
                Item(RFC4648Base64UrlEncode.Instance.DisplayName, RFC4648Base64UrlEncode.Instance),
                Item(NumberBaseConverter.CustomFormat, null))
            .OnItemSelected((_) => onSelectedItemChanged())
            .Select(0);
    }
}
