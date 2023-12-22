using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

namespace DevToys.UnitTests.Tools.Text.SmartCalculator;

public static class TestHelper
{
    public static string GetDataDisplayText(this IData data)
    {
        return data.GetDisplayText(SupportedCultures.English);
    }
}
