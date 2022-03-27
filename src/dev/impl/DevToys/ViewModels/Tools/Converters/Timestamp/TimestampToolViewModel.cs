#nullable enable

using System;
using System.Composition;
using DevToys.Api.Tools;
using DevToys.Views.Tools.Timestamp;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;

namespace DevToys.ViewModels.Tools.Timestamp
{
    [Export(typeof(TimestampToolViewModel))]
    public sealed class TimestampToolViewModel : ObservableRecipient, IToolViewModel
    {
        private bool _isInputInvalid;
        private double _timestamp;
        private DateTime _utcDateTime;
        private Lazy<DateTime> _localDateTime = null!;

        public Type View => typeof(TimestampToolPage);

        internal TimestampStrings Strings => LanguageManager.Instance.Timestamp;

        internal bool IsInputInvalid
        {
            get => _isInputInvalid;
            set => SetProperty(ref _isInputInvalid, value);
        }

        internal double Timestamp
        {
            get => _timestamp;
            set
            {
                if (double.IsNaN(value))
                {
                    _timestamp = 0;
                }
                else
                {
                    _timestamp = value;
                }

                ResetUtcDateTime();
                ResetLocalDateTime();
            }
        }

        internal int UtcYear
        {
            get => _utcDateTime.Year;
            set
            {
                if (value < 1) // empty = -2147483648
                {
                    return;
                }
                ResetUtcLeapYear(value);
                _utcDateTime = new DateTime(value, UtcMonth, UtcDay, UtcHour, UtcMinute, UtcSecond, DateTimeKind.Utc);
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int UtcMonth
        {
            get => _utcDateTime.Month;
            set
            {
                if (value < 1) // empty = -2147483648
                {
                    return;
                }
                ResetUtcLeapYear(UtcYear);
                _utcDateTime = new DateTime(UtcYear, value, UtcDay, UtcHour, UtcMinute, UtcSecond, DateTimeKind.Utc);
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int UtcDay
        {
            get => _utcDateTime.Day;
            set
            {
                if (value < 1) // empty = -2147483648
                {
                    return;
                }
                if (UtcMonth == 2 && value > 28)
                {
                    if (value != 29 || !DateTime.IsLeapYear(UtcYear))
                    {
                        return;
                    }
                }
                _utcDateTime = new DateTime(UtcYear, UtcMonth, value, UtcHour, UtcMinute, UtcSecond, DateTimeKind.Utc);
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int UtcHour
        {
            get => _utcDateTime.Hour;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                _utcDateTime = new DateTime(UtcYear, UtcMonth, UtcDay, value, UtcMinute, UtcSecond, DateTimeKind.Utc);
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int UtcMinute
        {
            get => _utcDateTime.Minute;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                _utcDateTime = new DateTime(UtcYear, UtcMonth, UtcDay, UtcHour, value, UtcSecond, DateTimeKind.Utc);
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int UtcSecond
        {
            get => _utcDateTime.Second;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                _utcDateTime = new DateTime(UtcYear, UtcMonth, UtcDay, UtcHour, UtcMinute, value, DateTimeKind.Utc);
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int LocalYear
        {
            get => _localDateTime.Value.Year;
            set
            {
                if (value < 1) // empty = -2147483648
                {
                    return;
                }
                ResetLocalLeapYear(value);
                var localDateTime = new DateTime(value, LocalMonth, LocalDay, LocalHour, LocalMinute, LocalSecond, DateTimeKind.Local);
                _timestamp = new DateTimeOffset(localDateTime.ToUniversalTime()).ToUnixTimeSeconds();
                ResetUtcDateTime();
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int LocalMonth
        {
            get => _localDateTime.Value.Month;
            set
            {
                if (value < 1) // empty = -2147483648
                {
                    return;
                }
                ResetLocalLeapYear(LocalYear);
                var localDateTime = new DateTime(LocalYear, value, LocalDay, LocalHour, LocalMinute, LocalSecond, DateTimeKind.Local);
                _timestamp = new DateTimeOffset(localDateTime.ToUniversalTime()).ToUnixTimeSeconds();
                ResetUtcDateTime();
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int LocalDay
        {
            get => _localDateTime.Value.Day;
            set
            {
                if (value < 1) // empty = -2147483648
                {
                    return;
                }
                if (LocalMonth == 2 && value > 28)
                {
                    if (value != 29 || !DateTime.IsLeapYear(LocalYear))
                    {
                        return;
                    }
                }
                var localDateTime = new DateTime(LocalYear, LocalMonth, value, LocalHour, LocalMinute, LocalSecond, DateTimeKind.Local);
                _timestamp = new DateTimeOffset(localDateTime.ToUniversalTime()).ToUnixTimeSeconds();
                ResetUtcDateTime();
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int LocalHour
        {
            get => _localDateTime.Value.Hour;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                var localDateTime = new DateTime(LocalYear, LocalMonth, LocalDay, value, LocalMinute, LocalSecond, DateTimeKind.Local);
                _timestamp = new DateTimeOffset(localDateTime.ToUniversalTime()).ToUnixTimeSeconds();
                ResetUtcDateTime();
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int LocalMinute
        {
            get => _localDateTime.Value.Minute;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                var localDateTime = new DateTime(LocalYear, LocalMonth, LocalDay, LocalHour, value, LocalSecond, DateTimeKind.Local);
                _timestamp = new DateTimeOffset(localDateTime.ToUniversalTime()).ToUnixTimeSeconds();
                ResetUtcDateTime();
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        internal int LocalSecond
        {
            get => _localDateTime.Value.Second;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                var localDateTime = new DateTime(LocalYear, LocalMonth, LocalDay, LocalHour, LocalMinute, value, DateTimeKind.Local);
                _timestamp = new DateTimeOffset(localDateTime.ToUniversalTime()).ToUnixTimeSeconds();
                ResetUtcDateTime();
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        public TimestampToolViewModel()
        {
            PasteCommand = new RelayCommand(ExecutePasteCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand);

            // Set to the current epoch time.
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

                string? text = await dataPackageView.GetTextAsync();

                if (long.TryParse(text, out long value))
                {
                    Timestamp = value;
                }
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
                data.SetText(Timestamp.ToString());

                Clipboard.SetContentWithOptions(data, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
                Clipboard.Flush(); // This method allows the content to remain available after the application shuts down.
            }
            catch (Exception ex)
            {
                Core.Logger.LogFault("Failed to copy from numeric box", ex);
            }
        }

        #endregion

        private void ResetUtcDateTime()
        {
            try
            {
                _utcDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((long)Timestamp);
                IsInputInvalid = false;
            }
            catch
            {
                IsInputInvalid = true;
            }

            OnPropertyChanged(nameof(UtcYear));
            OnPropertyChanged(nameof(UtcMonth));
            OnPropertyChanged(nameof(UtcDay));
            OnPropertyChanged(nameof(UtcHour));
            OnPropertyChanged(nameof(UtcMinute));
            OnPropertyChanged(nameof(UtcSecond));
        }

        private void ResetLocalDateTime()
        {
            _localDateTime = new Lazy<DateTime>(() => _utcDateTime.ToLocalTime());
            OnPropertyChanged(nameof(LocalYear));
            OnPropertyChanged(nameof(LocalMonth));
            OnPropertyChanged(nameof(LocalDay));
            OnPropertyChanged(nameof(LocalHour));
            OnPropertyChanged(nameof(LocalMinute));
            OnPropertyChanged(nameof(LocalSecond));
        }

        private void ResetTimestamp()
        {
            _timestamp = new DateTimeOffset(_utcDateTime).ToUnixTimeSeconds();
            OnPropertyChanged(nameof(Timestamp));
        }

        private void ResetUtcLeapYear(int setValue) {
            if (UtcDay > 28 && UtcMonth == 2 && !DateTime.IsLeapYear(setValue))
            {
                UtcDay = 28;
            }
        }

        private void ResetLocalLeapYear(int setValue)
        {
            if (LocalDay > 28 && LocalMonth == 2 && !DateTime.IsLeapYear(setValue))
            {
                LocalDay = 28;
            }
        }

    }
}
