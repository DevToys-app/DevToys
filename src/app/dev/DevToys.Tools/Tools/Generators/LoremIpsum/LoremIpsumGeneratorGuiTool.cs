using System.Globalization;
using System.Reflection.Emit;
using DevToys.Api;
using DevToys.Tools.Helpers.LoremIpsum;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.Generators.UUID;

namespace DevToys.Tools.Tools.Generators.LoremIpsum;

[Export(typeof(IGuiTool))]
[Name("LoremIpsumGenerator")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0111',
    GroupName = PredefinedCommonToolGroupNames.Generators,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Generators.LoremIpsum.LoremIpsumGenerator",
    ShortDisplayTitleResourceName = nameof(LoremIpsumGenerator.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(LoremIpsumGenerator.LongDisplayTitle),
    DescriptionResourceName = nameof(LoremIpsumGenerator.Description),
    AccessibleNameResourceName = nameof(LoremIpsumGenerator.AccessibleName))]
internal sealed class LoremIpsumGeneratorGuiTool : IGuiTool
{
    /// <summary>
    /// The type of lipsum to generate.
    /// </summary>
    private static readonly SettingDefinition<LipsumsCorpus> lipsum
        = new(
            name: $"{nameof(LoremIpsumGeneratorGuiTool)}.{nameof(lipsum)}",
            defaultValue: LipsumsCorpus.LoremIpsum);

    /// <summary>
    /// Whether a word, sentence, or paragraph should be generated.
    /// </summary>
    private static readonly SettingDefinition<Features> type
        = new(
            name: $"{nameof(LoremIpsumGeneratorGuiTool)}.{nameof(type)}",
            defaultValue: Features.Paragraphs);

    /// <summary>
    /// The amount of words, sentences, or paragraphs to generate.
    /// </summary>
    private static readonly SettingDefinition<int> length
        = new(
            name: $"{nameof(LoremIpsumGeneratorGuiTool)}.{nameof(length)}",
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

    private LipsumGenerator _generator;

    [ImportingConstructor]
    public LoremIpsumGeneratorGuiTool(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
        _generator = new LipsumGenerator(_settingsProvider.GetSetting(lipsum));

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

                                    Label().Text(LoremIpsumGenerator.ConfigurationTitle),

                                    Setting()
                                        .Icon("FluentSystemIcons", '\uE178')
                                        .Title(LoremIpsumGenerator.LipsumTitle)
                                        .Handle(
                                            _settingsProvider,
                                            lipsum,
                                            OnSettingChanged,
                                            Item(LoremIpsumGenerator.LipsumChildHarold, LipsumsCorpus.ChildHarold),
                                            Item(LoremIpsumGenerator.LipsumDecameron, LipsumsCorpus.Decameron),
                                            Item(LoremIpsumGenerator.LipsumFaust, LipsumsCorpus.Faust),
                                            Item(LoremIpsumGenerator.LipsumInDerFremde, LipsumsCorpus.InDerFremde),
                                            Item(LoremIpsumGenerator.LipsumLeBateauIvre, LipsumsCorpus.LeBateauIvre),
                                            Item(LoremIpsumGenerator.LipsumLeMasque, LipsumsCorpus.LeMasque),
                                            Item(LoremIpsumGenerator.LipsumLoremIpsum, LipsumsCorpus.LoremIpsum),
                                            Item(LoremIpsumGenerator.LipsumNagyonFaj, LipsumsCorpus.NagyonFaj),
                                            Item(LoremIpsumGenerator.LipsumOmagyar, LipsumsCorpus.Omagyar),
                                            Item(LoremIpsumGenerator.LipsumRobinsonoKruso, LipsumsCorpus.RobinsonoKruso),
                                            Item(LoremIpsumGenerator.LipsumTheRaven, LipsumsCorpus.TheRaven),
                                            Item(LoremIpsumGenerator.LipsumTierrayLuna, LipsumsCorpus.TierrayLuna)),

                                    Setting()
                                        .Icon("FluentSystemIcons", '\uEB28')
                                        .Title(LoremIpsumGenerator.TypeTitle)
                                        .Description(LoremIpsumGenerator.TypeDescription)
                                        .Handle(
                                            _settingsProvider,
                                            type,
                                            OnSettingChanged,
                                            Item(LoremIpsumGenerator.WordsType, Features.Words),
                                            Item(LoremIpsumGenerator.SentencesType, Features.Sentences),
                                            Item(LoremIpsumGenerator.ParagraphsType, Features.Paragraphs)),

                                    Setting()
                                        .Icon("FluentSystemIcons", '\uF57D')
                                        .Title(LoremIpsumGenerator.LengthTitle)
                                        .Description(LoremIpsumGenerator.LengthDescription)
                                        .InteractiveElement(
                                            NumberInput()
                                                .HideCommandBar()
                                                .Minimum(1)
                                                .OnValueChanged(OnLengthChanged)
                                                .Value(_settingsProvider.GetSetting(length)))))),

                Cell(
                    GridRow.Results,
                    GridColumn.Stretch,

                    _outputText
                        .Title(LoremIpsumGenerator.Output)
                        .ReadOnly()
                        .AlwaysWrap()
                        .CommandBarExtraContent(
                            Button()
                                .Icon("FluentSystemIcons", '\uF13D')
                                .Text(LoremIpsumGenerator.Refresh)
                                .OnClick(OnGenerateButtonClick)))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }

    private void OnSettingChanged(LipsumsCorpus value)
    {
        _generator = new LipsumGenerator(value);
        OnGenerateButtonClick();
    }

    private void OnSettingChanged(Features value)
    {
        OnGenerateButtonClick();
    }

    private void OnLengthChanged(double value)
    {
        _settingsProvider.SetSetting(length, (int)value);
        OnGenerateButtonClick();
    }

    private void OnGenerateButtonClick()
    {
        int length = _settingsProvider.GetSetting(LoremIpsumGeneratorGuiTool.length);
        if (length <= 0)
        {
            _outputText.Text(string.Empty);
            return;
        }

        Features type = _settingsProvider.GetSetting(LoremIpsumGeneratorGuiTool.type);

        Guard.IsNotNull(_generator);
        string output = _generator.GenerateLipsum(length, type);

        _outputText.Text(output);
    }
}
