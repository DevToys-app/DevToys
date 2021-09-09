using DevTools.Common.UI.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.UI.Xaml;

namespace DevTools.Tests.UI
{
    [TestClass]
    public class ConvertersTests
    {
        [TestMethod]
        public void BooleanToVisibilityConverterTest()
        {
            var converter = new BooleanToVisibilityConverter();
            converter.IsInverted = false;

            Assert.AreEqual(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(false, typeof(Visibility), null, null));

            converter.IsInverted = true;

            Assert.AreEqual(Visibility.Collapsed, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Visible, converter.Convert(false, typeof(Visibility), null, null));
        }
    }
}
