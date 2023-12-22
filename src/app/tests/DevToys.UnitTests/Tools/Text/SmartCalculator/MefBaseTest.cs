using System.Threading.Tasks;
using DevToys.Core.Mef;
using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using DevToys.Tools.Tools.Text.SmartCalculator.Core;

namespace DevToys.UnitTests.Tools.Text.SmartCalculator;

public abstract class MefBaseTest : IDisposable
{
    private readonly MefComposer _mefComposer;

    private bool _isDisposed;

    protected IMefProvider ExportProvider { get; }

    protected MefBaseTest()
    {
        // Do all the tests in English.
        // LanguageManager.Instance.SetCurrentCulture(new LanguageDefinition("en-US"));

        var mockCurrencyService = new Mock<CurrencyService>
        {
            CallBase = true
        };
        mockCurrencyService.Setup(s => s.LoadLatestRatesAsync())
            .Returns(
                Task.FromResult(
                    new Dictionary<string, double>
                    {
                        { "USD", 1 },
                        { "CAD", 1.31 }
                    }));

        _mefComposer
            = new MefComposer(
                new[]
                {
                    typeof(IMefProvider).Assembly,
                    typeof(ParserAndInterpreterFactory).Assembly
                },
                null,
                mockCurrencyService.As<ICurrencyService>().Object);

        ExportProvider = _mefComposer.ExportProvider.GetExport<IMefProvider>()!.Value;
    }

    ~MefBaseTest()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
            _mefComposer.Dispose();

        _isDisposed = true;
    }
}
