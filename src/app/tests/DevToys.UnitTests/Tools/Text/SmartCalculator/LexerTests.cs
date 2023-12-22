using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

namespace DevToys.UnitTests.Tools.Text.SmartCalculator;

public class LexerTests : MefBaseTest
{
    [Fact]
    public void TokenizeEmpty()
    {
        IReadOnlyList<LineInfo> lines = Analyze(string.Empty);
        Assert.Single(lines);
        Assert.Equal(0, lines[0].TokenCount);
    }

    [Fact]
    public void TokenizeWord()
    {
        IReadOnlyList<LineInfo> lines = Analyze(" a b c ");
        Assert.Single(lines);
        Assert.Equal(3, lines[0].TokenCount);
        Assert.True(lines[0].Tokens[0].IsOfType(PredefinedTokenAndDataTypeNames.Word));
        Assert.True(lines[0].Tokens[1].IsOfType(PredefinedTokenAndDataTypeNames.Word));
        Assert.True(lines[0].Tokens[2].IsOfType(PredefinedTokenAndDataTypeNames.Word));

        lines = Analyze("  abæçØ ");
        Assert.Single(lines);
        Assert.Single(lines[0].Tokens);
        Assert.True(lines[0].Tokens[0].IsOfType(PredefinedTokenAndDataTypeNames.Word));
        Assert.Equal(2, lines[0].Tokens[0].StartInLine);
        Assert.Equal(5, lines[0].Tokens[0].Length);

        Assert.True(lines[0].Tokens[0].IsTokenTextEqualTo("abæçØ", StringComparison.Ordinal));
        Assert.False(lines[0].Tokens[0].IsTokenTextEqualTo("a", StringComparison.Ordinal));
        Assert.False(lines[0].Tokens[0].IsTokenTextEqualTo("bæçØ", StringComparison.Ordinal));
        Assert.False(lines[0].Tokens[0].IsTokenTextEqualTo("baæçØ", StringComparison.Ordinal));
    }

    [Fact]
    public void TokenizeNumber()
    {
        IReadOnlyList<LineInfo> lines = Analyze(" 1 2 3 ");
        Assert.Single(lines);
        Assert.Equal(3, lines[0].Tokens.Count);
        Assert.True(lines[0].Tokens[0].IsOfType(PredefinedTokenAndDataTypeNames.Digit));
        Assert.True(lines[0].Tokens[1].IsOfType(PredefinedTokenAndDataTypeNames.Digit));
        Assert.True(lines[0].Tokens[2].IsOfType(PredefinedTokenAndDataTypeNames.Digit));

        lines = Analyze(" 12 ");
        Assert.Single(lines);
        Assert.Single(lines[0].Tokens);
        Assert.True(lines[0].Tokens[0].IsOfType(PredefinedTokenAndDataTypeNames.Digit));
        Assert.Equal(1, lines[0].Tokens[0].StartInLine);
        Assert.Equal(2, lines[0].Tokens[0].Length);
        Assert.Equal(2, lines[0].Tokens[0].Length);
    }

    [Fact]
    public void TokenizeWhitespace()
    {
        IReadOnlyList<LineInfo> lines = Analyze(" ");
        Assert.Single(lines);
        Assert.Equal(0, lines[0].Tokens.Count);

        lines = Analyze("  ");
        Assert.Single(lines);
        Assert.Equal(0, lines[0].Tokens.Count);

        lines = Analyze("   ");
        Assert.Single(lines);
        Assert.Equal(0, lines[0].Tokens.Count);

        lines = Analyze(" \t ");
        Assert.Single(lines);
        Assert.Equal(0, lines[0].Tokens.Count);
    }

    [Fact]
    public void TokenizePunctuationAndSymbols()
    {
        IReadOnlyList<LineInfo> lines = Analyze("!@$%^&_=`~[]{}\\|;:'\",.?");
        Assert.Single(lines);
        Assert.Equal(23, lines[0].Tokens.Count);
        for (int i = 0; i < lines[0].Tokens.Count; i++)
        {
            Assert.Equal(PredefinedTokenAndDataTypeNames.SymbolOrPunctuation, lines[0].Tokens[i].Type);
            Assert.Equal(1, lines[0].Tokens[i].Length);
        }

        lines = Analyze("π¿¾½¼»º¹¸¶µ´³²±°¯®­¬«ª©¨§¦¥¤£¢¡~}|{");
        Assert.Single(lines);
        Assert.Equal(35, lines[0].Tokens.Count);
        for (int i = 0; i < lines[0].Tokens.Count; i++)
        {
            Assert.Equal(PredefinedTokenAndDataTypeNames.SymbolOrPunctuation, lines[0].Tokens[i].Type);
            Assert.Equal(1, lines[0].Tokens[i].Length);
        }
    }

    [Fact]
    public void TokenizeMultipleLines()
    {
        IReadOnlyList<LineInfo> lines = Analyze("    \r\n\r\n\nabc\n  ");
        Assert.Equal(5, lines.Count);
        Assert.Equal(0, lines[0].TokenCount);
        Assert.Equal(0, lines[1].TokenCount);
        Assert.Equal(0, lines[2].TokenCount);
        Assert.True(lines[3].Tokens[0].IsOfType(PredefinedTokenAndDataTypeNames.Word));
        Assert.Equal(0, lines[4].TokenCount);
    }

    [Fact]
    public void TokenizeCustomTokens()
    {
        IReadOnlyList<LineInfo> lines = Analyze("True+False");
        Assert.Single(lines);
        Assert.Equal(3, lines[0].TokenCount);
        Assert.True(lines[0].Tokens[0].IsOfType(PredefinedTokenAndDataTypeNames.TrueIdentifier));
        Assert.True(lines[0].Tokens[1].IsOfType(PredefinedTokenAndDataTypeNames.AdditionOperator));
        Assert.True(lines[0].Tokens[2].IsOfType(PredefinedTokenAndDataTypeNames.FalseIdentifier));

        lines = Analyze("Truefalse");
        Assert.Single(lines);
        Assert.Equal(1, lines[0].TokenCount);
        Assert.True(lines[0].Tokens[0].IsOfType(PredefinedTokenAndDataTypeNames.Word));
    }

    private IReadOnlyList<LineInfo> Analyze(string input)
    {
        var lines = new List<LineInfo>();
        IReadOnlyList<TokenizedTextLine> tokenizedLines = ExportProvider.Import<ILexer>().Tokenize(SupportedCultures.English, input);

        for (int i = 0; i < tokenizedLines.Count; i++)
        {
            var tokens = new List<IToken>();

            LinkedToken linkedToken = tokenizedLines[i].Tokens;
            while (linkedToken is not null)
            {
                tokens.Add(linkedToken.Token);
                linkedToken = linkedToken.Next;
            }

            var lineInfo
                = new LineInfo
                {
                    LineNumber = i + 1,
                    Tokens = tokens,
                    TokenizedTextLine = tokenizedLines[i]
                };
            lines.Add(lineInfo);
        }

        return lines;
    }

    private class LineInfo
    {
        internal TokenizedTextLine TokenizedTextLine { get; set; }

        internal IReadOnlyList<IToken> Tokens { get; set; }

        internal int TokenCount => Tokens.Count;

        internal int LineNumber { get; set; }
    }
}
