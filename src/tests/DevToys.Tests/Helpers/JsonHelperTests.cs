using DevToys.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Helpers
{
    [TestClass]
    public class JsonHelperTests
    {
        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("", false)]
        [DataRow(" ", false)]
        [DataRow("   {  }  ", true)]
        [DataRow("   { \"foo\": 123 }  ", true)]
        [DataRow("   bar { \"foo\": 123 }  ", false)]
        public void IsValidJson(string input, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.IsValidJson(input));
        }
    }
}
