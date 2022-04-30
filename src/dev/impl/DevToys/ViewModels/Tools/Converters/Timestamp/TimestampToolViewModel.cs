#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly long _minimumUtcTimestamp = -62135596800;
        private readonly long _maximumUtcTimestamp = 253402300799;

        internal List<string> TimeZoneDisplayNameCollection = new();
        private readonly Dictionary<string, string> _timeZoneCollection = new();
        private readonly ReadOnlyCollection<TimeZoneInfo> _systemTimeZone = TimeZoneInfo.GetSystemTimeZones();
        private TimeZoneInfo _currentTimeZone = TimeZoneInfo.Utc;
        private string _currentTimeZoneDisplayName = TimeZoneInfo.Utc.DisplayName;
        private double _currentTimestamp;
        private DateTimeOffset _currentUtcDateTime;
        private DateTimeOffset _currentDateTime;
        private long _minimumCurrentTimestamp = -62135596800;
        private long _maximumCurrentTimestamp = 253402300799;

        private bool _isSupportsDST;
        private bool _isDisabledDST;
        private bool _isDaylightSavingTime;
        private bool _isDSTAmbiguousTime;


        public Type View => typeof(TimestampToolPage);

        internal TimestampStrings Strings => LanguageManager.Instance.Timestamp;

        internal bool IsInputInvalid
        {
            get => _isInputInvalid;
            set => SetProperty(ref _isInputInvalid, value);
        }

        internal bool IsSupportsDST
        {
            get => _isSupportsDST;
            set => SetProperty(ref _isSupportsDST, value);
        }

        internal bool IsDisabledDST
        {
            get => _isDisabledDST;
            set => SetProperty(ref _isDisabledDST, value);
        }

        internal bool IsDaylightSavingTime
        {
            get => _isDaylightSavingTime;
            set => SetProperty(ref _isDaylightSavingTime, value);
        }

        internal bool IsDSTAmbiguousTime
        {
            get => _isDSTAmbiguousTime;
            set => SetProperty(ref _isDSTAmbiguousTime, value);
        }

        private void DSTInfo()
        {
            IsDSTAmbiguousTime = false;
            IsDaylightSavingTime = false;
            IsSupportsDST = false;
            IsDisabledDST = false;
            if (_currentTimeZone.IsAmbiguousTime(_currentDateTime))
            {
                IsDSTAmbiguousTime = true;
            }
            else if (_currentTimeZone.IsDaylightSavingTime(_currentDateTime))
            {
                IsDaylightSavingTime = true;
            }
            else if (_currentTimeZone.SupportsDaylightSavingTime)
            {
                IsSupportsDST = true;
            }
            else
            {
                IsDisabledDST = true;
            }

        }

        private void SetTimeZoneList()
        {
            foreach (TimeZoneInfo zone in _systemTimeZone)
            {
                _timeZoneCollection.Add(zone.DisplayName, zone.Id);
                TimeZoneDisplayNameCollection.Add(zone.DisplayName);
            }
            //TimeZoneCollection.Reverse();

        }

        internal string CurrentTimeZoneDisplayName
        {
            get => _currentTimeZoneDisplayName;
            set
            {
                if (_timeZoneCollection.ContainsKey(value))
                {
                    _currentTimeZoneDisplayName = value;
                    _currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneCollection[value]);
                    if (_currentTimeZone.BaseUtcOffset.TotalSeconds > 0)
                    {
                        // UTC+
                        _minimumCurrentTimestamp = _minimumUtcTimestamp;
                        _maximumCurrentTimestamp = _maximumUtcTimestamp - (long)_currentTimeZone.BaseUtcOffset.TotalSeconds;
                    }
                    else
                    {
                        // UTC-
                        _minimumCurrentTimestamp = _minimumUtcTimestamp - (long)_currentTimeZone.BaseUtcOffset.TotalSeconds;
                        _maximumCurrentTimestamp = _maximumUtcTimestamp;
                    }
                    UpdateCurrentTimestamp(_currentTimestamp);
                }

            }
        }

        internal double CurrentTimestamp
        {
            get => _currentTimestamp;
            set
            {
                IsInputInvalid = false;
                if (_currentTimestamp == value)
                {
                    return;
                }
                UpdateCurrentTimestamp(value);
            }
        }

        internal int CurrentYear
        {
            get => _currentDateTime.Year;
            set
            {
                if (value < 1) // empty = -2147483648
                {
                    return;
                }
                /*
                if (CurrentDay > DateTime.DaysInMonth(value, CurrentMonth))
                {
                    return;
                }
                 */
                try
                {
                    if (IsValidDateTime(value, CurrentMonth, CurrentDay, CurrentHour, CurrentMinute, CurrentSecond))
                    {
                        _currentUtcDateTime = _currentUtcDateTime.AddYears(value - _currentDateTime.Year);
                    }
                    else
                    {
                        IsInputInvalid = true;
                        return;
                    }
                }
                catch
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.ToUnixTimeSeconds();
            }
        }

        internal int CurrentMonth
        {
            get => _currentDateTime.Month;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                /*
                if (CurrentDay > DateTime.DaysInMonth(CurrentYear, value))
                {
                    return;
                }
                 */
                try
                {
                    if (IsValidDateTime(CurrentYear, value, CurrentDay, CurrentHour, CurrentMinute, CurrentSecond))
                    {
                        _currentUtcDateTime = _currentUtcDateTime.AddMonths(value - _currentDateTime.Month);
                    }
                    else
                    {
                        IsInputInvalid = true;
                        return;
                    }
                }
                catch
                {
                    ResetCurrentDateTime();
                    IsInputInvalid = true;
                    return;

                }
                CurrentTimestamp = _currentUtcDateTime.ToUnixTimeSeconds();
            }
        }

        internal int CurrentDay
        {
            get => _currentDateTime.Day;
            set
            {
                if (value < 0) // empty = -2147483648
                {
                    return;
                }
                /*
                if (value > DateTime.DaysInMonth(CurrentYear, CurrentMonth))
                {
                    return;
                }
                 */
                try
                {
                    if (IsValidDateTime(CurrentYear, CurrentMonth, value, CurrentHour, CurrentMinute, CurrentSecond))
                    {
                        _currentUtcDateTime = _currentUtcDateTime.AddDays(value - _currentDateTime.Day);
                    }
                    else
                    {
                        IsInputInvalid = true;
                        return;
                    }
                }
                catch
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.ToUnixTimeSeconds();
            }
        }

        internal int CurrentHour
        {
            get => _currentDateTime.Hour;
            set
            {
                if (value < -1) // empty = -2147483648
                {
                    return;
                }
                try
                {
                    if (IsValidDateTime(CurrentYear, CurrentMonth, CurrentDay, value, CurrentMinute, CurrentSecond))
                    {
                        _currentUtcDateTime = _currentUtcDateTime.AddHours(value - _currentDateTime.Hour);
                    }
                    else
                    {
                        IsInputInvalid = true;
                        return;
                    }
                }
                catch
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.ToUnixTimeSeconds();
            }
        }

        internal int CurrentMinute
        {
            get => _currentDateTime.Minute;
            set
            {
                if (value < -1) // empty = -2147483648
                {
                    return;
                }
                try
                {
                    if (IsValidDateTime(CurrentYear, CurrentMonth, CurrentDay, CurrentHour, value, CurrentSecond))
                    {
                        _currentUtcDateTime = _currentUtcDateTime.AddMinutes(value - _currentDateTime.Minute);
                    }
                    else
                    {
                        IsInputInvalid = true;
                        return;
                    }
                }
                catch
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.ToUnixTimeSeconds();
            }
        }

        internal int CurrentSecond
        {
            get => _currentDateTime.Second;
            set
            {
                if (value < -1) // empty = -2147483648
                {
                    return;
                }
                try
                {
                    if (IsValidDateTime(CurrentYear, CurrentMonth, CurrentDay, CurrentHour, CurrentMinute, value))
                    {
                        _currentUtcDateTime = _currentUtcDateTime.AddSeconds(value - _currentDateTime.Second);
                    }
                    else
                    {
                        IsInputInvalid = true;
                        return;
                    }
                }
                catch
                {
                    IsInputInvalid = true;
                    return;
                }
                CurrentTimestamp = _currentUtcDateTime.ToUnixTimeSeconds();
            }
        }

        public TimestampToolViewModel()
        {
            PasteCommand = new RelayCommand(ExecutePasteCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand);
            SetTimeZoneList();

            // Set to the current epoch time.
            CurrentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            CurrentTimeZoneDisplayName = TimeZoneInfo.Local.DisplayName;
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
            if (IsValidTimestamp((long)value))
            {
                _currentTimestamp = value;
                _currentUtcDateTime = TimestampToUtcDateTime(value);
                _currentDateTime = TimeZoneInfo.ConvertTime(TimestampToUtcDateTime(value), _currentTimeZone);
                DSTInfo();
                ResetCurrentTimestamp();
                ResetCurrentDateTime();
            }
            else
            {
                IsInputInvalid = true;
            }
        }

        private bool IsValidTimestamp(long value)
        {
            if (value < _minimumCurrentTimestamp || value > _maximumCurrentTimestamp)
            {
                return false;
            }
            // TODO:
            //return TimeZoneInfo.ConvertTime(TimestampToUtcDateTime(value), _currentTimeZone).ToUnixTimeSeconds() == value;
            return true;
        }

        private bool IsValidDateTime(int Year, int Month, int Day, int Hour, int Minute, int Second)
        {
            if (Year is < 1 or > 9999)
            {
                return false;
            }
            TimeSpan offset = _currentTimeZone.BaseUtcOffset;
            DateTimeOffset calcDateTime = new(1970, 1, 1, 0, 0, 0, offset);
            try
            {
                calcDateTime = calcDateTime.AddMonths(Month - 1);
                calcDateTime = calcDateTime.AddDays(Day - 1);
                calcDateTime = calcDateTime.AddHours(Hour);
                calcDateTime = calcDateTime.AddMinutes(Minute);
                calcDateTime = calcDateTime.AddSeconds(Second);
            }
            catch
            {
                return false;
            }

            if (Year + calcDateTime.Year - 1970 is > 1 and < 9999)
            {
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
