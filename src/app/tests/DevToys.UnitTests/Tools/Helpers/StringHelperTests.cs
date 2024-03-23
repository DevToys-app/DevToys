#nullable enable

using System.Threading;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

namespace DevToys.UnitTests.Tools.Helpers;

public class StringHelperTests
{
    private const string Text = "Lorem ipsum dolor sit amet takimata sit.\r\n Placerat sed duis clita nonumy tincidunt est facilisis.\r Cum ea elitr consectetuer nonumy diam.\n Eos ut diam laoreet amet minim dolor dolores dolore amet lorem consetetur dolor clita.";
    private const string Text2 = "Lorem ipsum dolor sit amet takimata sit.\r\nPlacerat sed duis clita nonumy tincidunt est facilisis.\rCum ea elitr consectetuer nonumy diam.\nEos ut diam laoreet amet minim dolor dolores dolore amet lorem consetetur dolor clita.";

    [Theory]
    [InlineData("", 0, 0, 1, 0)]
    [InlineData(Text, 0, 0, 1, 0)]
    [InlineData(Text, 1, 0, 1, 1)]
    [InlineData(Text, 2, 0, 1, 2)]
    [InlineData(Text, 2, 1, 1, 2)]
    [InlineData(Text, 51, 1, 2, 9)]
    [InlineData(Text, 49, 1, 2, 7)]
    internal void AnalyzeSpan(string input, int selectionStart, int length, int expectLine, int expectedColumn)
    {
        StringHelper.AnalyzeSpan(
            input,
            new TextSpan(selectionStart, length),
            CancellationToken.None,
            out int _,
            out int _,
            out int _,
            out int spanLineNumber,
            out int spanColumnNumber);

        spanLineNumber.Should().Be(expectLine);
        spanColumnNumber.Should().Be(expectedColumn);
    }

    [Theory]
    [InlineData("", 0, 0, 0, 0, 1, 1, EndOfLineSequence.Unknown)]
    [InlineData(Text, 226, 226, 35, 4, 1, 4, EndOfLineSequence.Mixed)]
    [InlineData("Lorem ipsum dolor sit amet takimata sit.", 40, 40, 7, 1, 1, 1, EndOfLineSequence.Unknown)]
    [InlineData("Lorem \nipsum \ndolor \nsit \namet takimata sit.", 44, 44, 7, 1, 1, 5, EndOfLineSequence.LineFeed)]
    [InlineData("Lorem \ripsum \rdolor \rsit \ramet takimata sit.", 44, 44, 7, 1, 1, 5, EndOfLineSequence.CarriageReturn)]
    [InlineData("Lorem \r\nipsum \r\ndolor \r\nsit \r\namet takimata sit.", 48, 48, 7, 1, 1, 5, EndOfLineSequence.CarriageReturnLineFeed)]
    [InlineData("Lorem \n\ripsum \n\rdolor \n\rsit \n\ramet takimata sit.", 48, 48, 7, 1, 5, 9, EndOfLineSequence.Mixed)]
    internal void AnalyzeText(
        string input,
        int expectedByteCount,
        int expectedCharacterCount,
        int expectedWordCount,
        int expectedSentenceCount,
        int expectedParagraphCount,
        int expectedLineCount,
        EndOfLineSequence expectedOverallLineBreakType)
    {
        StringHelper.AnalyzeText(
            input,
            CancellationToken.None,
            out int byteCount,
            out int characterCount,
            out int wordCount,
            out int sentenceCount,
            out int paragraphCount,
            out int lineCount,
            out EndOfLineSequence overallLineBreakType,
            out Dictionary<char, int>? _,
            out Dictionary<string, int>? _);

        byteCount.Should().Be(expectedByteCount);
        characterCount.Should().Be(expectedCharacterCount);
        wordCount.Should().Be(expectedWordCount);
        sentenceCount.Should().Be(expectedSentenceCount);
        paragraphCount.Should().Be(expectedParagraphCount);
        lineCount.Should().Be(expectedLineCount);
        overallLineBreakType.Should().Be(expectedOverallLineBreakType);
    }

    [Fact]
    internal void SentenceCase()
    {
        string result = StringHelper.ConvertToSentenceCase(Text, CancellationToken.None);
        result
        .Should()
        .Be("Lorem ipsum dolor sit amet takimata sit.\r\n Placerat sed duis clita nonumy tincidunt est facilisis.\r Cum ea elitr consectetuer nonumy diam.\n Eos ut diam laoreet amet minim dolor dolores dolore amet lorem consetetur dolor clita.");
    }

    [Fact]
    internal void TitleCase()
    {
        string result = StringHelper.ConvertToTitleCase(Text, CancellationToken.None);
        result
        .Should()
        .Be("Lorem Ipsum Dolor Sit Amet Takimata Sit.\r\n Placerat Sed Duis Clita Nonumy Tincidunt Est Facilisis.\r Cum Ea Elitr Consectetuer Nonumy Diam.\n Eos Ut Diam Laoreet Amet Minim Dolor Dolores Dolore Amet Lorem Consetetur Dolor Clita.");
    }

    [Fact]
    internal void PascalCase()
    {
        string result = StringHelper.ConvertToPascalCase(Text, CancellationToken.None);
        result
        .Should()
        .Be("LoremIpsumDolorSitAmetTakimataSit\r\nPlaceratSedDuisClitaNonumyTinciduntEstFacilisis\rCumEaElitrConsectetuerNonumyDiam\nEosUtDiamLaoreetAmetMinimDolorDoloresDoloreAmetLoremConseteturDolorClita");
    }

    [Fact]
    internal void SnakeCase()
    {
        string result = StringHelper.ConvertToSnakeCase(Text, CancellationToken.None);
        result
        .Should()
        .Be("lorem_ipsum_dolor_sit_amet_takimata_sit\r\nplacerat_sed_duis_clita_nonumy_tincidunt_est_facilisis\rcum_ea_elitr_consectetuer_nonumy_diam\neos_ut_diam_laoreet_amet_minim_dolor_dolores_dolore_amet_lorem_consetetur_dolor_clita");
    }

    [Fact]
    internal void ConstantCase()
    {
        string result = StringHelper.ConvertToConstantCase(Text, CancellationToken.None);
        result
        .Should()
        .Be("LOREM_IPSUM_DOLOR_SIT_AMET_TAKIMATA_SIT\r\nPLACERAT_SED_DUIS_CLITA_NONUMY_TINCIDUNT_EST_FACILISIS\rCUM_EA_ELITR_CONSECTETUER_NONUMY_DIAM\nEOS_UT_DIAM_LAOREET_AMET_MINIM_DOLOR_DOLORES_DOLORE_AMET_LOREM_CONSETETUR_DOLOR_CLITA");
    }

    [Fact]
    internal void KebabCase()
    {
        string result = StringHelper.ConvertToKebabCase(Text, CancellationToken.None);
        result
        .Should()
        .Be("lorem-ipsum-dolor-sit-amet-takimata-sit\r\nplacerat-sed-duis-clita-nonumy-tincidunt-est-facilisis\rcum-ea-elitr-consectetuer-nonumy-diam\neos-ut-diam-laoreet-amet-minim-dolor-dolores-dolore-amet-lorem-consetetur-dolor-clita");
    }

    [Fact]
    internal void CobolCase()
    {
        string result = StringHelper.ConvertToCobolCase(Text, CancellationToken.None);
        result
        .Should()
        .Be("LOREM-IPSUM-DOLOR-SIT-AMET-TAKIMATA-SIT\r\nPLACERAT-SED-DUIS-CLITA-NONUMY-TINCIDUNT-EST-FACILISIS\rCUM-EA-ELITR-CONSECTETUER-NONUMY-DIAM\nEOS-UT-DIAM-LAOREET-AMET-MINIM-DOLOR-DOLORES-DOLORE-AMET-LOREM-CONSETETUR-DOLOR-CLITA");
    }

    [Fact]
    internal void TrainCase()
    {
        string result = StringHelper.ConvertToTrainCase(Text, CancellationToken.None);
        result
        .Should()
        .Be("Lorem-Ipsum-Dolor-Sit-Amet-Takimata-Sit\r\nPlacerat-Sed-Duis-Clita-Nonumy-Tincidunt-Est-Facilisis\rCum-Ea-Elitr-Consectetuer-Nonumy-Diam\nEos-Ut-Diam-Laoreet-Amet-Minim-Dolor-Dolores-Dolore-Amet-Lorem-Consetetur-Dolor-Clita");
    }

    [Fact]
    internal void AlternatingCase()
    {
        string result = StringHelper.ConvertToAlternatingCase(Text, CancellationToken.None);
        result
        .Should()
        .Be("lOrEm iPsUm dOlOr sIt aMeT TaKiMaTa sIt.\r\n PlAcErAt sEd dUiS ClItA NoNuMy tInCiDuNt eSt fAcIlIsIs.\r cUm eA ElItR CoNsEcTeTuEr nOnUmY DiAm.\n eOs uT DiAm lAoReEt aMeT MiNiM DoLoR DoLoReS DoLoRe aMeT LoReM CoNsEtEtUr dOlOr cLiTa.");
    }

    [Fact]
    internal void InverseCase()
    {
        string result = StringHelper.ConvertToInverseCase(Text, CancellationToken.None);
        result
            .Should()
            .Be("LoReM IpSuM DoLoR SiT AmEt tAkImAtA SiT.\r\n pLaCeRaT SeD DuIs cLiTa nOnUmY TiNcIdUnT EsT FaCiLiSiS.\r CuM Ea eLiTr cOnSeCtEtUeR NoNuMy dIaM.\n EoS Ut dIaM LaOrEeT AmEt mInIm dOlOr dOlOrEs dOlOrE AmEt lOrEm cOnSeTeTuR DoLoR ClItA.");
    }

    [Fact]
    internal void RandomCase()
    {
        string result = StringHelper.ConvertToRandomCase(Text, CancellationToken.None);

        result.Should().BeEquivalentTo(Text);
        result.Should().NotBe(Text);
    }

    [Theory]
    [InlineData("", EndOfLineSequence.Unknown)]
    [InlineData(Text, EndOfLineSequence.Mixed)]
    [InlineData("Lorem ipsum dolor sit amet takimata sit.", EndOfLineSequence.Unknown)]
    [InlineData("Lorem \nipsum \ndolor \nsit \namet takimata sit.", EndOfLineSequence.LineFeed)]
    [InlineData("Lorem \ripsum \rdolor \rsit \ramet takimata sit.", EndOfLineSequence.CarriageReturn)]
    [InlineData("Lorem \r\nipsum \r\ndolor \r\nsit \r\namet takimata sit.", EndOfLineSequence.CarriageReturnLineFeed)]
    [InlineData("Lorem \n\ripsum \n\rdolor \n\rsit \n\ramet takimata sit.", EndOfLineSequence.Mixed)]
    internal void DetectLineBreakKind(string input, EndOfLineSequence expectedLineBreak)
    {
        EndOfLineSequence result = StringHelper.DetectLineBreakKind(input, CancellationToken.None);
        result
        .Should()
        .Be(expectedLineBreak);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(Text, "Lorem ipsum dolor sit amet takimata sit.\n Placerat sed duis clita nonumy tincidunt est facilisis.\n Cum ea elitr consectetuer nonumy diam.\n Eos ut diam laoreet amet minim dolor dolores dolore amet lorem consetetur dolor clita.")]
    [InlineData("Lorem ipsum dolor sit amet takimata sit.", "Lorem ipsum dolor sit amet takimata sit.")]
    [InlineData("Lorem \nipsum \ndolor \nsit \namet takimata sit.", "Lorem \nipsum \ndolor \nsit \namet takimata sit.")]
    [InlineData("Lorem \ripsum \rdolor \rsit \ramet takimata sit.", "Lorem \nipsum \ndolor \nsit \namet takimata sit.")]
    [InlineData("Lorem \r\nipsum \r\ndolor \r\nsit \r\namet takimata sit.", "Lorem \nipsum \ndolor \nsit \namet takimata sit.")]
    [InlineData("Lorem \n\ripsum \n\rdolor \n\rsit \n\ramet takimata sit.", "Lorem \n\nipsum \n\ndolor \n\nsit \n\namet takimata sit.")]
    internal void ConvertLineBreakToLF(string input, string expectedResult)
    {
        string result = StringHelper.ConvertLineBreakToLF(input);
        result
        .Should()
        .Be(expectedResult);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(Text, "Lorem ipsum dolor sit amet takimata sit.\r\n Placerat sed duis clita nonumy tincidunt est facilisis.\r\n Cum ea elitr consectetuer nonumy diam.\r\n Eos ut diam laoreet amet minim dolor dolores dolore amet lorem consetetur dolor clita.")]
    [InlineData("Lorem ipsum dolor sit amet takimata sit.", "Lorem ipsum dolor sit amet takimata sit.")]
    [InlineData("Lorem \nipsum \ndolor \nsit \namet takimata sit.", "Lorem \r\nipsum \r\ndolor \r\nsit \r\namet takimata sit.")]
    [InlineData("Lorem \ripsum \rdolor \rsit \ramet takimata sit.", "Lorem \r\nipsum \r\ndolor \r\nsit \r\namet takimata sit.")]
    [InlineData("Lorem \r\nipsum \r\ndolor \r\nsit \r\namet takimata sit.", "Lorem \r\nipsum \r\ndolor \r\nsit \r\namet takimata sit.")]
    [InlineData("Lorem \n\ripsum \n\rdolor \n\rsit \n\ramet takimata sit.", "Lorem \r\n\r\nipsum \r\n\r\ndolor \r\n\r\nsit \r\n\r\namet takimata sit.")]
    internal void ConvertLineBreakToCRLF(string input, string expectedResult)
    {
        string result = StringHelper.ConvertLineBreakToCRLF(input);
        result
        .Should()
        .Be(expectedResult);
    }

    [Fact]
    internal void SortLinesAlphabetically()
    {
        string result = StringHelper.SortLinesAlphabetically(Text2, EndOfLineSequence.CarriageReturnLineFeed);
        result
        .Should()
        .Be("Cum ea elitr consectetuer nonumy diam.\r\nEos ut diam laoreet amet minim dolor dolores dolore amet lorem consetetur dolor clita.\r\nLorem ipsum dolor sit amet takimata sit.\r\nPlacerat sed duis clita nonumy tincidunt est facilisis.");
    }

    [Fact]
    internal void SortLinesAlphabeticallyDescending()
    {
        string result = StringHelper.SortLinesAlphabeticallyDescending(Text2, EndOfLineSequence.CarriageReturnLineFeed);
        result
        .Should()
        .Be("Placerat sed duis clita nonumy tincidunt est facilisis.\r\nLorem ipsum dolor sit amet takimata sit.\r\nEos ut diam laoreet amet minim dolor dolores dolore amet lorem consetetur dolor clita.\r\nCum ea elitr consectetuer nonumy diam.");
    }

    [Fact]
    internal void SortLinesByLastWord()
    {
        string result = StringHelper.SortLinesByLastWord(Text2, EndOfLineSequence.CarriageReturnLineFeed);
        result
        .Should()
        .Be("Eos ut diam laoreet amet minim dolor dolores dolore amet lorem consetetur dolor clita.\r\nCum ea elitr consectetuer nonumy diam.\r\nPlacerat sed duis clita nonumy tincidunt est facilisis.\r\nLorem ipsum dolor sit amet takimata sit.");
    }

    [Fact]
    internal void SortLinesByLastWordDescending()
    {
        string result = StringHelper.SortLinesByLastWordDescending(Text2, EndOfLineSequence.CarriageReturnLineFeed);
        result
        .Should()
        .Be("Lorem ipsum dolor sit amet takimata sit.\r\nPlacerat sed duis clita nonumy tincidunt est facilisis.\r\nCum ea elitr consectetuer nonumy diam.\r\nEos ut diam laoreet amet minim dolor dolores dolore amet lorem consetetur dolor clita.");
    }

    [Fact]
    internal void ReverseLines()
    {
        string result = StringHelper.ReverseLines(Text2, EndOfLineSequence.CarriageReturnLineFeed);
        result
        .Should()
        .Be("Eos ut diam laoreet amet minim dolor dolores dolore amet lorem consetetur dolor clita.\r\nCum ea elitr consectetuer nonumy diam.\r\nPlacerat sed duis clita nonumy tincidunt est facilisis.\r\nLorem ipsum dolor sit amet takimata sit.");
    }

    [Fact]
    internal void ShuffleLines()
    {
        string result = StringHelper.ShuffleLines(Text2, EndOfLineSequence.CarriageReturnLineFeed);
        result
        .Should()
        .NotBe(Text2);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("hello world", "hello world")]
    [InlineData("hello\rworld", "hello\\rworld")]
    internal void EscapeString(string input, string expectedResult)
    {
        StringHelper.EscapeString(input, new MockILogger(), CancellationToken.None)
            .Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("hello world", "hello world")]
    [InlineData("hello\rworld", "hello\rworld")]
    [InlineData("hello\\rworld", "hello\rworld")]
    internal void UnescapeString(string input, string expectedResult)
    {
        StringHelper.UnescapeString(input, new MockILogger(), CancellationToken.None)
            .Data.Should().Be(expectedResult);
    }
}
