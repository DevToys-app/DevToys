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
        public void BooleanToDoubleConverterTest()
        {
            var converter = new BooleanToDoubleConverter
            {
                ValueOnTrue = 1.0,
                ValueOnFalse = 2.0
            };

            Assert.AreEqual(1.0, converter.Convert(true, typeof(double), null, null));
            Assert.AreEqual(2.0, converter.Convert(false, typeof(double), null, null));
        }

        [TestMethod]
        public void BooleanToGridLengthConverterTest()
        {
            var converter = new BooleanToGridLengthConverter
            {
                GridLengthOnTrue = GridLength.Auto,
                GridLengthOnFalse = new GridLength(123)
            };

            Assert.AreEqual(GridLength.Auto, converter.Convert(true, typeof(GridLength), null, null));
            Assert.AreEqual(new GridLength(123), converter.Convert(false, typeof(GridLength), null, null));
        }

        [TestMethod]
        public void BooleanToIntegerConverterTest()
        {
            var converter = new BooleanToIntegerConverter
            {
                ValueOnTrue = 1,
                ValueOnFalse = 2
            };

            Assert.AreEqual(1, converter.Convert(true, typeof(int), null, null));
            Assert.AreEqual(2, converter.Convert(false, typeof(int), null, null));
        }

        [TestMethod]
        public void BooleanToScrollModeConverterTest()
        {
            var converter = new BooleanToScrollModeConverter
            {
                ScrollModeOnTrue = ScrollMode.Enabled,
                ScrollModeOnFalse = ScrollMode.Disabled
            };

            Assert.AreEqual(ScrollMode.Enabled, converter.Convert(true, typeof(ScrollMode), null, null));
            Assert.AreEqual(ScrollMode.Disabled, converter.Convert(false, typeof(ScrollMode), null, null));
        }

        [TestMethod]
        public void BooleanToTextWrappingConverterTest()
        {
            var converter = new BooleanToTextWrappingConverter
            {
                TextWrappingOnTrue = TextWrapping.NoWrap,
                TextWrappingOnFalse = TextWrapping.Wrap
            };

            Assert.AreEqual(TextWrapping.NoWrap, converter.Convert(true, typeof(TextWrapping), null, null));
            Assert.AreEqual(TextWrapping.Wrap, converter.Convert(false, typeof(TextWrapping), null, null));
        }

        [TestMethod]
        public void BooleanToVisibilityConverterTest()
        {
            var converter = new BooleanToVisibilityConverter
            {
                IsInverted = false
            };

            Assert.AreEqual(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(false, typeof(Visibility), null, null));

            converter.IsInverted = true;

            Assert.AreEqual(Visibility.Collapsed, converter.Convert(true, typeof(Visibility), null, null));
            Assert.AreEqual(Visibility.Visible, converter.Convert(false, typeof(Visibility), null, null));
        }

        [TestMethod]
        public void DoublrToGridLengthConverterTest()
        {
            var converter = new DoubleToGridLengthConverter();

            Assert.AreEqual(new GridLength(2.0), converter.Convert(2.0, typeof(GridLength), null, null));
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

        [TestMethod]
        public void EnumToThicknessConverterTest()
        {
            var converter = new EnumToThicknessConverter
            {
                ThicknessOnEnumDetected = new Thickness(10),
                ThicknessOnEnumNotDetected = new Thickness(0)
            };

            Assert.AreEqual(converter.ThicknessOnEnumNotDetected, converter.Convert(null, null, "foo", null));
            Assert.AreEqual(converter.ThicknessOnEnumNotDetected, converter.Convert(AppBarClosedDisplayMode.Compact, null, null, null));
            Assert.AreEqual(converter.ThicknessOnEnumNotDetected, converter.Convert("foo", null, "foo", null));

            Assert.AreEqual(converter.ThicknessOnEnumDetected, converter.Convert(AppBarClosedDisplayMode.Compact, null, "Compact", null));
            Assert.AreEqual(converter.ThicknessOnEnumNotDetected, converter.Convert(AppBarClosedDisplayMode.Compact, null, "Test", null));
        }

        [TestMethod]
        public void InvertedBooleanConverterTest()
        {
            var converter = new InvertedBooleanConverter();

            Assert.IsFalse((bool)converter.Convert(true, typeof(bool), null, null));
            Assert.IsTrue((bool)converter.Convert(false, typeof(bool), null, null));
        }

        [TestMethod]
        public void NullToBooleanConverterTest()
        {
            var converter = new NullToBooleanConverter
            {
                IsInverted = false
            };

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
        public void NullToVisibilityConverterTest()
        {
            var converter = new NullToVisibilityConverter
            {
                IsInverted = false
            };

            Assert.AreEqual(Visibility.Visible, (Visibility)converter.Convert(null, typeof(bool), null, null));
            Assert.AreEqual(Visibility.Collapsed, (Visibility)converter.Convert(1, typeof(bool), null, null));

            converter.IsInverted = true;

            Assert.AreEqual(Visibility.Collapsed, (Visibility)converter.Convert(null, typeof(bool), null, null));
            Assert.AreEqual(Visibility.Visible, (Visibility)converter.Convert(1, typeof(bool), null, null));
        }
    }
}
