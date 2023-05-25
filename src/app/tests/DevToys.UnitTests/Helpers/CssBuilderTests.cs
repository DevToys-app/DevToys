using DevToys.Blazor.Core.Helpers;
using Xunit;

namespace DevToys.UnitTests.Helpers;

public class CssBuilderTests
{
    [Fact]
    public void ShouldConstructWithDefaultValue()
    {
        //arrange
        string ClassToRender = CssBuilder.Default("item-one").Build();

        //assert
        Assert.Equal("item-one", ClassToRender);
    }

    [Fact]
    public void ShouldConstructWithEmpty()
    {
        //arrange
        string ClassToRender = string.IsNullOrEmpty(CssBuilder.Empty().ToString()) ? null : CssBuilder.Empty().ToString();

        //assert
        Assert.Null(ClassToRender);
    }

    [Fact]
    public void ShouldBuildConditionalCssClasses()
    {
        //arrange
        bool hasTwo = false;
        bool hasThree = true;
        Func<bool> hasFive = () => false;

        //act
        string ClassToRender = new CssBuilder("item-one")
                        .AddClass("item-two", when: hasTwo)
                        .AddClass("item-three", when: hasThree)
                        .AddClass("item-four")
                        .AddClass("item-five", when: hasFive)
                        .Build();
        //assert
        Assert.Equal("item-one item-three item-four", ClassToRender);
    }
    [Fact]
    public void ShouldBuildConditionalCssBuilderClasses()
    {
        //arrange
        bool hasTwo = false;
        bool hasThree = true;
        Func<bool> hasFive = () => false;

        //act
        string ClassToRender = new CssBuilder("item-one")
                        .AddClass("item-two", when: hasTwo)
                        .AddClass(new CssBuilder("item-three")
                                        .AddClass("item-foo", false)
                                        .AddClass("item-sub-three"),
                                        when: hasThree)
                        .AddClass("item-four")
                        .AddClass("item-five", when: hasFive)
                        .Build();
        //assert
        Assert.Equal("item-one item-three item-sub-three item-four", ClassToRender);
    }
    [Fact]
    public void ShouldBuildEmptyClasses()
    {
        //arrange
        bool shouldShow = false;

        //act
        string ClassToRender = new CssBuilder()
                        .AddClass("some-class", shouldShow)
                        .Build();
        //assert
        Assert.Equal(string.Empty, ClassToRender);
    }

    [Fact]
    public void ShouldBuildClassesWithFunc()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            IReadOnlyDictionary<string, object> attributes = new Dictionary<string, object> { { "class", "my-custom-class-1" } };

            //act
            string ClassToRender = new CssBuilder("item-one")
                            .AddClass(() => attributes["class"].ToString(), when: attributes.ContainsKey("class"))
                            .Build();
            //assert
            Assert.Equal("item-one my-custom-class-1", ClassToRender);
        }
    }

    [Fact]
    public void ShouldBuildClassesFromAttributes()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            IDictionary<string, object> attributes = new Dictionary<string, object> { { "class", "my-custom-class-1" } };

            //act
            string ClassToRender = new CssBuilder("item-one")
                            .AddClassFromAttributes(attributes)
                            .Build();
            //assert
            Assert.Equal("item-one my-custom-class-1", ClassToRender);
        }
    }

    [Fact]
    public void ShouldNotThrowWhenNullFor_BuildClassesFromAttributes()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            IDictionary<string, object> attributes = null;

            //act
            string ClassToRender = new CssBuilder("item-one")
                            .AddClassFromAttributes(attributes)
                            .Build();
            //assert
            Assert.Equal("item-one", ClassToRender);
        }
    }

    [Fact]
    public void ShouldNotThrowWhenNullForAttributeItem_BuildClassesFromAttributes()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            IDictionary<string, object> attributes = new Dictionary<string, object> { { "class", null } };

            //act
            string ClassToRender = new CssBuilder("item-one")
                            .AddClassFromAttributes(attributes)
                            .Build();
            //assert
            Assert.Equal("item-one", ClassToRender);
        }
    }

    [Fact]
    public void ForceNullForWhitespace_BuildClassesFromAttributes()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            IDictionary<string, object> attributes = null;

            //act
            string ClassToRender = string.IsNullOrEmpty(new CssBuilder().AddClassFromAttributes(attributes).ToString()) ? null : new CssBuilder().AddClassFromAttributes(attributes).ToString();

            //assert
            Assert.Null(ClassToRender);
        }
    }

    [Fact]
    public void ShouldNotThrowNoKeyExceptionWithDictionary()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            IDictionary<string, object> attributes = new Dictionary<string, object> { { "foo", "bar" } };

            //act
            string ClassToRender = new CssBuilder("item-one")
                            .AddClass(() => attributes["string"].ToString(), when: attributes.ContainsKey("class"))
                            .Build();
            //assert
            Assert.Equal("item-one", ClassToRender);
        }
    }

    [Fact]
    public void ShouldBuildConditionalCssClassesUsingPrefix()
    {
        //arrange
        bool hasTwo = false;
        bool hasThree = true;
        Func<bool> hasFive = () => false;

        //act
        string ClassToRender = new CssBuilder("default")
                        .SetPrefix("item-")
                        .AddClass("two", when: hasTwo)
                        .AddClass("three", when: hasThree)
                        .AddClass("four")
                        .AddClass("five", when: hasFive)
                        .Build();
        //assert
        Assert.Equal("default item-three item-four", ClassToRender);
    }

    [Fact]
    public void ShouldBuildConditionalCssClassesUsingMultiplePrefixes()
    {
        //arrange
        bool hasTwo = true;
        bool hasThree = true;
        Func<bool> hasFive = () => false;

        //act
        string ClassToRender = new CssBuilder("default")
                        .AddClass("no-prefix-two", when: hasTwo)
                        .SetPrefix("item-")
                        .AddClass("three", when: hasThree)
                        .SetPrefix("namespace-")
                        .AddClass("four")
                        .AddClass("five", when: hasFive)
                        .Build();
        //assert
        Assert.Equal("default no-prefix-two item-three namespace-four", ClassToRender);
    }

    [Fact]
    public void ShouldBuildClassesFromAttributesWithPrefix()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            IDictionary<string, object> attributes = new Dictionary<string, object> { { "class", "my-custom-class-1" } };

            //act
            string ClassToRender = new CssBuilder("item-one")
                            .SetPrefix("pre-")
                            .AddClassFromAttributes(attributes)
                            .Build();
            //assert
            Assert.Equal("item-one pre-my-custom-class-1", ClassToRender);
        }
    }

    [Fact]
    public void ShouldBuildClassesFromAttributesWithClearedPrefix()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            IDictionary<string, object> attributes = new Dictionary<string, object> { { "class", "my-custom-class-1" } };

            //act
            string ClassToRender = new CssBuilder("item-one")
                            .SetPrefix("item-")
                            .AddClass("two")
                            .SetPrefix(String.Empty)
                            .AddClassFromAttributes(attributes)
                            .Build();
            //assert
            Assert.Equal("item-one item-two my-custom-class-1", ClassToRender);
        }
    }
}
