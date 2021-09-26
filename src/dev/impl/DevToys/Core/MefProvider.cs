#nullable enable

using DevToys.Api.Core.Injection;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;

namespace DevToys.Core
{
    [Export(typeof(IMefProvider))]
    [Shared]
    internal sealed class MefProvider : IMefProvider
    {
        internal CompositionHost? ExportProvider { get; set; }

        public TExport Import<TExport>()
        {
            return ExportProvider!.GetExport<TExport>();
        }

        public IEnumerable<TExport> ImportMany<TExport>()
        {
            return ExportProvider!.GetExports<TExport>();
        }
    }
}
