using CascadePass.CascadeCore.UI.Converters;
using System;
using System.Windows;

namespace CascadePass.CascadeCore.UI.Tests.Converters
{
    [TestClass]
    public class EmptyStringToVisibilityConverterTests
    {
        [TestMethod]
        public void Space_Returns_Collapsed()
        {
            var converter = new EmptyStringToVisibilityConverter();
            var result = converter.Convert(" ", null, null, null);

            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [TestMethod]
        public void Tab_Returns_Collapsed()
        {
            var converter = new EmptyStringToVisibilityConverter();
            var result = converter.Convert("\t", null, null, null);

            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [TestMethod]
        public void Newline_Returns_Collapsed()
        {
            var converter = new EmptyStringToVisibilityConverter();
            var result = converter.Convert(Environment.NewLine, null, null, null);

            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [TestMethod]
        public void EmptyString_Returns_Collapsed()
        {
            var converter = new EmptyStringToVisibilityConverter();
            var result = converter.Convert(string.Empty, null, null, null);

            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [TestMethod]
        public void Null_Returns_Collapsed()
        {
            var converter = new EmptyStringToVisibilityConverter();
            var result = converter.Convert(null, null, null, null);

            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [TestMethod]
        public void Letter_Returns_Visible()
        {
            var converter = new EmptyStringToVisibilityConverter();
            var result = converter.Convert("A", null, null, null);

            Assert.AreEqual(Visibility.Visible, result);
        }

        [TestMethod]
        public void Guid_Returns_Visible()
        {
            var converter = new EmptyStringToVisibilityConverter();
            var result = converter.Convert(Guid.NewGuid().ToString(), null, null, null);

            Assert.AreEqual(Visibility.Visible, result);
        }

        [TestMethod]
        public void ConvertBack_Returns_UnsetValue()
        {
            var converter = new EmptyStringToVisibilityConverter();
            var result = converter.ConvertBack(Guid.NewGuid().ToString(), null, null, null);

            Assert.AreEqual(DependencyProperty.UnsetValue, result);
        }
    }
}
