using DevToys.Tools.Helpers;

namespace DevToys.UnitTests.Tools.Helpers;

public class PasswordGeneratorHelperTests
{
    [Theory]
    [InlineData(1, true, false, false, false, null)]
    [InlineData(1, false, true, false, false, null)]
    [InlineData(1, false, false, true, false, null)]
    [InlineData(1, false, false, false, true, null)]
    [InlineData(100, true, true, true, true, null)]
    [InlineData(100, true, true, true, true, "bcdefghijklmnopqrstuvwxyz")]
    [InlineData(100, false, true, false, false, "bcdefghijklmnopqrstuvwxyz")]
    internal void GeneratePassword(int length, bool hasUppercase, bool hasLowercase, bool hasNumber, bool hasSpecialCharacters, string? excludedCharacters)
    {
        string password
            = PasswordGeneratorHelper.GeneratePassword(
                length,
                hasUppercase,
                hasLowercase,
                hasNumber,
                hasSpecialCharacters,
                excludedCharacters?.ToCharArray());

        password.Should().NotBeNullOrEmpty();
        password.Length.Should().Be(length);

        if (hasUppercase)
        {
            ContainAny(password, PasswordGeneratorHelper.UppercaseLetters);
        }
        else
        {
            NotContainAny(password, PasswordGeneratorHelper.UppercaseLetters);
        }

        if (hasLowercase)
        {
            ContainAny(password, PasswordGeneratorHelper.LowercaseLetters);
        }
        else
        {
            NotContainAny(password, PasswordGeneratorHelper.LowercaseLetters);
        }

        if (hasNumber)
        {
            ContainAny(password, PasswordGeneratorHelper.Digits);
        }
        else
        {
            NotContainAny(password, PasswordGeneratorHelper.Digits);
        }

        if (hasSpecialCharacters)
        {
            ContainAny(password, PasswordGeneratorHelper.NonAlphanumeric);
        }
        else
        {
            NotContainAny(password, PasswordGeneratorHelper.NonAlphanumeric);
        }

        if (excludedCharacters != null)
        {
            NotContainAny(password, excludedCharacters);
        }
    }

    private static void ContainAny(string password, string characters)
    {
        characters.Should().ContainAny(password.ToCharArray().Select(c => c.ToString()));
    }

    private static void NotContainAny(string password, string characters)
    {
        characters.Should().NotContainAny(password.ToCharArray().Select(c => c.ToString()));
    }
}
