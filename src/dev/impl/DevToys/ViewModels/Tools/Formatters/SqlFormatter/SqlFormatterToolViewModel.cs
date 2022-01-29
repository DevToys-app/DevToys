#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Helpers.SqlFormatter;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.SqlFormatter;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.ViewModels.Tools.SqlFormatter
{
    [Export(typeof(SqlFormatterToolViewModel))]
    public sealed class SqlFormatterToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// The indentation to apply while formatting.
        /// </summary>
        private static readonly SettingDefinition<Indentation> Indentation
            = new(
                name: $"{nameof(SqlFormatterToolViewModel)}.{nameof(Indentation)}",
                isRoaming: true,
                defaultValue: Models.Indentation.TwoSpaces);

        /// <summary>
        /// The SQL language to consider when formatting.
        /// </summary>
        private static readonly SettingDefinition<SqlLanguage> SqlLanguage
            = new(
                name: $"{nameof(SqlFormatterToolViewModel)}.{nameof(SqlLanguage)}",
                isRoaming: true,
                defaultValue: Helpers.SqlFormatter.SqlLanguage.Sql);

        private readonly IMarketingService _marketingService;
        private readonly Queue<string> _formattingQueue = new();

        private bool _toolSuccessfullyWorked;
        private bool _formattingInProgress;
        private string? _inputValue;
        private string? _outputValue;

        public Type View { get; } = typeof(SqlFormatterToolPage);

        internal SqlFormatterStrings Strings => LanguageManager.Instance.SqlFormatter;

        /// <summary>
        /// Gets or sets the desired indentation.
        /// </summary>
        internal IndentationDisplayPair IndentationMode
        {
            get
            {
                Indentation settingsValue = SettingsProvider.GetSetting(Indentation);
                IndentationDisplayPair? indentation = Indentations.FirstOrDefault(x => x.Value == settingsValue);
                return indentation ?? IndentationDisplayPair.TwoSpaces;
            }
            set
            {
                if (IndentationMode != value)
                {
                    SettingsProvider.SetSetting(Indentation, value.Value);
                    OnPropertyChanged();
                    QueueFormatting();
                }
            }
        }

        /// <summary>
        /// Gets or sets the desired SQL language to use.
        /// </summary>
        internal SqlLanguageDisplayPair SqlLanguageMode
        {
            get
            {
                SqlLanguage settingsValue = SettingsProvider.GetSetting(SqlLanguage);
                SqlLanguageDisplayPair? indentation = SqlLanguages.FirstOrDefault(x => x.Value == settingsValue);
                return indentation ?? SqlLanguageDisplayPair.Sql;
            }
            set
            {
                if (SqlLanguageMode != value)
                {
                    SettingsProvider.SetSetting(SqlLanguage, value.Value);
                    OnPropertyChanged();
                    QueueFormatting();
                }
            }
        }

        /// <summary>
        /// Get a list of supported Indentation
        /// </summary>
        internal IReadOnlyList<IndentationDisplayPair> Indentations = new ObservableCollection<IndentationDisplayPair> {
            Models.IndentationDisplayPair.TwoSpaces,
            Models.IndentationDisplayPair.FourSpaces
        };

        /// <summary>
        /// Get a list of supported SQL Languages
        /// </summary>
        internal IReadOnlyList<SqlLanguageDisplayPair> SqlLanguages = new ObservableCollection<SqlLanguageDisplayPair> {
            Models.SqlLanguageDisplayPair.Db2,
            Models.SqlLanguageDisplayPair.MariaDb,
            Models.SqlLanguageDisplayPair.MySql,
            Models.SqlLanguageDisplayPair.N1ql,
            Models.SqlLanguageDisplayPair.PlSql,
            Models.SqlLanguageDisplayPair.PostgreSql,
            Models.SqlLanguageDisplayPair.RedShift,
            Models.SqlLanguageDisplayPair.Spark,
            Models.SqlLanguageDisplayPair.Sql,
            Models.SqlLanguageDisplayPair.Tsql
        };

        /// <summary>
        /// Gets or sets the input text.
        /// </summary>
        internal string? InputValue
        {
            get => _inputValue;
            set
            {
                SetProperty(ref _inputValue, value);
                QueueFormatting();
            }
        }

        internal string? OutputValue
        {
            get => _outputValue;
            set => SetProperty(ref _outputValue, value);
        }

        internal ISettingsProvider SettingsProvider { get; }

        [ImportingConstructor]
        public SqlFormatterToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            SettingsProvider = settingsProvider;
            _marketingService = marketingService;
        }

        private void QueueFormatting()
        {
            _formattingQueue.Enqueue(InputValue ?? string.Empty);
            TreatQueueAsync().Forget();
        }

        private async Task TreatQueueAsync()
        {
            if (_formattingInProgress)
            {
                return;
            }

            _formattingInProgress = true;

            await TaskScheduler.Default;

            while (_formattingQueue.TryDequeue(out string? text))
            {
                int indentationSize = IndentationMode.Value switch
                {
                    Models.Indentation.TwoSpaces => 2,
                    Models.Indentation.FourSpaces => 4,
                    _ => throw new NotSupportedException(),
                };

                string? result
                    = SqlFormatterHelper.Format(
                        text,
                        SqlLanguageMode.Value,
                        new SqlFormatterOptions(
                            indentationSize,
                            uppercase: true,
                            linesBetweenQueries: 2));

                if (result != null)
                {
                    ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                    {
                        OutputValue = result;

                        if (!_toolSuccessfullyWorked)
                        {
                            _toolSuccessfullyWorked = true;
                            _marketingService.NotifyToolSuccessfullyWorked();
                        }
                    }).ForgetSafely();
                }
            }

            _formattingInProgress = false;
        }
    }
}
