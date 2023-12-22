using System.Text;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Core;
using Microsoft.Recognizers.Text;

namespace DevToys.Tools.Tools.Text.SmartCalculator;

[Export(typeof(IGuiTool))]
[Name("SmartCalculator")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0113',
    GroupName = PredefinedCommonToolGroupNames.Text,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.SmartCalculator.SmartCalculator",
    ShortDisplayTitleResourceName = nameof(SmartCalculator.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(SmartCalculator.LongDisplayTitle),
    DescriptionResourceName = nameof(SmartCalculator.Description),
    AccessibleNameResourceName = nameof(SmartCalculator.AccessibleName),
    SearchKeywordsResourceName = nameof(SmartCalculator.SearchKeywords))]
internal class SmartCalculatorGuiTool : IGuiTool
{
    // Use English by default
    private const string DefaultCulture = Culture.English;

    private readonly ParserAndInterpreter _parserAndInterpreter;
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput();
    private readonly IUIMultiLineTextInput _outputText = MultilineTextInput();
    private readonly TextDocument _textDocument = new();
    private readonly Task _warmUpTask;

    [ImportingConstructor]
    internal SmartCalculatorGuiTool(ParserAndInterpreterFactory parserAndInterpreterFactory)
    {
        _parserAndInterpreter = parserAndInterpreterFactory.CreateInstance(DefaultCulture, _textDocument);
        _warmUpTask = WarmupAsync(_textDocument, _parserAndInterpreter);
    }

    public UIToolView View
        => new(
            isScrollable: false,
            SplitGrid()
                .Vertical()
                .LeftPaneLength(new UIGridLength(5, UIGridUnitType.Fraction))
                .RightPaneLength(new UIGridLength(2, UIGridUnitType.Fraction))
                .WithLeftPaneChild(
                    _inputText
                        .Language("ini")
                        .OnTextChanged(OnTextChangedAsync))
                .WithRightPaneChild(
                    _outputText
                        .Language("ini")
                        .ReadOnly()));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        throw new NotImplementedException();
    }

    private async ValueTask OnTextChangedAsync(string text)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);
        await _warmUpTask;

        _textDocument.Text = text;
        IReadOnlyList<ParserAndInterpreterResultLine>? results = await _parserAndInterpreter.WaitAsync();

        var output = new StringBuilder();
        if (results is not null && results.Count > 0)
        {
            for (int i = 0; i < results.Count; i++)
            {
                ParserAndInterpreterResultLine result = results[i];

                string displayResult = string.Empty;
                if (result.SummarizedResultData is not null)
                {
                    bool isError = result.SummarizedResultData!.IsOfType(PredefinedTokenAndDataTypeNames.Error);
                    displayResult = result.SummarizedResultData.GetDisplayText(DefaultCulture);
                }

                output.AppendLine(displayResult);
            }
        }

        _outputText.Text(output.ToString());
    }

    private static async Task WarmupAsync(TextDocument textDocument, ParserAndInterpreter parserAndInterpreter)
    {
        // Passing this text to the interpreter will force .Net to compile all the regex from Microsoft.Recognizers.Text
        textDocument.Text
            = @"average between 0 and 10
                    1000 m2 / 10 m2
                    June 23 2022 at 4pm
                    25 (50)
                    20h
                    01/01/2022
                    1km
                    1km/h
                    1kg
                    25%
                    123
                    1rad
                    2 km2
                    1 USD
                    a fifth
                    the third
                    1 MB
                    1F
                    if 20% off 60 + 50 equals 98 then tax = 12 else tax = 13
                    1 < True
                    if one hundred thousand dollars of income + (30% tax / two people) > 150k then test
                    7/1900";
        await parserAndInterpreter.WaitAsync();
        textDocument.Text = string.Empty;
    }
}
