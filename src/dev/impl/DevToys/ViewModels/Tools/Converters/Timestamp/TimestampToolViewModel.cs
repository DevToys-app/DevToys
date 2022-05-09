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

                if (UtcDay > DateTime.DaysInMonth(value, UtcMonth))
                {
                    return;
                }
                if (!UpdateUtcDateTime(value, UtcMonth, UtcDay, UtcHour, UtcMinute, UtcSecond))
                {
                    return;
                }
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

                if (UtcDay > DateTime.DaysInMonth(UtcYear, value))
                {
                    return;
                }

                if (!UpdateUtcDateTime(UtcYear, value, UtcDay, UtcHour, UtcMinute, UtcSecond))
                {
                    return;
                }
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

                if (value > DateTime.DaysInMonth(UtcYear, UtcMonth))
                {
                    return;
                }
                if (!UpdateUtcDateTime(UtcYear, UtcMonth, value, UtcHour, UtcMinute, UtcSecond))
                {
                    return;
                }
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
                if(!UpdateUtcDateTime(UtcYear, UtcMonth, UtcDay, value, UtcMinute, UtcSecond))
                {
                    return;
                }
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
                if (!UpdateUtcDateTime(UtcYear, UtcMonth, UtcDay, UtcHour, value, UtcSecond))
                {
                    return;
                }
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
                if (!UpdateUtcDateTime(UtcYear, UtcMonth, UtcDay, UtcHour, UtcMinute, value))
                {
                    return;
                }
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

                if (LocalDay > DateTime.DaysInMonth(value, LocalMonth))
                {
                    return;
                }
                if (!UpdateLocalDateTime(value, LocalMonth, LocalDay, LocalHour, LocalMinute, LocalSecond))
                {
                    return;
                }
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

                if (LocalDay > DateTime.DaysInMonth(LocalYear, value))
                {
                    return;
                }
                if (!UpdateLocalDateTime(LocalYear, value, LocalDay, LocalHour, LocalMinute, LocalSecond))
                {
                    return;
                }
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

                if (value > DateTime.DaysInMonth(LocalYear, LocalMonth))
                {
                    return;
                }
                if (!UpdateLocalDateTime(LocalYear, LocalMonth, value, LocalHour, LocalMinute, LocalSecond))
                {
                    return;
                }
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
                if (!UpdateLocalDateTime(LocalYear, LocalMonth, LocalDay, value, LocalMinute, LocalSecond))
                {
                    return;
                }
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
                if (!UpdateLocalDateTime(LocalYear, LocalMonth, LocalDay, LocalHour, value, LocalSecond))
                {
                    return;
                }
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
                if(!UpdateLocalDateTime(LocalYear, LocalMonth, LocalDay, LocalHour, LocalMinute, value))
                {
                    return;
                }
                ResetUtcDateTime();
                ResetLocalDateTime();
                ResetTimestamp();
            }
        }

        public TimestampToolViewModel()
        {
            PasteCommand = new RelayCommand(ExecutePasteCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand);
            NowCommand = new RelayCommand(ExecuteNowCommand);

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

        #region NowCommand
        internal IRelayCommand NowCommand { get; }

        private void ExecuteNowCommand()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            ResetUtcDateTime();
            ResetLocalDateTime();
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

        private bool UpdateUtcDateTime(int UtcYear, int UtcMonth, int UtcDay, int UtcHour, int UtcMinute, int UtcSecond)
        {
            try
            {
                var utcDateTime = new DateTime(UtcYear, UtcMonth, UtcDay, UtcHour, UtcMinute, UtcSecond, DateTimeKind.Utc);
                long timestamp = new DateTimeOffset(utcDateTime).ToUnixTimeSeconds();
                // TODO: UTC 9999/12/31 23:59:59 + LocalTime(+0030...) -> invalid LocalTime
                // TODO: UTC 0001/01/01 00:00:00 + LocalTime(-0030...) -> invalid LocalTime
                /*
                if (timestamp is < (-62135596800) or > 253402300799)
                {
                    IsInputInvalid = true;
                    return false;
                }
                 */
                DateTime localDateTime = utcDateTime.ToLocalTime();
                _utcDateTime = utcDateTime;
                IsInputInvalid = false;
            }
            catch
            {
                IsInputInvalid = true;
                return false;
            }
            return true;
        }
 
        private bool UpdateLocalDateTime(int LocalYear, int LocalMonth, int LocalDay, int LocalHour, int LocalMinute, int LocalSecond)
        {
            try
            {
                var localDateTime = new DateTime(LocalYear, LocalMonth, LocalDay, LocalHour, LocalMinute, LocalSecond, DateTimeKind.Local);
                long timestamp = new DateTimeOffset(localDateTime.ToUniversalTime()).ToUnixTimeSeconds();
                // TODO: UTC 9999/12/31 23:59:59 + LocalTime(+0030...) -> invalid LocalTime
                // TODO: UTC 0001/01/01 00:00:00 + LocalTime(-0030...) -> invalid LocalTime
                /*
                if (timestamp is < (-62135596800) or > 253402300799)
                {
                    IsInputInvalid = true;
                    return false;
                }
                 */
                DateTime utcDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
                _timestamp = timestamp;
                IsInputInvalid = false;
            }
            catch
            {
                IsInputInvalid = true;
                return false;
            }
            return true;
        }
    }
}
