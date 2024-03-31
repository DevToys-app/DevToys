using System.Globalization;
using System.Reflection;
using DevToys.Core.Mef;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests;

public abstract class MefBasedTest
{
    private readonly MefComposer _mefComposer;

    protected IMefProvider MefProvider => _mefComposer.Provider;

    protected MefBasedTest(params Assembly[] assembliesToLoad)
    {
        LoggingExtensions.LoggerFactory ??= LoggerFactory.Create(builder => { });

        // Initialize MEF.
        _mefComposer = new MefComposer([.. assembliesToLoad, typeof(MefBasedTest).Assembly]);

        // Set language to english for unit tests.
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
    }
}
