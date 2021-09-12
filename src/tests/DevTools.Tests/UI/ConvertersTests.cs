#nullable disable

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

        [TestMethod]
        public void NullToBooleanConverterTest()
        {
            var converter = new NullToBooleanConverter();
            converter.IsInverted = false;

            Assert.IsTrue((bool)converter.Convert(null, typeof(bool), null, null));
            Assert.IsFalse((bool)converter.Convert(1, typeof(bool), null, null));

            converter.IsInverted = true;

            Assert.IsFalse((bool)converter.Convert(null, typeof(bool), null, null));
            Assert.IsTrue((bool)converter.Convert(1, typeof(bool), null, null));

            converter.IsInverted = false;

            Assert.IsTrue((bool)converter.Convert(string.Empty, typeof(bool), null, null));
            Assert.IsFalse((bool)converter.Convert("foo", typeof(bool), null, null));

            converter.IsInverted = true;

            Assert.IsFalse((bool)converter.Convert(string.Empty, typeof(bool), null, null));
            Assert.IsTrue((bool)converter.Convert("foo", typeof(bool), null, null));
        }
    }
}
