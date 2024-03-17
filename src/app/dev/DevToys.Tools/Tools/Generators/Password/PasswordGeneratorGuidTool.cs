using System.Text;
using DevToys.Tools.Helpers;

namespace DevToys.Tools.Tools.Generators.Password;

[Export(typeof(IGuiTool))]
[Name("PasswordGenerator")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uE8C9',
    GroupName = PredefinedCommonToolGroupNames.Generators,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Generators.Password.PasswordGenerator",
    ShortDisplayTitleResourceName = nameof(PasswordGenerator.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(PasswordGenerator.LongDisplayTitle),
    DescriptionResourceName = nameof(PasswordGenerator.Description),
    SearchKeywordsResourceName = nameof(PasswordGenerator.SearchKeywords),
    AccessibleNameResourceName = nameof(PasswordGenerator.AccessibleName))]
internal sealed class PasswordGeneratorGuidTool : IGuiTool
{
    /// <summary>
    /// Whether the password should include uppercase characters.
    /// </summary>
    private static readonly SettingDefinition<bool> uppercase
        = new(
            name: $"{nameof(PasswordGeneratorGuidTool)}.{nameof(uppercase)}",
            defaultValue: true);

    /// <summary>
    /// Whether the password should include lowercase characters.
    /// </summary>
    private static readonly SettingDefinition<bool> lowercase
        = new(
            name: $"{nameof(PasswordGeneratorGuidTool)}.{nameof(lowercase)}",
            defaultValue: true);

    /// <summary>
    /// Whether the password should include numbers.
    /// </summary>
    private static readonly SettingDefinition<bool> numbers
        = new(
            name: $"{nameof(PasswordGeneratorGuidTool)}.{nameof(numbers)}",
            defaultValue: true);

    /// <summary>
    /// Whether the password should include special characters.
    /// </summary>
    private static readonly SettingDefinition<bool> specialCharacters
        = new(
            name: $"{nameof(PasswordGeneratorGuidTool)}.{nameof(specialCharacters)}",
            defaultValue: true);

    /// <summary>
    /// Excluded characters from password.
    /// </summary>
    private static readonly SettingDefinition<string> excludedCharacters
        = new(
            name: $"{nameof(PasswordGeneratorGuidTool)}.{nameof(excludedCharacters)}",
            defaultValue: string.Empty);

    /// <summary>
    /// How long the password should be.
    /// </summary>
    private static readonly SettingDefinition<int> length
        = new(
            name: $"{nameof(PasswordGeneratorGuidTool)}.{nameof(length)}",
            defaultValue: 30);

    /// <summary>
    /// How many passwords should be generated at once.
    /// </summary>
    private static readonly SettingDefinition<int> passwordsToGenerate
        = new(
            name: $"{nameof(PasswordGeneratorGuidTool)}.{nameof(passwordsToGenerate)}",
            defaultValue: 1);

    private enum GridColumn
    {
        Stretch
    }

    private enum GridRow
    {
        Settings,
        Results
    }

    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIMultiLineTextInput _outputText = MultilineTextInput();
    private readonly IUISetting _excludedCharactersSetting = Setting();
    private readonly IUIInfoBar _infoBar = InfoBar();

    [ImportingConstructor]
    public PasswordGeneratorGuidTool(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;

        OnGenerateButtonClick();
    }

    public UIToolView View
        => new(
            isScrollable: true,
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()
                .Rows(
                    (GridRow.Settings, Auto),
                    (GridRow.Results, new UIGridLength(1, UIGridUnitType.Fraction)))
                .Columns(
                    (GridColumn.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

            .Cells(
                Cell(
                    GridRow.Settings,
                    GridColumn.Stretch,
                    Stack()
                        .Vertical()
                        .LargeSpacing()
                        .WithChildren(

                            Stack()
                                .Vertical()
                                .WithChildren(

                                    Label().Text(PasswordGenerator.ConfigurationTitle),

                                    SettingGroup()
                                        .Icon("FluentSystemIcons", '\uF57D')
                                        .Title(PasswordGenerator.Length)
                                        .InteractiveElement(
                                            NumberInput()
                                                .HideCommandBar()
                                                .Minimum(5)
                                                .OnValueChanged(OnLengthChanged)
                                                .Value(_settingsProvider.GetSetting(length)))
                                        .WithSettings(

                                            Setting()
                                                .Title(PasswordGenerator.Lowercase)
                                                .Description(PasswordGenerator.LowercaseDescription)
                                                .Handle(
                                                    _settingsProvider,
                                                    lowercase,
                                                    stateDescriptionWhenOn: PasswordGenerator.LowercaseDescriptionStateOn,
                                                    stateDescriptionWhenOff: null,
                                                    OnSettingChanged),

                                            Setting()
                                                .Title(PasswordGenerator.Uppercase)
                                                .Description(PasswordGenerator.UppercaseDescription)
                                                .Handle(
                                                    _settingsProvider,
                                                    uppercase,
                                                    stateDescriptionWhenOn: PasswordGenerator.UppercaseDescriptionStateOn,
                                                    stateDescriptionWhenOff: null,
                                                    OnSettingChanged),

                                            Setting()
                                                .Title(PasswordGenerator.Digits)
                                                .Description(PasswordGenerator.DigitsDescription)
                                                .Handle(
                                                    _settingsProvider,
                                                    numbers,
                                                    stateDescriptionWhenOn: PasswordGenerator.DigitsDescriptionStateOn,
                                                    stateDescriptionWhenOff: null,
                                                    OnSettingChanged),

                                            Setting()
                                                .Title(PasswordGenerator.SpecialCharacters)
                                                .Description(PasswordGenerator.SpecialCharactersDescription)
                                                .Handle(
                                                    _settingsProvider,
                                                    specialCharacters,
                                                    stateDescriptionWhenOn: PasswordGenerator.SpecialCharactersDescriptionStateOn,
                                                    stateDescriptionWhenOff: null,
                                                    OnSettingChanged),

                                            _excludedCharactersSetting
                                                .Title(PasswordGenerator.ExcludeCharacters)
                                                .StateDescription(GenerateExcludedCharactersDescriptionState())
                                                .InteractiveElement(
                                                    SingleLineTextInput()
                                                        .HideCommandBar()
                                                        .Text(_settingsProvider.GetSetting(excludedCharacters))
                                                        .OnTextChanged(OnExcludedCharactersChanged))),

                                    _infoBar
                                        .Warning()
                                        .Description(PasswordGenerator.NoCharacterSetsWarning)
                                        .NonClosable()),

                            Stack()
                                .Vertical()
                                .WithChildren(

                                    Label().Text(PasswordGenerator.GenerateTitle),
                                    Stack()
                                        .Horizontal()
                                        .SmallSpacing()
                                        .WithChildren(

                                            Button()
                                                .AccentAppearance()
                                                .Text(PasswordGenerator.GenerateButton)
                                                .OnClick(OnGenerateButtonClick),

                                            Label().Style(UILabelStyle.BodyStrong).Text(PasswordGenerator.MultiplySymbol),

                                            NumberInput()
                                                .HideCommandBar()
                                                .Minimum(1)
                                                .Maximum(10000)
                                                .OnValueChanged(OnNumberOfPasswordsToGenerateChanged)
                                                .Value(_settingsProvider.GetSetting(passwordsToGenerate)))))),

                Cell(
                    GridRow.Results,
                    GridColumn.Stretch,

                    _outputText
                        .Title(PasswordGenerator.Output)
                        .ReadOnly())));

    private bool HasAnyCharacterSets
        => _settingsProvider.GetSetting(uppercase)
        || _settingsProvider.GetSetting(lowercase)
        || _settingsProvider.GetSetting(numbers)
        || _settingsProvider.GetSetting(specialCharacters);

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }

    private void OnSettingChanged(bool value)
    {
        OnGenerateButtonClick();
    }

    private void OnLengthChanged(double value)
    {
        _settingsProvider.SetSetting(length, (int)value);
        OnGenerateButtonClick();
    }

    private void OnExcludedCharactersChanged(string value)
    {
        _settingsProvider.SetSetting(excludedCharacters, value);
        _excludedCharactersSetting.StateDescription(GenerateExcludedCharactersDescriptionState());

        OnGenerateButtonClick();
    }

    private void OnNumberOfPasswordsToGenerateChanged(double value)
    {
        _settingsProvider.SetSetting(passwordsToGenerate, (int)value);
        OnGenerateButtonClick();
    }

    private void OnGenerateButtonClick()
    {
        // There are no character sets selected, so we can't generate anything.
        if (!HasAnyCharacterSets)
        {
            _infoBar.Open();
            _outputText.Text(string.Empty);
            return;
        }
        else
        {
            _infoBar.Close();
        }

        bool hasUppercase = _settingsProvider.GetSetting(uppercase);
        bool hasLowercase = _settingsProvider.GetSetting(lowercase);
        bool hasNumbers = _settingsProvider.GetSetting(numbers);
        bool hasSpecialCharacters = _settingsProvider.GetSetting(specialCharacters);
        char[] excludedCharactersList = _settingsProvider.GetSetting(excludedCharacters).ToCharArray();
        int passwordLength = _settingsProvider.GetSetting(length);

        // Generate a random password using the the combined character set.
        var newPasswords = new StringBuilder();
        for (int i = 0; i < _settingsProvider.GetSetting(passwordsToGenerate); i++)
        {
            string password
                = PasswordGeneratorHelper.GeneratePassword(
                    passwordLength,
                    hasUppercase,
                    hasLowercase,
                    hasNumbers,
                    hasSpecialCharacters,
                    excludedCharactersList);

            if (password.Length > 0)
            {
                newPasswords.AppendLine(password);
            }
        }

        _outputText.Text(newPasswords.ToString());
    }

    private string GenerateExcludedCharactersDescriptionState()
    {
        if (_settingsProvider.GetSetting(excludedCharacters).Length == 0)
        {
            return string.Empty;
        }
        else
        {
            return string.Format(PasswordGenerator.ExcludedCharactersDescriptionState, _settingsProvider.GetSetting(excludedCharacters));
        }
    }
}
