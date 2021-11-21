using System;
using DevToys.Shared.Api.Core;
using DevToys.Shared.Core;
using DevToys.ViewModels;

namespace DevToys.Tests
{
    public abstract class MefBaseTest : IDisposable
    {
        private readonly MefComposer _mefComposer;

        private bool _isDisposed;

        protected IMefProvider ExportProvider { get; }

        public MefBaseTest()
        {
            // Do all the tests in English.
            LanguageManager.Instance.SetCurrentCulture(new LanguageDefinition("en-US"));

            _mefComposer
                = new MefComposer(
                    typeof(MainPageViewModel).Assembly,
                    typeof(MefComposer).Assembly);

            ExportProvider = _mefComposer.ExportProvider.GetExport<IMefProvider>();
        }

        ~MefBaseTest()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _mefComposer.Dispose();
            }

            _isDisposed = true;
        }
    }
}
