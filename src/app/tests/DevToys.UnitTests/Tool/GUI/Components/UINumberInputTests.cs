using System.Globalization;

namespace DevToys.UnitTests.Tool.GUI.Components;

public class UINumberInputTests
{
    [Fact]
    public void Value_ParsesTextWithInvariantCulture()
    {
        // The HTML number input always provides its text with '.' as the decimal separator.
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            IUINumberInput numberInput = DevToys.Api.GUI.NumberInput("id").Text("1.5");

            Assert.Equal(1.5, numberInput.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void ValueExtension_WritesTextWithInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            IUINumberInput numberInput = DevToys.Api.GUI.NumberInput("id").Value(1.5);

            Assert.Equal("1.5", numberInput.Text);
            Assert.Equal(1.5, numberInput.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
