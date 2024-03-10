using System.Reflection;

namespace DevToys.Core;

public static class AppHelper
{
    /// <summary>
    /// Indicates whether the current instance of the app is a preview/beta version.
    /// </summary>
    public static readonly Lazy<bool> IsPreviewVersion = new(() =>
    {
        var assemblyInformationalVersion
            = (AssemblyInformationalVersionAttribute)
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute))!;
        return assemblyInformationalVersion.InformationalVersion.Contains("pre", StringComparison.CurrentCultureIgnoreCase);
    });
}
