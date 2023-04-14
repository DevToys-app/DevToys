#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using Cronos;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Shared.Core;
using DevToys.Views.Tools.CronParser;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;

namespace DevToys.ViewModels.Tools.CronParser
{
    [Export(typeof(CronParserToolViewModel))]
    public sealed class CronParserToolViewModel : ObservableRecipient, IToolViewModel
    {
        private readonly ISettingsProvider _settingsProvider;

        private bool _isInputInvalid;
        private bool _isOutputFormatInvalid;
        private string _cronExpression;
        private bool _setPropertyInProgress;
        private string? _outputValue;

        private const string DefaultCronWithSeconds = "* * * * * *";
        private const string DefaultCronWithoutSeconds = "* * * * *";
        private const string DefaultTimestampFormat = "yyyy-MM-dd ddd HH:mm:ss";
        private const string DefaultOutputLimit = "5";

        /// <summary>
        /// Whether the tool should include seconds in cron definition
        /// </summary>
        private static readonly SettingDefinition<bool> IncludeSeconds
            = new(
                name: $"{nameof(CronParserToolViewModel)}.{nameof(IncludeSeconds)}",
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// Whether datetime format tool should use for output
        /// </summary>
        private static readonly SettingDefinition<string> OutputDateTime
            = new(
                name: $"{nameof(CronParserToolViewModel)}.{nameof(OutputDateTime)}",
                isRoaming: true,
                defaultValue: DefaultTimestampFormat);

        /// <summary>
        /// How many lines of next occurencies the tool should generate
        /// </summary>
        private static readonly SettingDefinition<string> OutputLimit
            = new(
                name: $"{nameof(CronParserToolViewModel)}.{nameof(OutputLimit)}",
                isRoaming: true,
                defaultValue: DefaultOutputLimit);

        public Type View => typeof(CronParserToolPage);

        internal CRONParserStrings Strings => LanguageManager.Instance.CRONParser;

        internal bool IsInputInvalid
        {
            get => _isInputInvalid;
            set => SetProperty(ref _isInputInvalid, value);
        }

        internal bool IsOutputFormatInvalid
        {
            get => _isOutputFormatInvalid;
            set => SetProperty(ref _isOutputFormatInvalid, value);
        }

        /// <summary>
        /// Gets or sets Cron expression
        /// </summary>
        internal string UserCronExpression
        {
            get => _cronExpression;
            set
            {
                SetProperty(ref _cronExpression, value);
                ParseCronExpression();
            }
        }

        /// <summary>
        /// Gets or sets whatever Cron should include settings
        /// </summary>
        internal bool IncludeSecondsMode
        {
            get => _settingsProvider.GetSetting(IncludeSeconds);
            set
            {
                if (!_setPropertyInProgress)
                {
                    _setPropertyInProgress = true;

                    if (_settingsProvider.GetSetting(IncludeSeconds) != value)
                    {
                        _settingsProvider.SetSetting(IncludeSeconds, value);
                        OnPropertyChanged();
                        ParseCronExpression();
                    }

                    _setPropertyInProgress = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets number of output lines.
        /// </summary>
        internal string OutputLimitMode
        {
            get => _settingsProvider.GetSetting(OutputLimit);
            set
            {
                if (!_setPropertyInProgress)
                {
                    _setPropertyInProgress = true;

                    if (_settingsProvider.GetSetting(OutputLimit) != value)
                    {
                        _settingsProvider.SetSetting(OutputLimit, value);
                        OnPropertyChanged();
                        ParseCronExpression();
                    }

                    _setPropertyInProgress = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the output format
        /// </summary>
        internal string OutputDateTimeFormat
        {
            get => _settingsProvider.GetSetting(OutputDateTime);
            set
            {
                if (!_setPropertyInProgress)
                {
                    _setPropertyInProgress = true;

                    if (_settingsProvider.GetSetting(OutputDateTime) != value)
                    {
                        _settingsProvider.SetSetting(OutputDateTime, value);
                        OnPropertyChanged();
                        ParseCronExpression();
                    }

                    _setPropertyInProgress = false;
                }
            }
        }

        internal string? OutputValue
        {
            get => _outputValue;
            private set => SetProperty(ref _outputValue, value);
        }


        [ImportingConstructor]
        public CronParserToolViewModel(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;

            _cronExpression = string.Empty;

            IsInputInvalid = false;
            IsOutputFormatInvalid = false;

            if (IncludeSecondsMode)
            {
                UserCronExpression = DefaultCronWithSeconds;
            }
            else
            {
                UserCronExpression = DefaultCronWithoutSeconds;
            }
        }

        private void ParseCronExpression()
        {
            IsInputInvalid = false;
            IsOutputFormatInvalid = false;

            if (!ValidateDateTimeFormat(OutputDateTimeFormat))
            {
                IsOutputFormatInvalid = true;

                return;
            }

            var output = new List<string>();

            try
            {
                if (string.IsNullOrEmpty(UserCronExpression))
                {
                    return;
                }

                var expression = CronExpression.Parse(UserCronExpression, IncludeSecondsMode ? CronFormat.IncludeSeconds : CronFormat.Standard);

                Assumes.NotNull(expression, nameof(expression));

                DateTimeOffset? nextOccurence = expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local, true);

                if (nextOccurence == null)
                {
                    return;
                }

                output.Add(nextOccurence.Value.ToString(OutputDateTimeFormat));

                for (int i = 0; i <= int.Parse(OutputLimitMode); i++)
                {
                    nextOccurence = expression.GetNextOccurrence(nextOccurence.Value, TimeZoneInfo.Local, false);

                    if (nextOccurence == null)
                    {
                        break;
                    }

                    output.Add(nextOccurence.Value.ToString(OutputDateTimeFormat));
                }

                OutputValue = string.Join(Environment.NewLine, output);
            }
            catch (CronFormatException)
            {
                IsInputInvalid = true;
            }
        }

        private bool ValidateDateTimeFormat(string dateFormat)
        {
            try
            {
                string s = DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture);
                DateTime.Parse(s, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
