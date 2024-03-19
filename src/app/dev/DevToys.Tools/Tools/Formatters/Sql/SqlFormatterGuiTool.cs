using DevToys.Tools.Helpers.SqlFormatter;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Formatters.Sql;

[Export(typeof(IGuiTool))]
[Name("SqlFormatter")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0114',
    GroupName = PredefinedCommonToolGroupNames.Formatters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Formatters.Sql.SqlFormatter",
    ShortDisplayTitleResourceName = nameof(SqlFormatter.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(SqlFormatter.LongDisplayTitle),
    DescriptionResourceName = nameof(SqlFormatter.Description),
    AccessibleNameResourceName = nameof(SqlFormatter.AccessibleName))]
internal sealed partial class SqlFormatterGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Which indentation the tool should use.
    /// </summary>
    private static readonly SettingDefinition<Indentation> indentationMode
        = new(name: $"{nameof(SqlFormatterGuiTool)}.{nameof(indentationMode)}", defaultValue: Indentation.TwoSpaces);

    /// <summary>
    /// The SQL language to consider when formatting.
    /// </summary>
    private static readonly SettingDefinition<SqlLanguage> sqlLanguage
        = new(name: $"{nameof(SqlFormatterGuiTool)}.{nameof(sqlLanguage)}", defaultValue: SqlLanguage.Sql);

    /// <summary>
    /// Whether to use leading commas in the formatted SQL queries.
    /// </summary>
    private static readonly SettingDefinition<bool> useLeadingComma
        = new(name: $"{nameof(SqlFormatterGuiTool)}.{nameof(useLeadingComma)}", defaultValue: false);

    private enum GridColumn
    {
        Content
    }

    private enum GridRow
    {
        Header,
        Content,
        Footer
    }

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIMultiLineTextInput _inputTextArea = MultilineTextInput("sql-input-text-area");
    private readonly IUIMultiLineTextInput _outputTextArea = MultilineTextInput("sql-output-text-area");

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public SqlFormatterGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
    }

    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: true,
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()
                .Rows(
                    (GridRow.Header, Auto),
                    (GridRow.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Columns(
                    (GridColumn.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
            .Cells(
                Cell(
                    GridRow.Header,
                    GridColumn.Content,
                    Stack().Vertical().WithChildren(
                        Label().Text(SqlFormatter.Configuration),
                        Setting("sql-text-newLineOnAttributes-setting")
                        .Icon("FluentSystemIcons", '\uF2EF')
                        .Title(SqlFormatter.SqlLanguage)
                        .Handle(
                            _settingsProvider,
                            sqlLanguage,
                            OnSqlLanguageChanged,
                            Item(SqlFormatter.Db2, SqlLanguage.Db2),
                            Item(SqlFormatter.MariaDb, SqlLanguage.MariaDb),
                            Item(SqlFormatter.MySql, SqlLanguage.MySql),
                            Item(SqlFormatter.N1ql, SqlLanguage.N1ql),
                            Item(SqlFormatter.PlSql, SqlLanguage.PlSql),
                            Item(SqlFormatter.PostgreSql, SqlLanguage.PostgreSql),
                            Item(SqlFormatter.RedShift, SqlLanguage.RedShift),
                            Item(SqlFormatter.SparkSql, SqlLanguage.Spark),
                            Item(SqlFormatter.StandardSql, SqlLanguage.Sql),
                            Item(SqlFormatter.TransactSql, SqlLanguage.Tsql)
                        ),
                        Setting("sql-text-indentation-setting")
                        .Icon("FluentSystemIcons", '\uF6F8')
                        .Title(SqlFormatter.Indentation)
                        .Handle(
                            _settingsProvider,
                            indentationMode,
                            OnIndentationModelChanged,
                            Item(SqlFormatter.TwoSpaces, Indentation.TwoSpaces),
                            Item(SqlFormatter.FourSpaces, Indentation.FourSpaces),
                            Item(SqlFormatter.OneTab, Indentation.OneTab)
                        ),
                        Setting("sql-leading-comma-setting")
                           .Icon("FluentSystemIcons", '\uF18D')
                           .Title(SqlFormatter.LeadingComma)
                           .Handle(
                                _settingsProvider,
                                useLeadingComma,
                                OnUseLeadingCommaChanged
                            )
                    )
                ),
                Cell(
                    GridRow.Content,
                    GridColumn.Content,
                    SplitGrid()
                        .Vertical()
                        .WithLeftPaneChild(
                            _inputTextArea
                                .Language("sql")
                                .Title(SqlFormatter.Input)
                                .OnTextChanged(OnInputTextChanged))
                        .WithRightPaneChild(
                            _outputTextArea
                                .Language("sql")
                                .Title(SqlFormatter.Output)
                                .ReadOnly()
                                .Extendable())
                )
            )
        );

    // Smart detection handler.
    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnIndentationModelChanged(Indentation indentationMode)
    {
        StartFormat(_inputTextArea.Text);
    }

    private void OnSqlLanguageChanged(SqlLanguage sqlLanguage)
    {
        StartFormat(_inputTextArea.Text);
    }

    private void OnUseLeadingCommaChanged(bool useLeadingComma)
    {
        StartFormat(_inputTextArea.Text);
    }

    private void OnInputTextChanged(string text)
    {
        StartFormat(text);
    }

    private void StartFormat(string text)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = FormatAsync(
            text,
            _settingsProvider.GetSetting(indentationMode),
            _settingsProvider.GetSetting(sqlLanguage),
            _settingsProvider.GetSetting(useLeadingComma),
            _cancellationTokenSource.Token);
    }

    private async Task FormatAsync(string input, Indentation indentationSetting, SqlLanguage sqlLanguageSetting, bool useLeadingComma, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            string formatResult = SqlFormatterHelper.Format(
                input,
                sqlLanguageSetting,
                new SqlFormatterOptions(
                            indentationSetting,
                            Uppercase: true,
                            LinesBetweenQueries: 2,
                            UseLeadingComma: useLeadingComma));

            _outputTextArea.Text(formatResult);
        }
    }
}
