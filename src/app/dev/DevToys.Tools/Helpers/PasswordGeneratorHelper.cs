using System.Text;
using DevToys.Tools.Helpers.Core;

namespace DevToys.Tools.Helpers;

internal static class PasswordGeneratorHelper
{
    /// <summary>
    /// All non-alphanumeric characters.
    /// </summary>
    internal const string NonAlphanumeric = "!@#$%^&*";

    /// <summary>
    /// All lower case ASCII characters.
    /// </summary>
    internal const string LowercaseLetters = "abcdefghijkmnopqrstuvwxyz";

    /// <summary>
    /// All upper case ASCII characters.
    /// </summary>
    internal const string UppercaseLetters = "ABCDEFGHJKLMNOPQRSTUVWXYZ";

    /// <summary>
    /// All digits.
    /// </summary>
    internal const string Digits = "0123456789";

    internal static string GeneratePassword(
        int length,
        bool hasUppercase,
        bool hasLowercase,
        bool hasNumbers,
        bool hasSpecialCharacters,
        char[]? excludedCharacters)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

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
        }

        if (hasLowercase)
        {
            randomChars[1] = RemoveExcludedCharacters(LowercaseLetters, excludedCharacters);
        }

        if (hasNumbers)
        {
            randomChars[2] = RemoveExcludedCharacters(Digits, excludedCharacters);
        }

        if (hasSpecialCharacters)
        {
            randomChars[3] = RemoveExcludedCharacters(NonAlphanumeric, excludedCharacters);
        }

        randomChars = randomChars.Where(r => r.Length > 0).ToArray();

        // Only continue if the user hasn't excluded everything.
        if (randomChars.Length != 0)
        {
            for (int j = 0; j < length; j++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                newPasswordCharacters.Insert(rand.Next(0, newPasswordCharacters.Count), rcs[rand.Next(0, rcs.Length)]);
            }
        }

        return new string(newPasswordCharacters.ToArray());
    }

    private static string RemoveExcludedCharacters(string input, char[]? excludedCharacters)
    {
        if (excludedCharacters == null || excludedCharacters.Length == 0)
        {
            return input;
        }

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
