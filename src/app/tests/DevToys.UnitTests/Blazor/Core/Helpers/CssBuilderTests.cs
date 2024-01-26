using DevToys.Blazor.Core.Helpers;

namespace DevToys.UnitTests.Blazor.Core.Helpers;

public class CssBuilderTests
{
    [Fact]
    public void ShouldConstructWithDefaultValue()
    {
        //arrange
        string classToRender = CssBuilder.Default("item-one").Build();

        //assert
        classToRender.Should().Be("item-one");
    }

    [Fact]
    public void ShouldConstructWithEmpty()
    {
        //arrange
        CssBuilder? classToRender = CssBuilder.Empty();
        classToRender = string.IsNullOrEmpty(classToRender.ToString()) ? null : classToRender;

        //assert
        classToRender.Should().BeNull();
    }

    [Fact]
    public void ShouldBuildConditionalCssClasses()
    {
        //arrange
        bool hasTwo = false;
        bool hasThree = true;
        static bool hasFive() => false;

        //act
        string classToRender = new CssBuilder("item-one")
                        .AddClass("item-two", when: hasTwo)
                        .AddClass("item-three", when: hasThree)
                        .AddClass("item-four")
                        .AddClass("item-five", when: hasFive)
                        .Build();
        //assert
        classToRender.Should().Be("item-one item-three item-four");
    }

    [Fact]
    public void ShouldBuildConditionalCssBuilderClasses()
    {
        //arrange
        bool hasTwo = false;
        bool hasThree = true;
        static bool hasFive() => false;

        //act
        string classToRender = new CssBuilder("item-one")
                        .AddClass("item-two", when: hasTwo)
                        .AddClass(new CssBuilder("item-three")
                                        .AddClass("item-foo", false)
                                        .AddClass("item-sub-three"),
                                        when: hasThree)
                        .AddClass("item-four")
                        .AddClass("item-five", when: hasFive)
                        .Build();
        //assert
        classToRender.Should().Be("item-one item-three item-sub-three item-four");
    }

    [Fact]
    public void ShouldBuildEmptyClasses()
    {
        //arrange
        bool shouldShow = false;

        //act
        string classToRender = new CssBuilder()
                        .AddClass("some-class", shouldShow)
                        .Build();
        //assert
        classToRender.Should().Be(string.Empty);
    }

    [Fact]
    public void ShouldBuildClassesWithFunc()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            var attributes = new Dictionary<string, object> { { "class", "my-custom-class-1" } };

            //act
            string classToRender = new CssBuilder("item-one")
                            .AddClass(() => attributes["class"].ToString(), when: attributes.ContainsKey("class"))
                            .Build();
            //assert
            classToRender.Should().Be("item-one my-custom-class-1");
        }
    }

    [Fact]
    public void ShouldBuildClassesFromAttributes()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            var attributes = new Dictionary<string, object> { { "class", "my-custom-class-1" } };

            //act
            string classToRender = new CssBuilder("item-one")
                            .AddClassFromAttributes(attributes)
                            .Build();
            //assert
            classToRender.Should().Be("item-one my-custom-class-1");
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
            string classToRender = new CssBuilder("item-one")
                            .AddClassFromAttributes(attributes)
                            .Build();
            //assert
            classToRender.Should().Be("item-one");
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
            CssBuilder? classToRender = new CssBuilder()
                            .AddClassFromAttributes(attributes);
            classToRender = string.IsNullOrEmpty(classToRender.ToString()) ? null : classToRender;

            //assert
            classToRender.Should().BeNull();
        }
    }

    [Fact]
    public void ShouldNotThrowNoKeyExceptionWithDictionary()
    {
        //arrange
        // Simulates Razor Components attribute splatting feature
        var attributes = new Dictionary<string, object> { { "foo", "bar" } };

        //act
        string classToRender = new CssBuilder("item-one")
                        .AddClass(() => attributes["string"].ToString(), when: attributes.ContainsKey("class"))
                        .Build();
        //assert
        classToRender.Should().Be("item-one");
    }
}
