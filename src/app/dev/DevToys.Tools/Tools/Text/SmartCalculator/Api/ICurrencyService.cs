namespace DevToys.Tools.Tools.Text.SmartCalculator.Api;

/// <summary>
/// Provides a way to convert currencies using recent exchange rates.
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Converts a currency.
    /// </summary>
    /// <param name="fromCurrencyIso">i.e USD</param>
    /// <param name="value">The value in <paramref name="fromCurrencyIso"/> currency.</param>
    /// <param name="toCurrencyIso">i.e EUR</param>
    /// <returns>Returns null if <paramref name="fromCurrencyIso"/> or <paramref name="toCurrencyIso"/> can't be found.</returns>
    Task<double?> ConvertCurrencyAsync(string fromCurrencyIso, double value, string toCurrencyIso);
}
