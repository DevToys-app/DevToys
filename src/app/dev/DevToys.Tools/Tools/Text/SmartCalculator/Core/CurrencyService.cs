using System.Reflection;
using System.Runtime.CompilerServices;
using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using Newtonsoft.Json;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Core;

// TODO: Instead of mocking CurrencyService, we should create a new service for LoadLatestRatesAsync
// and mock it.
[Export(typeof(ICurrencyService))]
public /*sealed // Don't make it sealed or Moq won't work*/ class CurrencyService : ICurrencyService
{
    // Using https://exchangerate.host/
    private const string LatestRatesUrl = "https://api.exchangerate.host/latest?base=USD&amount=1";
    private const string BaseCurrency = "USD";

    private readonly DisposableSemaphore _disposableSempahore = new();

    private DateTime _lastUpdate = DateTime.MinValue;
    private Dictionary<string, double>? _isoCurrencyRates;

    [ImportingConstructor]
    public CurrencyService()
    {
        LoadLatestRatesAsync().ForgetSafely();
    }

    public async Task<double?> ConvertCurrencyAsync(string fromCurrencyIso, double value, string toCurrencyIso)
    {
        if (string.IsNullOrWhiteSpace(fromCurrencyIso))
            fromCurrencyIso = BaseCurrency;

        if (string.IsNullOrWhiteSpace(toCurrencyIso))
            toCurrencyIso = BaseCurrency;

        Dictionary<string, double>? isoCurrencyRates
            = await LoadLatestRatesAsync().ConfigureAwait(false);

        if (isoCurrencyRates is null
            || !isoCurrencyRates.TryGetValue(fromCurrencyIso, out double fromRate)
            || !isoCurrencyRates.TryGetValue(toCurrencyIso, out double toRate))
        {
            return null;
        }

        if (string.Equals(BaseCurrency, fromCurrencyIso, StringComparison.OrdinalIgnoreCase))
            return fromRate * toRate * value;

        return toRate / fromRate * value;
    }

    // Internal for unit tests
    public virtual async Task<Dictionary<string, double>?> LoadLatestRatesAsync()
    {
        using (await _disposableSempahore.WaitAsync(CancellationToken.None))
        {
            if (_lastUpdate >= DateTime.Now - TimeSpan.FromHours(1) && _isoCurrencyRates is not null)
                return _isoCurrencyRates;

            try
            {
                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

                using var httpClient = new HttpClient();
                string jsonResult = await httpClient.GetStringAsync(string.Format(LatestRatesUrl)).ConfigureAwait(false);
                LatestRatesResult? latestRatesResult = JsonConvert.DeserializeObject<LatestRatesResult>(jsonResult);

                if (latestRatesResult is not null
                    && latestRatesResult.Success.HasValue && latestRatesResult.Success.Value
                    && latestRatesResult.Rates is not null)
                {
                    _lastUpdate = DateTime.Now;
                    _isoCurrencyRates = latestRatesResult.Rates;
                    return latestRatesResult.Rates;
                }

                // Failed.
            }
            catch
            {
                // Ignore. Could fail for multiple reasons including no internet connection.
            }

            // TODO: Should we tell the user that rates may not be up to date?
            _lastUpdate = DateTime.MinValue;
            _isoCurrencyRates = LoadEmbeddedExchangeRates()?.Rates;
            return _isoCurrencyRates;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static LatestRatesResult? LoadEmbeddedExchangeRates()
    {
        var assembly = Assembly.GetExecutingAssembly();

        using Stream? embeddedResourceStream = assembly.GetManifestResourceStream("DevToys.Tools.Tools.Text.SmartCalculator.Core.Assets.defaultExchangeRates.json");
        if (embeddedResourceStream is null)
            throw new Exception("Unable to find the defaultExchangeRates file.");

        using var textStreamReader = new StreamReader(embeddedResourceStream);

        return JsonConvert.DeserializeObject<LatestRatesResult>(textStreamReader.ReadToEnd());
    }

    private sealed class LatestRatesResult
    {
        [JsonProperty("base", NullValueHandling = NullValueHandling.Ignore)]
        public string? Base { get; set; }

        [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
        public string? Date { get; set; }

        [JsonProperty("rates", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, double>? Rates { get; set; }

        [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Success { get; set; }
    }
}
