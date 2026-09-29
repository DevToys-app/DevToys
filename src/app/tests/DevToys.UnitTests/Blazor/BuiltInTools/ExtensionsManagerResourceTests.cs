using System.Globalization;
using DevToys.Blazor.BuiltInTools.ExtensionsManager;

namespace DevToys.UnitTests.Blazor.BuiltInTools;

/// <summary>
/// Regression tests for issue #1651: "Manage Extensions: Potential Missing Language Entry".
/// The "Install Extension" button text is fetched through <see cref="ExtensionsManager.InstallExtension"/>,
/// which reads the strongly-typed resource from the user's UI culture. When that culture is
/// en-GB, the resource value must not be empty — otherwise the button renders only its icon.
/// </summary>
public class ExtensionsManagerResourceTests
{
    [Fact]
    public void InstallExtension_ShouldNotBeEmpty_ForBritishEnglishCulture()
    {
        // Arrange
        CultureInfo originalCulture = ExtensionsManager.Culture;

        try
        {
            ExtensionsManager.Culture = CultureInfo.GetCultureInfo("en-GB");

            // Act
            string text = ExtensionsManager.InstallExtension;

            // Assert
            text.Should().NotBeNullOrWhiteSpace(
                "the 'Install Extension' button label must be visible for en-GB users (issue #1651).");
        }
        finally
        {
            ExtensionsManager.Culture = originalCulture;
        }
    }

    [Fact]
    public void InstallExtension_ShouldMatchNeutralResource_ForBritishEnglishCulture()
    {
        // Arrange
        CultureInfo originalCulture = ExtensionsManager.Culture;

        try
        {
            ExtensionsManager.Culture = CultureInfo.GetCultureInfo("en-GB");

            // Act
            string enGbText = ExtensionsManager.InstallExtension;

            ExtensionsManager.Culture = CultureInfo.InvariantCulture;

            string neutralText = ExtensionsManager.InstallExtension;

            // Assert
            enGbText.Should().NotBeNullOrWhiteSpace(
                "the 'Install Extension' button label must be visible for en-GB users (issue #1651).");

            enGbText.Should().Be(neutralText,
                "en-GB English is not expected to differ from the neutral resource for this label.");
        }
        finally
        {
            ExtensionsManager.Culture = originalCulture;
        }
    }
}
