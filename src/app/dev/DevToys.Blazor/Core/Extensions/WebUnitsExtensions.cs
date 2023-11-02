using System.Globalization;

namespace DevToys.Blazor.Core.Extensions;

internal static class WebUnitsExtensions
{
    internal static string ToPx(this int val) => $"{val}px";

    internal static string ToPx(this int? val) => val != null ? val.Value.ToPx() : string.Empty;

    internal static string ToPx(this long val) => $"{val}px";

    internal static string ToPx(this long? val) => val != null ? val.Value.ToPx() : string.Empty;

    internal static string ToPx(this double val) => $"{val.ToString("0.##", CultureInfo.InvariantCulture)}px";

    internal static string ToPx(this double? val) => val != null ? val.Value.ToPx() : string.Empty;

    internal static int ToPxInteger(this UISpacing val)
    {
        return val switch
        {
            UISpacing.None => 0,
            UISpacing.Small => 4,
            UISpacing.Medium => 8,
            UISpacing.Large => 12,
            _ => throw new NotSupportedException()
        };
    }

    internal static string ToPercentage(this int val) => $"{val}%";

    internal static string ToPercentage(this int? val) => val != null ? val.Value.ToPercentage() : string.Empty;

    internal static string ToPercentage(this long val) => $"{val}%";

    internal static string ToPercentage(this long? val) => val != null ? val.Value.ToPercentage() : string.Empty;

    internal static string ToPercentage(this double val) => $"{val.ToString("0.##", CultureInfo.InvariantCulture)}%";

    internal static string ToPercentage(this double? val) => val != null ? val.Value.ToPercentage() : string.Empty;
}
