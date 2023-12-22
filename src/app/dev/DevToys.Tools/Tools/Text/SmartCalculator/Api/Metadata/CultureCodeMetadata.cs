namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

public class CultureCodeMetadata
{
    public string[] CultureCodes { get; set; }

    public CultureCodeMetadata(IDictionary<string, object> metadata)
    {
        if (metadata.TryGetValue(nameof(CultureAttribute.CultureCode), out object? value) && value is not null)
        {
            if (value is string culture)
                CultureCodes = new[] { culture };
            else if (value is string[] cultures)
            {
                CultureCodes = cultures;
            }
            else
            {
                ThrowHelper.ThrowInvalidDataException($"Unable to understand MEF's '{nameof(CultureAttribute.CultureCode)}' information.");
            }
        }
        else
        {
            CultureCodes = new[] { SupportedCultures.Any };
        }
    }
}
