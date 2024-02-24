using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Tools.Converters.Cron;

namespace DevToys.UnitTests.Tools.Converters;

public sealed class CronParserGuiToolTests : MefBasedTest
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly UIToolView _toolView;
    private readonly CronParserGuiTool _tool;
    private readonly IUISingleLineTextInput _dateFormatText;
    private readonly IUISingleLineTextInput _cronExpressionText;
    private readonly IUISingleLineTextInput _outputCronDescriptionText;
    private readonly IUIInfoBar _infoBar;

    public CronParserGuiToolTests()
        : base(typeof(CronParserGuiTool).Assembly)
    {
        _settingsProvider = MefProvider.Import<ISettingsProvider>();
        _tool = (CronParserGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "CronParser")
            .Value;

        _toolView = _tool.View;

        _dateFormatText = (IUISingleLineTextInput)_toolView.GetChildElementById("cron-parser-date-format");
        _cronExpressionText = (IUISingleLineTextInput)_toolView.GetChildElementById("cron-parser-cron-expression");
        _outputCronDescriptionText = (IUISingleLineTextInput)_toolView.GetChildElementById("cron-parser-output-description");
        _infoBar = (IUIInfoBar)_toolView.GetChildElementById("cron-parser-info-bar");

        _settingsProvider.SetSetting(CronParserGuiTool.includeSeconds, true);
    }

    [Theory(DisplayName = "Invalid cron expression")]
    [InlineData("*")]
    [InlineData("* * * * * * * * *")]
    public void InvalidCronExpression(string input)
    {
        _infoBar.IsOpened.Should().BeFalse();
        _cronExpressionText.Text(input);
        _infoBar.IsOpened.Should().BeTrue();
    }

    [Theory(DisplayName = "Invalid date format")]
    [InlineData("hello")]
    public void InvalidDateFormat(string input)
    {
        _infoBar.IsOpened.Should().BeFalse();
        _dateFormatText.Text(input);
        _infoBar.IsOpened.Should().BeTrue();
    }

    [Theory(DisplayName = "Parse cron correctly")]
    [InlineData("* * * * * *", true, "Every second")]
    [InlineData("* * * * *", false, "Every minute")]
    public void ParseCron(string input, bool includeSeconds, string expectedResult)
    {
        _settingsProvider.SetSetting(CronParserGuiTool.includeSeconds, includeSeconds);
        _cronExpressionText.Text(input);

        _infoBar.IsOpened.Should().BeFalse();
        _outputCronDescriptionText.Text.Should().Be(expectedResult);
    }
}
