using DevToys.Blazor.Core.Extensions;
using DevToys.Blazor.Core.Helpers;

namespace DevToys.UnitTests.Blazor.Core.Helpers;

public class StyleBuilderTests
{
    [Fact]
    public void ShouldBuildConditionalInlineStyles()
    {
        //arrange
        bool hasBorder = true;
        bool isOnTop = false;
        int top = 2;
        int bottom = 10;
        int left = 4;
        int right = 20;

        //act
        string classToRender = new StyleBuilder("background-color", "DodgerBlue")
                        .AddStyle("border-width", $"{top.ToPx()} {right.ToPx()} {bottom.ToPx()} {left.ToPx()}", when: hasBorder)
                        .AddStyle("z-index", "999", when: isOnTop)
                        .AddStyle("z-index", "-1", when: !isOnTop)
                        .AddStyle("padding", "35px")
                        .Build();
        //assert
        classToRender.Should().Be("background-color:DodgerBlue;border-width:2px 20px 10px 4px;z-index:-1;padding:35px;");
    }

    [Fact]
    public void ShouldBuildConditionalInlineStylesFromAttributes()
    {
        //arrange
        bool hasBorder = true;
        bool isOnTop = false;
        int top = 2;
        int bottom = 10;
        int left = 4;
        int right = 20;

        //act
        string styleToRender = new StyleBuilder("background-color", "DodgerBlue")
                        .AddStyle("border-width", $"{top}px {right}px {bottom}px {left}px", when: hasBorder)
                        .AddStyle("z-index", "999", when: isOnTop)
                        .AddStyle("z-index", "-1", when: !isOnTop)
                        .AddStyle("padding", "35px")
                        .Build();

        IDictionary<string, object> attributes = new Dictionary<string, object> { { "style", styleToRender } };

        string classToRender = new StyleBuilder().AddStyleFromAttributes(attributes).Build();
        //assert
        classToRender.Should().Be("background-color:DodgerBlue;border-width:2px 20px 10px 4px;z-index:-1;padding:35px;");
    }

    [Fact]
    public void ShouldAddExistingStyle()
    {
        string styleToRender = StyleBuilder.Empty()
            .AddStyle("background-color:DodgerBlue;")
            .AddStyle("padding", "35px")
            .Build();

        string styleToRenderFromDefaultConstructor = StyleBuilder.Default(styleToRender).Build();

        /// Double ;; is valid HTML.
        /// The CSS syntax allows for empty declarations, which means that you can add leading and trailing semicolons as you like. For instance, this is valid CSS
        /// .foo { ;;;display:none;;;color:black;;; }
        /// Trimming is possible, but is it worth the operations for a non-issue?
        styleToRender.Should().Be("background-color:DodgerBlue;;padding:35px;");
        styleToRenderFromDefaultConstructor.Should().Be("background-color:DodgerBlue;;padding:35px;;");
    }

    [Fact]
    public void ShouldNotAddEmptyStyle()
    {
        StyleBuilder? styleToRender = StyleBuilder.Empty().AddStyle("");
        styleToRender = string.IsNullOrEmpty(styleToRender.ToString()) ? null : styleToRender;

        styleToRender.Should().BeNull();
    }

    [Fact]
    public void ShouldAddNestedStyles()
    {
        StyleBuilder child = StyleBuilder.Empty()
            .AddStyle("background-color", "DodgerBlue")
            .AddStyle("padding", "35px");

        string styleToRender = StyleBuilder.Empty()
            .AddStyle(child)
            .AddStyle("z-index", "-1")
            .Build();

        /// Double ;; is valid HTML.
        /// The CSS syntax allows for empty declarations, which means that you can add leading and trailing semicolons as you like. For instance, this is valid CSS
        /// .foo { ;;;display:none;;;color:black;;; }
        /// Trimming is possible, but is it worth the operations for a non-issue?
        styleToRender.Should().Be("background-color:DodgerBlue;padding:35px;z-index:-1;");

    }

    [Fact]
    public void ShouldAddComplexStyles()
    {
        string styleToRender = StyleBuilder.Empty()
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
        styleToRender.Should().Be("text-decoration:underline line-through;z-index:-1;");
    }

    [Fact]
    public void ShouldBuildStyleWithFunc()
    {
        //arrange
        // Simulates Razor Components attribute splatting feature
        IReadOnlyDictionary<string, object> attributes = new Dictionary<string, object> { { "class", "my-custom-class-1" } };

        //act
        string styleToRender = StyleBuilder.Empty()
                        .AddStyle("background-color", () => attributes["style"].ToString(), when: attributes.ContainsKey("style"))
                        .AddStyle("background-color", "black")
                        .Build();
        //assert
        styleToRender.Should().Be("background-color:black;");
    }
}
