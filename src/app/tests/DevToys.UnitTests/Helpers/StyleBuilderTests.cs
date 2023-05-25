using DevToys.Blazor.Core.Helpers;

namespace DevToys.UnitTests.Helpers;

public class StyleBuilderTests
{
    [Fact]
    public void ShouldBulidConditionalInlineStyles()
    {
        //arrange
        bool hasBorder = true;
        bool isOnTop = false;
        int top = 2;
        int bottom = 10;
        int left = 4;
        int right = 20;

        //act
        string ClassToRender = new StyleBuilder("background-color", "DodgerBlue")
                        .AddStyle("border-width", $"{top}px {right}px {bottom}px {left}px", when: hasBorder)
                        .AddStyle("z-index", "999", when: isOnTop)
                        .AddStyle("z-index", "-1", when: !isOnTop)
                        .AddStyle("padding", "35px")
                        .Build();
        //assert
        Assert.Equal("background-color:DodgerBlue;border-width:2px 20px 10px 4px;z-index:-1;padding:35px;", ClassToRender);
    }

    [Fact]
    public void ShouldBulidConditionalInlineStylesFromAttributes()
    {

        //arrange
        bool hasBorder = true;
        bool isOnTop = false;
        int top = 2;
        int bottom = 10;
        int left = 4;
        int right = 20;

        //act
        string StyleToRender = new StyleBuilder("background-color", "DodgerBlue")
                        .AddStyle("border-width", $"{top}px {right}px {bottom}px {left}px", when: hasBorder)
                        .AddStyle("z-index", "999", when: isOnTop)
                        .AddStyle("z-index", "-1", when: !isOnTop)
                        .AddStyle("padding", "35px")
                        .Build();

        IReadOnlyDictionary<string, object> attributes = new Dictionary<string, object> { { "style", StyleToRender } };

        string ClassToRender = new StyleBuilder().AddStyleFromAttributes(attributes).Build();
        //assert
        Assert.Equal("background-color:DodgerBlue;border-width:2px 20px 10px 4px;z-index:-1;padding:35px;", ClassToRender);
    }

    [Fact]
    public void ShouldAddExistingStyle()
    {
        string StyleToRender = StyleBuilder.Empty()
            .AddStyle("background-color:DodgerBlue;")
            .AddStyle("padding", "35px")
            .Build();

        string StyleToRenderFromDefaultConstructor = StyleBuilder.Default(StyleToRender).Build();

        /// Double ;; is valid HTML.
        /// The CSS syntax allows for empty declarations, which means that you can add leading and trailing semicolons as you like. For instance, this is valid CSS
        /// .foo { ;;;display:none;;;color:black;;; }
        /// Trimming is possible, but is it worth the operations for a non-issue?
        Assert.Equal("background-color:DodgerBlue;;padding:35px;", StyleToRender);
        Assert.Equal("background-color:DodgerBlue;;padding:35px;;", StyleToRenderFromDefaultConstructor);

    }

    [Fact]
    public void ShouldNotAddEmptyStyle()
    {
        StyleBuilder StyleToRender = StyleBuilder.Empty().AddStyle("");

        Assert.Null(string.IsNullOrEmpty(StyleToRender.ToString()) ? null : StyleToRender.ToString());

    }

    [Fact]
    public void ShouldAddNestedStyles()
    {

        StyleBuilder Child = StyleBuilder.Empty()
            .AddStyle("background-color", "DodgerBlue")
            .AddStyle("padding", "35px");

        string StyleToRender = StyleBuilder.Empty()
            .AddStyle(Child)
            .AddStyle("z-index", "-1")
            .Build();

        /// Double ;; is valid HTML.
        /// The CSS syntax allows for empty declarations, which means that you can add leading and trailing semicolons as you like. For instance, this is valid CSS
        /// .foo { ;;;display:none;;;color:black;;; }
        /// Trimming is possible, but is it worth the operations for a non-issue?
        Assert.Equal("background-color:DodgerBlue;padding:35px;z-index:-1;", StyleToRender);

    }

    [Fact]
    public void ShouldAddComplexStyles()
    {
        string StyleToRender = StyleBuilder.Empty()
            .AddStyle("text-decoration", v => v
                        .AddValue("underline", true)
                        .AddValue("overline", false)
                        .AddValue("line-through", true),
                        when: true)
            .AddStyle("z-index", "-1")
            .Build();

        /// Double ;; is valid HTML.
        /// The CSS syntax allows for empty declarations, which means that you can add leading and trailing semicolons as you like. For instance, this is valid CSS
        /// .foo { ;;;display:none;;;color:black;;; }
        /// Trimming is possible, but is it worth the operations for a non-issue?
        Assert.Equal("text-decoration:underline line-through;z-index:-1;", StyleToRender);

    }

    [Fact]
    public void ShouldBuildStyleWithFunc()
    {
        {
            //arrange
            // Simulates Razor Components attribute splatting feature
            IReadOnlyDictionary<string, object> attributes = new Dictionary<string, object> { { "class", "my-custom-class-1" } };

            //act
            string StyleToRender = StyleBuilder.Empty()
                            .AddStyle("background-color", () => attributes["style"].ToString(), when: attributes.ContainsKey("style"))
                            .AddStyle("background-color", "black")
                            .Build();
            //assert
            Assert.Equal("background-color:black;", StyleToRender);
        }
    }
}
