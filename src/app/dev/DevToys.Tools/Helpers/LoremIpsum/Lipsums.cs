// Forked from https://github.com/alexcpendleton/NLipsum

using System.Reflection;

namespace DevToys.Tools.Helpers.LoremIpsum;

/// <summary>
/// All of these Lipsums are derived from the files at http://lipsum.sourceforge.net/
/// </summary>
internal static class Lipsums
{
    internal static string GetText(LipsumsCorpus lipsum)
    {
        return lipsum switch
        {
            LipsumsCorpus.ChildHarold => GetTextFromRawXml("childharold"),
            LipsumsCorpus.Decameron => GetTextFromRawXml("decameron"),
            LipsumsCorpus.Faust => GetTextFromRawXml("faust"),
            LipsumsCorpus.InDerFremde => GetTextFromRawXml("inderfremde"),
            LipsumsCorpus.LeBateauIvre => GetTextFromRawXml("lebateauivre"),
            LipsumsCorpus.LeMasque => GetTextFromRawXml("lemasque"),
            LipsumsCorpus.LoremIpsum => GetTextFromRawXml("loremipsum"),
            LipsumsCorpus.NagyonFaj => GetTextFromRawXml("nagyonfaj"),
            LipsumsCorpus.Omagyar => GetTextFromRawXml("omagyar"),
            LipsumsCorpus.RobinsonoKruso => GetTextFromRawXml("robinsonokruso"),
            LipsumsCorpus.TheRaven => GetTextFromRawXml("theraven"),
            LipsumsCorpus.TierrayLuna => GetTextFromRawXml("tierrayluna"),
            _ => throw new NotSupportedException(),
        };
    }

    private static string GetTextFromRawXml(string resourceName)
    {
        return LipsumUtilities.GetTextFromRawXml(ReadEmbeddedResource(resourceName)).ToString();
    }

    private static string ReadEmbeddedResource(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = $"DevToys.Tools.Assets.Lipsums.{name}.xml";

        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        using StreamReader reader = new(stream, System.Text.Encoding.UTF8);
        string text = reader.ReadToEnd();
        return text;
    }
}
