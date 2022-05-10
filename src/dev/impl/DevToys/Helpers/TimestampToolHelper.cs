#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevToys.Core;
using DevToys.Models;

namespace DevToys.Helpers
{
    internal static class TimestampToolHelper
    {
        internal static class ZoneInfo
        {
            private static string _utcDisplayName = "";
            private static string _localDisplayName = "";
            private static readonly ReadOnlyCollection<TimeZoneInfo> _systemTimeZone = TimeZoneInfo.GetSystemTimeZones();
            private static readonly Dictionary<string, string> _timeZoneCollection = InitTimeZoneCollection();

            internal static string UtcDisplayName => _utcDisplayName;

            internal static string LocalDisplayName => _localDisplayName;

            internal static List<string> DisplayNames => _timeZoneCollection.Keys.ToList();

            internal static Dictionary<string, string> TimeZones => _timeZoneCollection;

            private static Dictionary<string, string> InitTimeZoneCollection()
            {
                Dictionary<string, string> timeZoneCollection = new();
                if (!Regex.IsMatch(_systemTimeZone[0].DisplayName, @"^\(UTC.*\).+$"))
                {
                    // version < .Net6
                    // This implementation is due to the effect that projects
                    // for using external tools require .net6 or earlier libraries.
                    foreach (TimeZoneInfo zone in _systemTimeZone)
                    {
                        string displayName = $"(UTC{zone.BaseUtcOffset.Hours:+00;-00;}:{zone.BaseUtcOffset.Minutes:00;00;}) " + zone.DisplayName;
                        if (zone.Id == TimeZoneInfo.Utc.Id)
                        {
                            displayName = "(UTC) " + zone.DisplayName;
                            _utcDisplayName = "(UTC) " + zone.DisplayName;
                        }
                        if (zone.Id == TimeZoneInfo.Local.Id)
                        {
                            _localDisplayName = displayName;
                        }
                        timeZoneCollection.Add(displayName, zone.Id);
                    }
                }
                else
                {
                    // version >= .Net6
                    foreach (TimeZoneInfo zone in _systemTimeZone)
                    {
                        timeZoneCollection.Add(zone.DisplayName, zone.Id);
                    }
                    _utcDisplayName = TimeZoneInfo.Utc.DisplayName;
                    _localDisplayName = TimeZoneInfo.Local.DisplayName;
                }
                return timeZoneCollection;
            }
        }

        internal static class TimeZone
        {
            internal static DateTimeOffset SafeMinValue(TimeZoneInfo timezone)
            {
                if (timezone is null)
                {
                    timezone = TimeZoneInfo.Utc;
                }
                DateTimeOffset t1 = TimeZoneInfo.ConvertTime(
                    new DateTimeOffset(10, 1, 1, 0, 0, 0, TimeZoneInfo.Utc.BaseUtcOffset),
                    timezone);
                DateTimeOffset minValue = DateTimeOffset.MinValue;
                if (t1.Year < 10)
                {
                    minValue = minValue.Subtract(t1.Offset);
                }
                return TimeZoneInfo.ConvertTime(minValue, timezone);
            }

            internal static DateTimeOffset SafeMaxValue(TimeZoneInfo timezone)
            {
                if (timezone is null)
                {
                    timezone = TimeZoneInfo.Utc;
                }
                DateTimeOffset t1 = TimeZoneInfo.ConvertTime(
                    new DateTimeOffset(9990, 12, 31, 23, 59, 59, TimeZoneInfo.Utc.BaseUtcOffset),
                    timezone);
                DateTimeOffset maxValue = DateTimeOffset.MaxValue;
                if (t1.Year > 9990)
                {
                    maxValue = maxValue.Subtract(t1.Offset);
                }
                return TimeZoneInfo.ConvertTime(maxValue, timezone);
            }

        }

    }
}
