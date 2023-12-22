namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

/// <summary>
/// Defines the culture supported by a component.
/// </summary>
/// <remarks>
/// <para>
/// Set <see cref="CultureCode"/> to <see cref="SupportedCultures.Any"/> if the component is culture invariant.
/// </para>
/// </remarks>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CultureAttribute : Attribute
{
    public string CultureCode { get; }

    public CultureAttribute(string cultureCode)
    {
        CultureCode = cultureCode;
    }
}
