using DevToys.Tools.Helpers.LoremIpsum;

namespace DevToys.UnitTests.Tools.Helpers;

public class LipsumTests
{
    [Fact]
    public void LoadXml()
    {
        string template = "<root><text>{0}</text></root>";
        string expectedText = "Lorem ipsum dolor sit amet";
        string formatted = String.Format(template, expectedText);

        var lipsum = new LipsumGenerator(formatted, true);
        lipsum.LipsumText.ToString().Should().Be(expectedText);
    }

    [Fact]
    public void LoadPlainText()
    {
        string expectedText = "Lorem ipsum dolor sit amet";

        var lipsum = new LipsumGenerator(expectedText, false);
        lipsum.LipsumText.ToString().Should().Be(expectedText);
    }

    [Fact]
    public void DefaultConstructorContainsLoremIpsum()
    {
        string expected = Lipsums.GetText(LipsumsCorpus.LoremIpsum);
        var generator = new LipsumGenerator(LipsumsCorpus.LoremIpsum);
        generator.LipsumText.ToString().Should().Be(expected);
    }

    [Fact]
    public void PrepareWords()
    {
        string rawText = "lorem ipsum dolor sit amet consetetur";
        string[] expectedArray = new string[] {
                "lorem", "ipsum", "dolor", "sit", "amet", "consetetur"
            };
        int wordsInRawText = 6;

        var lipsum = new LipsumGenerator(rawText, false);
        IReadOnlyList<string> wordsPrepared = lipsum.PreparedWords;

        wordsPrepared.Should().HaveCount(wordsInRawText);
        wordsPrepared.Should().BeEquivalentTo(expectedArray);

    }

    [Fact]
    public void GenerateWords()
    {
        string rawText = "lorem ipsum dolor sit amet consetetur";

        var lipsum = new LipsumGenerator(rawText, false);

        int wordCount = 4;

        string[] generatedWords = lipsum.GenerateWords(wordCount);

        generatedWords.Should().HaveCount(wordCount);

        for (int i = 0; i < wordCount; i++)
        {
            rawText.Should().Contain(generatedWords[i]);
        }
    }

    [Fact]
    public void GenerateSentences()
    {
        string rawText = Lipsums.GetText(LipsumsCorpus.LoremIpsum);
        var lipsum = new LipsumGenerator(rawText, false);

        int desiredSentenceCount = 5;
        string[] generatedSentences = lipsum.
            GenerateSentences(desiredSentenceCount, Sentence.Medium);

        generatedSentences.Should().HaveCount(desiredSentenceCount);

        for (int i = 0; i < desiredSentenceCount; i++)
        {
            generatedSentences[i].Should().NotBeNull();
            generatedSentences[i].Should().NotBeEmpty();
        }
    }

    [Fact]
    public void SentenceCapitalizationAndPunctuation()
    {
        string rawText = "this";
        var lipsum = new LipsumGenerator(rawText, false);
        string[] generatedSentences = lipsum.GenerateSentences(1, new Sentence(1, 1));
        string desiredSentence = "This.";
        generatedSentences[0].Should().Be(desiredSentence);
    }

    [Fact]
    public void GenerateParagraphs()
    {
        string rawText = Lipsums.GetText(LipsumsCorpus.LoremIpsum);
        var lipsum = new LipsumGenerator(rawText, false);

        int desiredParagraphCount = 5;
        string[] generatedParagraphs = lipsum.GenerateParagraphs(desiredParagraphCount, Paragraph.Medium);

        generatedParagraphs.Should().HaveCount(desiredParagraphCount);

        for (int i = 0; i < desiredParagraphCount; i++)
        {
            generatedParagraphs[i].Should().NotBeNull();
            generatedParagraphs[i].Should().NotBeEmpty();
        }
    }

    [Fact]
    public void GenerateCharacters()
    {
        string rawText = "lorem ipsum dolor sit amet consetetur";
        int desiredCharacterCount = 10;
        string expectedText = rawText.Substring(0, desiredCharacterCount);

        var lipsum = new LipsumGenerator(rawText, false);

        string[] charsRetrieved = lipsum.GenerateCharacters(desiredCharacterCount);

        // This should only retrieve one string
        charsRetrieved.Should().HaveCount(1);

        string generatedString = charsRetrieved[0];
        generatedString.Should().NotBeNull();
        generatedString.Should().NotBeEmpty();

        generatedString.Should().Be(expectedText);
    }

    [Fact]
    public void RemoveEmptyElements()
    {
        string[] arrayWithEmpties = new string[] {
                "", "lorem", "ipsum", null, String.Empty, "xxx"
            };

        string[] expectedArray = new string[] {
                "lorem", "ipsum", "xxx"
            };

        int expectedLength = 3;

        IReadOnlyList<string> returnedArray = LipsumUtilities.RemoveEmptyElements(arrayWithEmpties);

        returnedArray.Should().HaveCount(expectedLength);
        returnedArray.Should().NotContain("");
        for (int i = 0; i < returnedArray.Count; i++)
        {
            returnedArray[i].Should().NotBeNull();
        }
        returnedArray.Should().BeEquivalentTo(expectedArray);
    }
}
