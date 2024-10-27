namespace DevToys.UnitTests.Tool.GUI.Components;

public class UIElementWithChildrenTests
{
    private class TestUIElement : UIElementWithChildren
    {
        private readonly IEnumerable<IUIElement> _children;

        public TestUIElement(string id, IEnumerable<IUIElement> children)
            : base(id)
        {
            _children = children;
        }

        protected override IEnumerable<IUIElement> GetChildren() => _children;
    }

    [Fact]
    public void GetChildElementById_ReturnsNull_WhenIdIsNullOrEmpty()
    {
        var element = new TestUIElement("parent", new List<IUIElement>());
        Assert.Null(element.GetChildElementById(null!));
        Assert.Null(element.GetChildElementById(string.Empty));
    }

    [Fact]
    public void GetChildElementById_ReturnsNull_WhenChildNotFound()
    {
        var child = new Mock<IUIElement>();
        child.Setup(c => c.Id).Returns("child1");

        var element = new TestUIElement("parent", new List<IUIElement> { child.Object });
        Assert.Null(element.GetChildElementById("child2"));
    }

    [Fact]
    public void GetChildElementById_ReturnsChild_WhenChildFound()
    {
        var child = new Mock<IUIElement>();
        child.Setup(c => c.Id).Returns("child1");

        var element = new TestUIElement("parent", new List<IUIElement> { child.Object });
        Assert.Equal(child.Object, element.GetChildElementById("child1"));
    }

    [Fact]
    public void GetChildElementById_ReturnsNestedChild_WhenNestedChildFound()
    {
        var nestedChild = new Mock<IUIElement>();
        nestedChild.Setup(c => c.Id).Returns("nestedChild");

        var child = new Mock<IUIElementWithChildren>();
        child.Setup(c => c.Id).Returns("child1");
        child.Setup(c => c.GetChildElementById("nestedChild")).Returns(nestedChild.Object);

        var element = new TestUIElement("parent", new List<IUIElement> { child.Object });
        Assert.Equal(nestedChild.Object, element.GetChildElementById("nestedChild"));
    }

    [Fact]
    public void GetChildElementById_ReturnsFirstMatchingChild_WhenMultipleChildrenWithSameId()
    {
        var child1 = new Mock<IUIElement>();
        child1.Setup(c => c.Id).Returns("child");

        var child2 = new Mock<IUIElement>();
        child2.Setup(c => c.Id).Returns("child");

        var element = new TestUIElement(
            "parent",
            new List<IUIElement> { child1.Object, child2.Object }
        );
        Assert.Equal(child1.Object, element.GetChildElementById("child"));
    }
}
