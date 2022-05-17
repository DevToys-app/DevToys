#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using DevToys.Api.Tools;
using DevToys.Helpers;
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

        internal List<string> TimeZoneDisplayNameCollection = TimestampToolHelper.ZoneInfo.DisplayNames;
        private readonly Dictionary<string, string> _timeZoneCollection = TimestampToolHelper.ZoneInfo.TimeZones;
        private TimeZoneInfo _currentTimeZone = TimeZoneInfo.Utc;
        private string _currentTimeZoneDisplayName = TimestampToolHelper.ZoneInfo.UtcDisplayName;
        private double _currentTimestamp;
        private DateTimeOffset _currentUtcDateTime;
        private DateTimeOffset _currentDateTime;
        private long _minimumCurrentTimestamp = -62135596800;
        private long _maximumCurrentTimestamp = 253402300799;

        private string _dstInfoText = "";
        private string _dstInfoOffset = "";
        private string _dstInfoLocalDateTime = "";
        private string _dstInfoUtcDateTime = "";
        private string _dstInfoUtcTicks = "";

        public Type View => typeof(TimestampToolPage);

        internal TimestampStrings Strings => LanguageManager.Instance.Timestamp;

        /// <summary>
        /// Gets or sets true if the DateTimeOffset structure may exceed the settable range.
        /// </summary>
        internal bool IsInputInvalid
        {
            get => _isInputInvalid;
            set => SetProperty(ref _isInputInvalid, value);
        }

        /// <summary>
        /// Daylight saving time display in DSTInfo block.
        /// Gets or sets text that displays whether it is 
        /// daylight saving time support, in daylight saving time, or ambiguous time 
        /// for a given time zone.
        /// </summary>
        internal string DSTInfoText
        {
            get => _dstInfoText;
            set => SetProperty(ref _dstInfoText, value);
        }

        /// <summary>
        /// Local date and time value in DSTInfo block.
        /// Get or set the date and time converted to the time zone on the PC.
        /// (e.g. "2026/05/13 09:12:34")
        /// </summary>
        internal string DSTInfoLocalDateTime
        {
            get => _dstInfoLocalDateTime;
            set => SetProperty(ref _dstInfoLocalDateTime, value);
        }

        /// <summary>
        /// Time zone offset value for DSTInfo block.
        /// Gets or sets the offset value that changes with the date and time in the specified time zone.
        /// (e.g. "+09:00" )
        /// </summary>
        internal string DSTInfoOffset
        {
            get => _dstInfoOffset;
            set => SetProperty(ref _dstInfoOffset, value);
        }

        /// <summary>
        /// UTC date and time value in DSTInfo block.
        /// Gets or sets the string of the specified date and time converted to UTC(+00:00).
        /// (e.g. "2026/05/13 01:23:45")
        /// </summary>
        internal string DSTInfoUtcDateTime
        {
            get => _dstInfoUtcDateTime;
            set => SetProperty(ref _dstInfoUtcDateTime, value);
        }

        /// <summary>
        /// UTCTicks value in DSTInfo block.
        /// Gets or sets the string of the specified date and time converted to UTCTicks.
        /// </summary>
        internal string DSTInfoUtcTicks
        {
            get => _dstInfoUtcTicks;
            set => SetProperty(ref _dstInfoUtcTicks, value);
        }

        private void DSTInfo()
        {
            if (_currentTimeZone.IsAmbiguousTime(_currentDateTime))
            {
                DSTInfoText = Strings.DSTAmbiguousTime;
            }
            else if (_currentTimeZone.IsDaylightSavingTime(_currentDateTime))
            {
                DSTInfoText = Strings.DaylightSavingTime;
            }
            else if (_currentTimeZone.SupportsDaylightSavingTime)
            {
                DSTInfoText = Strings.SupportsDaylightSavingTime;
            }
            else
            {
                DSTInfoText = Strings.DisabledDaylightSavingTime;
            }
            DSTInfoOffset = _currentDateTime.ToString("zzz");
            DSTInfoLocalDateTime = _currentDateTime.LocalDateTime.ToString("yyyy/MM/dd HH:mm:ss");
            DSTInfoUtcDateTime = _currentDateTime.UtcDateTime.ToString("yyyy/MM/dd HH:mm:ss");
            DSTInfoUtcTicks = _currentDateTime.UtcTicks.ToString();
        }

        /// <summary>
        /// Gets or sets the time zone name.
        /// This value is essentially the value of TimeZoneInfo.(zone).DisplayName,
        /// which is used to reverse lookup the time zone ID supported by the OS(e.g. TimeZoneInfo.Utc.Id).
        /// </summary>
        internal string CurrentTimeZoneDisplayName
        {
            get => _currentTimeZoneDisplayName;
            set
            {
                if (_timeZoneCollection.ContainsKey(value))
                {
                    _currentTimeZoneDisplayName = value;
                    _currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneCollection[value]);
                    _minimumCurrentTimestamp = TimestampToolHelper.TimeZone.SafeMinValue(_currentTimeZone)
                                                                           .ToUnixTimeSeconds();
                    _maximumCurrentTimestamp = TimestampToolHelper.TimeZone.SafeMaxValue(_currentTimeZone)
                                                                           .ToUnixTimeSeconds();
                    UpdateCurrentTimestamp(_currentTimestamp);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Unix time.
        /// -62135596800 to 253402300799 integer value.
        /// </summary>
        internal double CurrentTimestamp
        {
            get => _currentTimestamp;
            set
            {
                IsInputInvalid = false;
                UpdateCurrentTimestamp(value);
            }
        }

        /// <summary>
        /// Gets or sets the year value.
        /// </summary>
        internal int CurrentYear
        {
            get => _currentDateTime.Year;
            set
            {
                if (value < 1) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(value, CurrentMonth, CurrentDay, CurrentHour, CurrentMinute, CurrentSecond))
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.AddYears(value - _currentDateTime.Year).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Gets or sets the month value.
        /// </summary>
        internal int CurrentMonth
        {
            get => _currentDateTime.Month;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(CurrentYear, value, CurrentDay, CurrentHour, CurrentMinute, CurrentSecond))
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.AddMonths(value - _currentDateTime.Month).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Gets or sets the day value.
        /// </summary>
        internal int CurrentDay
        {
            get => _currentDateTime.Day;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(CurrentYear, CurrentMonth, value, CurrentHour, CurrentMinute, CurrentSecond))
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.AddDays(value - _currentDateTime.Day).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Gets or sets the hour value.
        /// </summary>
        internal int CurrentHour
        {
            get => _currentDateTime.Hour;
            set
            {
                if (value < -1) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(CurrentYear, CurrentMonth, CurrentDay, value, CurrentMinute, CurrentSecond))
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.AddHours(value - _currentDateTime.Hour).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Gets or sets the minute value.
        /// </summary>
        internal int CurrentMinute
        {
            get => _currentDateTime.Minute;
            set
            {
                if (value < -1) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(CurrentYear, CurrentMonth, CurrentDay, CurrentHour, value, CurrentSecond))
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.AddMinutes(value - _currentDateTime.Minute).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Gets or sets the second value.
        /// </summary>
        internal int CurrentSecond
        {
            get => _currentDateTime.Second;
            set
            {
                if (value < -1) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(CurrentYear, CurrentMonth, CurrentDay, CurrentHour, CurrentMinute, value))
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.AddSeconds(value - _currentDateTime.Second).ToUnixTimeSeconds();
            }
        }

        public TimestampToolViewModel()
        {
            PasteCommand = new RelayCommand(ExecutePasteCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand);
            NowCommand = new RelayCommand(ExecuteNowCommand);

            // Set to the current epoch time.
            CurrentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            CurrentTimeZoneDisplayName = TimestampToolHelper.ZoneInfo.LocalDisplayName;
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
                    CurrentTimestamp = value;
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
                data.SetText(CurrentTimestamp.ToString());

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
            CurrentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        #endregion

        private DateTimeOffset TimestampToUtcDateTime(double value)
        {
            return DateTimeOffset.FromUnixTimeSeconds((long)value).UtcDateTime;
        }

        private void UpdateCurrentTimestamp(double value)
        {
            if (double.IsNaN(value))
            {
                value = 0;
            }
            if (!IsValidTimestamp((long)value))
            {
                IsInputInvalid = true;
                if (value < 0)
                {
                    value = _minimumCurrentTimestamp;
                }
                else
                {
                    value = _maximumCurrentTimestamp;
                }

            }
            _currentTimestamp = value;
            _currentUtcDateTime = TimestampToUtcDateTime(value);
            _currentDateTime = TimeZoneInfo.ConvertTime(_currentUtcDateTime, _currentTimeZone);
            DSTInfo();
            ResetCurrentTimestamp();
            ResetCurrentDateTime();
        }

        private bool IsValidTimestamp(long value)
        {
            if (value < _minimumCurrentTimestamp || value > _maximumCurrentTimestamp)
            {
                return false;
            }
            return true;
        }

        private bool IsValidDateTime(int Year, int Month, int Day, int Hour, int Minute, int Second)
        {
            if (Year is < 1 or > 9999)
            {
                return false;
            }

            DateTimeOffset calcDateTime = TimeZoneInfo.ConvertTime(_currentDateTime, TimeZoneInfo.Utc);
            calcDateTime = calcDateTime.AddYears(1970 - calcDateTime.Year);
            try
            {
                calcDateTime = calcDateTime.AddMonths(Month - _currentDateTime.Month);
                calcDateTime = calcDateTime.AddDays(Day - _currentDateTime.Day);
                calcDateTime = calcDateTime.AddHours(Hour - _currentDateTime.Hour);
                calcDateTime = calcDateTime.AddMinutes(Minute - _currentDateTime.Minute);
                calcDateTime = calcDateTime.AddSeconds(Second - _currentDateTime.Second);
            }
            catch
            {
                return false;
            }
            calcDateTime = TimeZoneInfo.ConvertTime(calcDateTime, _currentTimeZone);
            TimeSpan offset = calcDateTime.Offset;

            if (Year + calcDateTime.Year - 1970 is > 1 and < 9999)
            {
                // 0002 ... 9998
                return true;
            }

            if (offset.TotalSeconds >= 0)
            {
                //UTC+
                //example +01:00 Valid -> 0001/01/01 01:00:00 - 9999/12/31 23:59:59
                if (Year == 1)
                {
                    if (calcDateTime.UtcDateTime.Year < 1970)
                    {
                        return false;
                    }
                }
                else //Year == 9999
                {
                    if (calcDateTime.Year > 1970)
                    {
                        return false;
                    }
                }
            }
            else
            {
                //UTC-
                //example -01:00 Valid -> 0001/01/01 00:00:00 - 9999/12/31 22:59:59
                if (Year == 1)
                {
                    if (calcDateTime.Year < 1970)
                    {
                        return false;
                    }
                }
                else //Year == 9999
                {
                    if (calcDateTime.UtcDateTime.Year > 1970)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void ResetCurrentDateTime()
        {
            OnPropertyChanged(nameof(CurrentYear));
            OnPropertyChanged(nameof(CurrentMonth));
            OnPropertyChanged(nameof(CurrentDay));
            OnPropertyChanged(nameof(CurrentHour));
            OnPropertyChanged(nameof(CurrentMinute));
            OnPropertyChanged(nameof(CurrentSecond));
        }

        private void ResetCurrentTimestamp()
        {
            OnPropertyChanged(nameof(CurrentTimestamp));
        }

    }
}
