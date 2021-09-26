#nullable enable

using DevToys.Api.Core.Injection;
using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Reflection;

namespace DevToys.Core
{
    /// <summary>
    /// Provides a set of methods to initialize and manage MEF.
    /// </summary>
    public sealed class MefComposer : IDisposable
    {
        private readonly Assembly[] _assemblies;
        private bool isExportProviderDisposed = true;

        public static IMefProvider Provider { get; private set; } = null!;

        public CompositionHost ExportProvider { get; private set; }

        public MefComposer(params Assembly[] assemblies)
        {
            if (Provider is not null)
            {
                throw new InvalidOperationException("Mef composer already initialized.");
            }

            _assemblies = assemblies;
            ExportProvider = InitializeMef();

            Provider = ExportProvider.GetExport<IMefProvider>();
            ((MefProvider)Provider).ExportProvider = ExportProvider;
        }

        public void Dispose()
        {
            if (ExportProvider is not null)
            {
                ExportProvider.Dispose();
            }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Provider = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            isExportProviderDisposed = true;
        }

        internal void Reset()
        {
            // For unit tests.
            Dispose();
            InitializeMef();
        }

        private CompositionHost InitializeMef()
        {
            if (!isExportProviderDisposed)
            {
                return ExportProvider;
            }

            var assemblies = new HashSet<Assembly>(_assemblies);

            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);

            ExportProvider = configuration.CreateContainer();

            isExportProviderDisposed = false;

            return ExportProvider;
        }
    }
}
