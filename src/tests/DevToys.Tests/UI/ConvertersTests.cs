using DevToys.UI.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevToys.Tests.UI
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

        [TestMethod]
        public void EnumToThicknessConverterTest()
        {
            var converter = new EnumToThicknessConverter();
            converter.ThicknessOnEnumDetected = new Thickness(10);
            converter.ThicknessOnEnumNotDetected = new Thickness(0);

            Assert.AreEqual(converter.ThicknessOnEnumNotDetected, converter.Convert(null, null, "foo", null));
            Assert.AreEqual(converter.ThicknessOnEnumNotDetected, converter.Convert(AppBarClosedDisplayMode.Compact, null, null, null));
            Assert.AreEqual(converter.ThicknessOnEnumNotDetected, converter.Convert("foo", null, "foo", null));

            Assert.AreEqual(converter.ThicknessOnEnumDetected, converter.Convert(AppBarClosedDisplayMode.Compact, null, "Compact", null));
            Assert.AreEqual(converter.ThicknessOnEnumNotDetected, converter.Convert(AppBarClosedDisplayMode.Compact, null, "Test", null));
        }

        [TestMethod]
        public void EnumToBooleanConverterTest()
        {
            var converter = new EnumToBooleanConverter();

            Assert.IsFalse((bool)converter.Convert(null, null, "foo", null));
            Assert.IsFalse((bool)converter.Convert(AppBarClosedDisplayMode.Compact, null, null, null));
            Assert.IsFalse((bool)converter.Convert("foo", null, "foo", null));

            Assert.IsTrue((bool)converter.Convert(AppBarClosedDisplayMode.Compact, null, "Compact", null));
            Assert.IsFalse((bool)converter.Convert(AppBarClosedDisplayMode.Compact, null, "Test", null));

            Assert.AreEqual(AppBarClosedDisplayMode.Compact, converter.ConvertBack(true, typeof(AppBarClosedDisplayMode), "Compact", null));
        }
    }
}
