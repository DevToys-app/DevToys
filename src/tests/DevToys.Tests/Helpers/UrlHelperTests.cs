using DevToys.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Helpers
{
    [TestClass]
    public class UrlHelperTests
    {
        [DataTestMethod]
        [DataRow("123\r456", "123%0A456")]
        [DataRow("test\r", "test%0A")]
        [DataRow("hello\r\nworld", "hello%0Aworld")]
        public void EncodeNewLines(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, UrlHelper.UrlEncode(input));
        }
    }
}
