using DevToys.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Helpers
{
    [TestClass]
    public class YamlHelperTests
    {
        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("", false)]
        [DataRow(" ", false)]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", false)]
        [DataRow("foo :\n  bar :\n    - boo: 1\n    - rab: 2\n    - plop: 3", true)]
        public void IsValidYaml(string input, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, YamlHelper.IsValidYaml(input));
        }
    }
}
