﻿using System.Linq;
using DevToys.Api.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers
{
    [TestClass]
    public class IToolProviderFactoryTest : MefBaseTest
    {
        [DataTestMethod]
        [DataRow(null, 0)]
        [DataRow("", 0)]
        [DataRow(" ", 0)]
        [DataRow("e", 5)]
        [DataRow("encoder", 1)]
        [DataRow("ENCODER", 1)]
        [DataRow("ENCODER ", 1)]
        public void GetTools(string searchquery, int expectedMatchSpanCount)
        {
            System.Collections.Generic.IEnumerable<MatchedToolProvider> result = ExportProvider.Import<IToolProviderFactory>().GetTools(searchquery);
            MatchedToolProvider base64Tool = result.First(item => item.Metadata.ProtocolName == "base64");
            Assert.AreEqual(expectedMatchSpanCount, base64Tool.MatchedSpans.Length);
        }
    }
}
