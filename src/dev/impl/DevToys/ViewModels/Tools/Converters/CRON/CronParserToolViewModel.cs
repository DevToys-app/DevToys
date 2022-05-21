#nullable enable

using System;
using Cronos;
using System.Composition;
using DevToys.Api.Tools;
using DevToys.Views.Tools.CronParser;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.Generic;
using DevToys.Api.Core.Settings;

namespace DevToys.ViewModels.Tools.CronParser
{
    [Export(typeof(CronParserToolViewModel))]
    public sealed class CronParserToolViewModel : ObservableRecipient, IToolViewModel
    {
        private readonly ISettingsProvider _settingsProvider;

        private bool _isInputInvalid;
        private string _cronExpression;
        private string _errorMessage;
        private bool _setPropertyInProgress;
        private string? _outputValue;

        private const string DefaultCronWithSeconds = "* * * * * *";
        private const string DefaultCronWithoutSeconds = "* * * * *";
        private const string DefaultTimestampFormat = "yyyy-MM-dd ddd HH:mm:ss";
        private const string DefaultOutputLimit = "5";

        /// <summary>
        /// Whether the tool should encode or decode Base64.
        /// </summary>
        private static readonly SettingDefinition<bool> IncludeSeconds
            = new(
                name: $"{nameof(CronParserToolViewModel)}.{nameof(IncludeSeconds)}",
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// Whether the tool should encode or decode Base64.
        /// </summary>
        private static readonly SettingDefinition<string> OutputDateTime
            = new(
                name: $"{nameof(CronParserToolViewModel)}.{nameof(OutputDateTime)}",
                isRoaming: true,
                defaultValue: DefaultTimestampFormat);

        /// <summary>
        /// Whether the tool should encode/decode in Unicode or ASCII.
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

        internal string UserCronExpression
        {
            get => _cronExpression;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    SetProperty(ref _cronExpression, string.Empty);
                }
                else
                {
                    SetProperty(ref _cronExpression, value);
                }

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

        private void ParseCronExpression()
        {
            IList<string> dateTimeOffsets = new List<string>();

            IsInputInvalid = false;

            try
            {
                if (string.IsNullOrEmpty(UserCronExpression))
                {
                    return;
                }

                var expression = CronExpression.Parse(UserCronExpression, IncludeSecondsMode ? CronFormat.IncludeSeconds : CronFormat.Standard);

                DateTimeOffset? nextOccurence = expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local, true);

                if (nextOccurence == null)
                {
                    return;
                }

                dateTimeOffsets.Add(nextOccurence.Value.ToString(DefaultTimestampFormat));

                for (int i = 0; i <= int.Parse(OutputLimitMode); i++)
                {
                    nextOccurence = expression.GetNextOccurrence(nextOccurence.Value, TimeZoneInfo.Local, false);

                    if (nextOccurence == null)
                    {
                        return;
                    }

                    dateTimeOffsets.Add(nextOccurence.Value.ToString(DefaultTimestampFormat));
                }

                OutputValue = String.Join(Environment.NewLine, dateTimeOffsets);
            }
            catch (CronFormatException)
            {
                IsInputInvalid = true;
            }
        }

        [ImportingConstructor]
        public CronParserToolViewModel(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;

            PasteCommand = new RelayCommand(ExecutePasteCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand);

            _errorMessage = String.Empty;
            _cronExpression = String.Empty;

            IsInputInvalid = false;
            
            if (IncludeSecondsMode)
            {
                UserCronExpression = DefaultCronWithSeconds;
            }
            else
            {
                UserCronExpression = DefaultCronWithoutSeconds;
            }
        }

        #region PasteCommand

        internal IRelayCommand PasteCommand { get; }

        private async void ExecutePasteCommand()
        {
            try
            {
                DataPackageView? dataPackageView = Clipboard.GetContent();
                if (!dataPackageView.Contains(StandardDataFormats.Text))
                {
                    return;
                }

                string text = await dataPackageView.GetTextAsync();

                UserCronExpression = text;
                
            }
            catch (Exception ex)
            {
                Core.Logger.LogFault("Failed to paste in numeric box", ex);
            }
        }

        #endregion

        #region CopyCommand

        internal IRelayCommand CopyCommand { get; }

        private void ExecuteCopyCommand()
        {
            try
            {
                var data = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                data.SetText(UserCronExpression);

                Clipboard.SetContentWithOptions(data, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
                Clipboard.Flush();
            }
            catch (Exception ex)
            {
                Core.Logger.LogFault("Failed to copy from numeric box", ex);
            }
        }

        #endregion               
    }
}
