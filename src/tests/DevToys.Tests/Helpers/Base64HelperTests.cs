using DevToys.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Helpers
{
    [TestClass]
    public class Base64HelperTests
    {
        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("", false)]
        [DataRow(" ", false)]
        [DataRow("aGVsbG8gd29ybGQ=", true)]
        [DataRow("aGVsbG8gd2f9ybGQ=", false)]
        [DataRow("SGVsbG8gV29y", true)]
        [DataRow("SGVsbG8gVa29y", false)]
        public void IsValid(string input, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, Base64Helper.IsBase64DataStrict(input));
        }
    }
}
