using System.ComponentModel.Composition.Hosting;
using DevToys.Api;

namespace DevToys.Core.Mef;

[Export(typeof(IMefProvider))]
internal sealed class MefProvider : IMefProvider
{
    internal ExportProvider? ExportProvider { get; set; }

    public TExport Import<TExport>()
    {
        return ExportProvider!.GetExport<TExport>()!.Value;
    }

    public IEnumerable<Lazy<TExport, TMetadataView>> ImportMany<TExport, TMetadataView>()
    {
        return ExportProvider!.GetExports<TExport, TMetadataView>();
    }
}
