using System.Text;
using DevToys.Tools.Helpers.Core;

namespace DevToys.Tools.Helpers;

internal static class PasswordGeneratorHelper
{
    /// <summary>
    /// All non-alphanumeric characters.
    /// </summary>
    private const string NonAlphanumeric = "!@#$%^&*";

    /// <summary>
    /// All lower case ASCII characters.
    /// </summary>
    private const string LowercaseLetters = "abcdefghijkmnopqrstuvwxyz";

    /// <summary>
    /// All upper case ASCII characters.
    /// </summary>
    private const string UppercaseLetters = "ABCDEFGHJKLMNOPQRSTUVWXYZ";

    /// <summary>
    /// All digits.
    /// </summary>
    private const string Digits = "0123456789";

    internal static string GeneratePassword(
        int length,
        bool hasUppercase,
        bool hasLowercase,
        bool hasNumbers,
        bool hasSpecialCharacters,
        char[] excludedCharacters)
    {
        // Combine all character sets together.
        string[] randomChars = new[] {
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty
                };

        var rand = new CryptoRandom();
        var newPasswordCharacters = new List<char>();

        if (hasUppercase)
        {
            randomChars[0] = RemoveExcludedCharacters(UppercaseLetters, excludedCharacters);

            // If the whole set gets excluded don't include it.
            if (randomChars[0].Length != 0)
            {
                newPasswordCharacters.Insert(rand.Next(0, newPasswordCharacters.Count), randomChars[0][rand.Next(0, randomChars[0].Length)]);
            }
        }

        if (hasLowercase)
        {
            randomChars[1] = RemoveExcludedCharacters(LowercaseLetters, excludedCharacters);
            if (randomChars[1].Length != 0)
            {
                newPasswordCharacters.Insert(rand.Next(0, newPasswordCharacters.Count), randomChars[1][rand.Next(0, randomChars[1].Length)]);
            }
        }

        if (hasNumbers)
        {
            randomChars[2] = RemoveExcludedCharacters(Digits, excludedCharacters);
            if (randomChars[2].Length != 0)
            {
                newPasswordCharacters.Insert(rand.Next(0, newPasswordCharacters.Count), randomChars[2][rand.Next(0, randomChars[2].Length)]);
            }
        }

        if (hasSpecialCharacters)
        {
            randomChars[3] = RemoveExcludedCharacters(NonAlphanumeric, excludedCharacters);
            if (randomChars[3].Length != 0)
            {
                newPasswordCharacters.Insert(rand.Next(0, newPasswordCharacters.Count), randomChars[3][rand.Next(0, randomChars[3].Length)]);
            }
        }

        randomChars = randomChars.Where(r => r.Length > 0).ToArray();

        // Only continue if the user hasn't excluded everything.
        if (randomChars.Length != 0)
        {
            for (int j = newPasswordCharacters.Count; j < length; j++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                newPasswordCharacters.Insert(rand.Next(0, newPasswordCharacters.Count), rcs[rand.Next(0, rcs.Length)]);
            }
        }

        return new string(newPasswordCharacters.ToArray());
    }

    private static string RemoveExcludedCharacters(string input, char[] excludedCharacters)
    {
        var excludedSet = new HashSet<char>(excludedCharacters); // HashSet provides a faster lookup than Array.Contains().
        var stringBuilder = new StringBuilder();

        foreach (char c in input)
        {
            if (!excludedSet.Contains(c))
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString();
    }
}
