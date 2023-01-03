namespace DevToys.UnitTests.Core;

public class ExtensionOrdererTests
{
    [Fact]
    public void EquivalentTest()
    {

        TestLazy[] extensions = new[] {
                    Create("Foo"),
                    Create("Bar"),
                    Create("Baz")
                };

        IEnumerable<Lazy<string, OrderableMetadata>> result = ExtensionOrderer.Order(extensions);

        Assert.Equivalent(extensions, result);
    }

    [Fact]
    public void DuplicateNameTest()
    {

        TestLazy[] extensions = new[] {
                    Create("Foo"),
                    Create("Foo"),
                    Create("Baz")
                };

        Assert.Throws<ArgumentException>(() => ExtensionOrderer.Order(extensions));
    }

    [Fact]
    public void CycleAfterSelfTest()
    {

        TestLazy[] extensions = new[] {
                Create("Foo").After("Foo"),
            };

        Assert.Throws<ArgumentException>(() => ExtensionOrderer.Order(extensions));
    }

    [Fact]
    public void CycleBeforeSelfTest()
    {

        TestLazy[] extensions = new[] {
                Create("Foo").Before("Foo"),
            };

        Assert.Throws<ArgumentException>(() => ExtensionOrderer.Order(extensions));
    }

    [Fact]
    public void SimpleOrderTest()
    {

        TestLazy[] extensions = new[] {
                Create("Foo").After("Baz"),
                Create("Bar").After("Baz").Before("Foo"),
                Create("Baz")
            };

        IEnumerable<Lazy<string, OrderableMetadata>> result = ExtensionOrderer.Order(extensions);

        string[] expected = new[] {
                "Baz",
                "Bar",
                "Foo"
            };

        Assert.Equivalent(result.Select(e => e.Metadata.InternalComponentName), expected);
    }

    [Fact]
    public void SimpleCycleTest()
    {

        TestLazy[] extensions = new[] {
                Create("Foo").After("Baz"),
                Create("Bar").After("Baz").Before("Foo"),
                Create("Baz").After("Foo")
            };

        Assert.Throws<ArgumentException>(() => ExtensionOrderer.Order(extensions));
    }

    [Fact]
    public void OrderTest()
    {

        TestLazy[] extensions = new[] {
                Create("Foo").After("Baz").After("Bar"),
                Create("Bar").After("Baz").Before("Foo"),
                Create("Baz").Before("Bar").Before("Foo")
            };

        IEnumerable<Lazy<string, OrderableMetadata>> result = ExtensionOrderer.Order(extensions);

        string[] expected = new[] {
                "Baz",
                "Bar",
                "Foo"
            };

        Assert.Equivalent(result.Select(e => e.Metadata.InternalComponentName), expected);
    }

    [Fact]
    public void CycleTest()
    {

        TestLazy[] extensions = new[] {
                Create("Foo").After("Baz").After("Bar"),
                Create("Bar").After("Baz").Before("Foo"),
                Create("Baz").Before("Bar").Before("Foo").Before("Bar")
            };

        IEnumerable<Lazy<string, OrderableMetadata>> result = ExtensionOrderer.Order(extensions);

        string[] expected = new[] {
                "Baz",
                "Bar",
                "Foo"
            };

        Assert.Equivalent(result.Select(e => e.Metadata.InternalComponentName), expected);
    }

    #region Helper

    private static TestLazy Create(string name)
    {
        return new TestLazy(name);
    }

    private class OrderableMetadata : IOrderableMetadata
    {

        public OrderableMetadata(string name)
        {
            InternalComponentName = name;
            Before = new List<string>();
            After = new List<string>();
        }

        public string InternalComponentName { get; }
        public List<string> Before { get; }
        public List<string> After { get; }

        string IOrderableMetadata.InternalComponentName => InternalComponentName;
        IReadOnlyList<string> IOrderableMetadata.Before => Before;
        IReadOnlyList<string> IOrderableMetadata.After => After;
    }

    private class TestLazy : Lazy<string, OrderableMetadata>
    {
        public TestLazy(string name) : base(() => name, new OrderableMetadata(name))
        {
        }

        public TestLazy Before(string before)
        {
            Metadata.Before.Add(before);
            return this;
        }
        public TestLazy After(string after)
        {
            Metadata.After.Add(after);
            return this;
        }
    }

    #endregion
}
