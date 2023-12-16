using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Generators.Password;

[Export(typeof(ICommandLineTool))]
[Name("PasswordGenerator")]
[CommandName(
    Name = "password",
    Alias = "pwd",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Generators.Password.PasswordGenerator",
    DescriptionResourceName = nameof(PasswordGenerator.Description))]
internal sealed class PasswordGeneratorCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "length",
        Alias = "l",
        DescriptionResourceName = nameof(PasswordGenerator.LengthOptionDescription))]
    internal int Length { get; set; } = 30;

    [CommandLineOption(
        Name = "uppercase",
        Alias = "u",
        DescriptionResourceName = nameof(PasswordGenerator.UppercaseOptionDescription))]
    internal bool Uppercase { get; set; } = true;

    [CommandLineOption(
        Name = "lowercase",
        Alias = "m",
        DescriptionResourceName = nameof(PasswordGenerator.LowercaseOptionDescription))]
    internal bool Lowercase { get; set; } = true;

    [CommandLineOption(
        Name = "digits",
        Alias = "d",
        DescriptionResourceName = nameof(PasswordGenerator.DigitsOptionDescription))]
    internal bool Digits { get; set; } = true;

    [CommandLineOption(
        Name = "special",
        Alias = "s",
        DescriptionResourceName = nameof(PasswordGenerator.SpecialCharactersOptionDescription))]
    internal bool SpecialCharacters { get; set; } = true;

    [CommandLineOption(
        Name = "excluded",
        Alias = "e",
        DescriptionResourceName = nameof(PasswordGenerator.ExcludedCharactersOptionDescription))]
    internal string ExcludedCharacters { get; set; } = string.Empty;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        string password
            = PasswordGeneratorHelper.GeneratePassword(
                Length,
                Uppercase,
                Lowercase,
                Digits,
                SpecialCharacters,
                ExcludedCharacters.ToArray());

        Console.WriteLine(password);
        return ValueTask.FromResult(0);
    }
}
