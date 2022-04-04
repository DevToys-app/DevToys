﻿using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
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
        public async Task SearchTools(string searchquery, int expectedMatchSpanCount)
        {
            await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                System.Collections.Generic.IEnumerable<ToolProviderViewItem> result = await ExportProvider.Import<IToolProviderFactory>().SearchToolsAsync(searchquery);

                if (expectedMatchSpanCount > 0)
                {
                    ToolProviderViewItem base64Tool = result.First(item => item.Metadata.ProtocolName == "base64");
                    Assert.AreEqual(expectedMatchSpanCount, base64Tool.MatchedSpans.Length);
                }
                else
                {
                    Assert.IsFalse(result.Any());
                }
            });
        }

        [DataTestMethod]
        [DataRow(null, 0)]
        [DataRow("", 0)]
        [DataRow(" ", 0)]
        [DataRow("RFC 4648", 1)]
        [DataRow("RFC4648", 1)]
        [DataRow(" RFC 4648 base64 ", 1)]
        public async Task SearchToolsKeyword(string searchquery, int expectedMatchCount)
        {
            await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                System.Collections.Generic.IEnumerable<ToolProviderViewItem> result = await ExportProvider.Import<IToolProviderFactory>().SearchToolsAsync(searchquery);

                if (expectedMatchCount > 0)
                {
                    Assert.AreEqual(expectedMatchCount, result.ToList().Count);
                }
                else
                {
                    Assert.IsFalse(result.Any());
                }
            });
        }

        [TestMethod]
        public async Task ToolTree()
        {
            await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                System.Collections.Generic.IEnumerable<ToolProviderViewItem> result = await ExportProvider.Import<IToolProviderFactory>().GetToolsTreeAsync();

                Assert.IsTrue(result.Count() > 0);
                Assert.IsTrue(result.First().ChildrenTools.Count > 0);
            });
        }

        [TestMethod]
        public async Task ChildrenTool()
        {
            await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                System.Collections.Generic.IEnumerable<ToolProviderViewItem> result = await ExportProvider.Import<IToolProviderFactory>().GetToolsTreeAsync();
                System.Collections.Generic.IEnumerable<ToolProviderViewItem> result2 = ExportProvider.Import<IToolProviderFactory>().GetAllChildrenTools(result.First().ToolProvider);

                Assert.IsTrue(result2.Count() > 0);
            });
        }
    }
}
