using DevTools.Core.Impl.Injection;
using DevTools.Core.Injection;
using DevTools.Localization;
using DevTools.Providers;
using System;
using System.Globalization;

namespace DevTools.Tests
{
    public abstract class MefBaseTest : IDisposable
    {
        private readonly MefComposer _mefComposer;

        private bool _isDisposed;

        protected IMefProvider ExportProvider { get; }

        public MefBaseTest()
        {
            // Do all the tests in English.
            LanguageManager.Instance.SetCurrentCulture(new CultureInfo("en"));

            _mefComposer
                = new MefComposer(
                    typeof(MefComposer).Assembly,
                    typeof(IToolProvider).Assembly,
                    typeof(DevTools.Providers.Impl.Dummy).Assembly,
                    typeof(Impl.Dummy).Assembly);

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
