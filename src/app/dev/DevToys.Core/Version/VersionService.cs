namespace DevToys.Core.Version;

[Export(typeof(IVersionService))]
internal sealed class VersionService : IVersionService
{
    public bool IsPreviewVersion()
    {
        return AppHelper.IsPreviewVersion.Value;
    }
}
