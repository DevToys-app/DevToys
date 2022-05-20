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

        internal IReadOnlyList<string> TimeZoneDisplayNameCollection = TimestampToolHelper.ZoneInfo.DisplayNames;
        private readonly IReadOnlyDictionary<string, string> _timeZoneCollection = TimestampToolHelper.ZoneInfo.TimeZones;
        private TimeZoneInfo _currentTimeZone = TimeZoneInfo.Utc;
        private string _currentTimeZoneDisplayName = TimestampToolHelper.ZoneInfo.UtcDisplayName;
        private double _timestamp;
        private DateTimeOffset _utcDateTime;
        private DateTimeOffset _zoneOffsetDateTime;
        private long _minimumZoneOffsetTimestamp = -62135596800;
        private long _maximumZoneOffsetTimestamp = 253402300799;

        private string _dstInfoDSTMessage = "";
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
        internal string DSTInfoMessage
        {
            get => _dstInfoDSTMessage;
            set => SetProperty(ref _dstInfoDSTMessage, value);
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
        /// (e.g. "+09:00")
        /// </summary>
        internal string DSTInfoOffsetValue
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
            if (_currentTimeZone.IsAmbiguousTime(_zoneOffsetDateTime))
            {
                DSTInfoMessage = Strings.DSTAmbiguousTime;
            }
            else if (_currentTimeZone.IsDaylightSavingTime(_zoneOffsetDateTime))
            {
                DSTInfoMessage = Strings.DaylightSavingTime;
            }
            else if (_currentTimeZone.SupportsDaylightSavingTime)
            {
                DSTInfoMessage = Strings.SupportsDaylightSavingTime;
            }
            else
            {
                DSTInfoMessage = Strings.DisabledDaylightSavingTime;
            }
            DSTInfoOffsetValue = _zoneOffsetDateTime.ToString("zzz");
            DSTInfoLocalDateTime = _zoneOffsetDateTime.LocalDateTime.ToString("yyyy/MM/dd HH:mm:ss");
            DSTInfoUtcDateTime = _zoneOffsetDateTime.UtcDateTime.ToString("yyyy/MM/dd HH:mm:ss");
            DSTInfoUtcTicks = _zoneOffsetDateTime.UtcTicks.ToString();
        }

        /// <summary>
        /// Gets or sets the time zone name.
        /// This value is essentially the value of TimeZoneInfo.(zone).DisplayName (e.g. "(UTC) Coordinated Universal Time"),
        /// which is used to reverse lookup the time zone ID supported by the OS(e.g. TimeZoneInfo.Utc.Id -> "UTC").
        /// </summary>
        internal string CurrentTimeZoneDisplayName
        {
            get => _currentTimeZoneDisplayName;
            set
            {
                if (_timeZoneCollection.TryGetValue(value, out string timeZoneID))
                {
                    _currentTimeZoneDisplayName = value;
                    _currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneID);
                    _minimumZoneOffsetTimestamp = TimestampToolHelper.TimeZone.SafeMinValue(_currentTimeZone)
                                                                           .ToUnixTimeSeconds();
                    _maximumZoneOffsetTimestamp = TimestampToolHelper.TimeZone.SafeMaxValue(_currentTimeZone)
                                                                           .ToUnixTimeSeconds();
                    UpdateZoneOffsetTimestamp(_timestamp);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Unix time.
        /// -62135596800 (0001-01-01T00:00:00Z) to 253402300799 (9999-12-31T23:59:59Z) integer value.
        /// </summary>
        internal double Timestamp
        {
            get => _timestamp;
            set
            {
                IsInputInvalid = false;
                UpdateZoneOffsetTimestamp(value);
            }
        }

        /// <summary>
        /// Gets or sets the year value.
        /// </summary>
        internal int ZoneOffsetYear
        {
            get => _zoneOffsetDateTime.Year;
            set
            {
                if (value < 1) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(value, ZoneOffsetMonth, ZoneOffsetDay, ZoneOffsetHour, ZoneOffsetMinute, ZoneOffsetSecond))
                {
                    IsInputInvalid = true;
                    return;
                }
                Timestamp = _utcDateTime.AddYears(value - _zoneOffsetDateTime.Year).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Gets or sets the month value.
        /// </summary>
        internal int ZoneOffsetMonth
        {
            get => _zoneOffsetDateTime.Month;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(ZoneOffsetYear, value, ZoneOffsetDay, ZoneOffsetHour, ZoneOffsetMinute, ZoneOffsetSecond))
                {
                    IsInputInvalid = true;
                    return;
                }
                Timestamp = _utcDateTime.AddMonths(value - _zoneOffsetDateTime.Month).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Gets or sets the day value.
        /// </summary>
        internal int ZoneOffsetDay
        {
            get => _zoneOffsetDateTime.Day;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(ZoneOffsetYear, ZoneOffsetMonth, value, ZoneOffsetHour, ZoneOffsetMinute, ZoneOffsetSecond))
                {
                    IsInputInvalid = true;
                    return;
                }
                Timestamp = _utcDateTime.AddDays(value - _zoneOffsetDateTime.Day).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Gets or sets the hour value.
        /// </summary>
        internal int ZoneOffsetHour
        {
            get => _zoneOffsetDateTime.Hour;
            set
            {
                if (value < -1) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(ZoneOffsetYear, ZoneOffsetMonth, ZoneOffsetDay, value, ZoneOffsetMinute, ZoneOffsetSecond))
                {
                    IsInputInvalid = true;
                    return;
                }
                Timestamp = _utcDateTime.AddHours(value - _zoneOffsetDateTime.Hour).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Gets or sets the minute value.
        /// </summary>
        internal int ZoneOffsetMinute
        {
            get => _zoneOffsetDateTime.Minute;
            set
            {
                if (value < -1) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(ZoneOffsetYear, ZoneOffsetMonth, ZoneOffsetDay, ZoneOffsetHour, value, ZoneOffsetSecond))
                {
                    IsInputInvalid = true;
                    return;
                }
                Timestamp = _utcDateTime.AddMinutes(value - _zoneOffsetDateTime.Minute).ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// Gets or sets the second value.
        /// </summary>
        internal int ZoneOffsetSecond
        {
            get => _zoneOffsetDateTime.Second;
            set
            {
                if (value < -1) // empty = -2147483648
                {
                    return;
                }
                if (!IsValidDateTime(ZoneOffsetYear, ZoneOffsetMonth, ZoneOffsetDay, ZoneOffsetHour, ZoneOffsetMinute, value))
                {
                    IsInputInvalid = true;
                    return;
                }
                Timestamp = _utcDateTime.AddSeconds(value - _zoneOffsetDateTime.Second).ToUnixTimeSeconds();
            }
        }

        public TimestampToolViewModel()
        {
            PasteCommand = new RelayCommand(ExecutePasteCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand);
            NowCommand = new RelayCommand(ExecuteNowCommand);

            // Set to the current epoch time.
            Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
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
            Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        #endregion

        private DateTimeOffset TimestampToUtcDateTime(double value)
        {
            return DateTimeOffset.FromUnixTimeSeconds((long)value).UtcDateTime;
        }

        private void UpdateZoneOffsetTimestamp(double value)
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
                    value = _minimumZoneOffsetTimestamp;
                }
                else
                {
                    value = _maximumZoneOffsetTimestamp;
                }

            }
            _timestamp = value;
            _utcDateTime = TimestampToUtcDateTime(value);
            _zoneOffsetDateTime = TimeZoneInfo.ConvertTime(_utcDateTime, _currentTimeZone);
            DSTInfo();
            ResetZoneOffsetTimestamp();
            ResetZoneOffsetDateTime();
        }

        private bool IsValidTimestamp(long value)
        {
            if (value < _minimumZoneOffsetTimestamp || value > _maximumZoneOffsetTimestamp)
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

            DateTimeOffset calcDateTime = TimeZoneInfo.ConvertTime(_zoneOffsetDateTime, TimeZoneInfo.Utc);
            calcDateTime = calcDateTime.AddYears(1970 - calcDateTime.Year);
            try
            {
                calcDateTime = calcDateTime.AddMonths(Month - _zoneOffsetDateTime.Month);
                calcDateTime = calcDateTime.AddDays(Day - _zoneOffsetDateTime.Day);
                calcDateTime = calcDateTime.AddHours(Hour - _zoneOffsetDateTime.Hour);
                calcDateTime = calcDateTime.AddMinutes(Minute - _zoneOffsetDateTime.Minute);
                calcDateTime = calcDateTime.AddSeconds(Second - _zoneOffsetDateTime.Second);
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

        private void ResetZoneOffsetDateTime()
        {
            OnPropertyChanged(nameof(ZoneOffsetYear));
            OnPropertyChanged(nameof(ZoneOffsetMonth));
            OnPropertyChanged(nameof(ZoneOffsetDay));
            OnPropertyChanged(nameof(ZoneOffsetHour));
            OnPropertyChanged(nameof(ZoneOffsetMinute));
            OnPropertyChanged(nameof(ZoneOffsetSecond));
        }

        private void ResetZoneOffsetTimestamp()
        {
            OnPropertyChanged(nameof(Timestamp));
        }

    }
}
