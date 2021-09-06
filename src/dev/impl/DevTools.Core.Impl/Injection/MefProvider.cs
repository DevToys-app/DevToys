#nullable enable

using DevTools.Core.Injection;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;

namespace DevTools.Core.Impl.Injection
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
