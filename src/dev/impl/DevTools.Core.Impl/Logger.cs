#nullable enable

using DevTools.Core.Threading;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;

namespace DevTools.Core.Impl
{
    [Export(typeof(ILogger))]
    internal sealed class Logger : ILogger
    {
        private const string LogFileName = "logs.txt";

        private readonly static CoreDispatcher _uiDispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

        private readonly DisposableSempahore _semaphore = new DisposableSempahore();

        public async Task OpenLogsAsync()
        {
            StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;

            IStorageItem file = await localCacheFolder.TryGetItemAsync(LogFileName);
            if (file is not null and IStorageFile storageFile)
            {
                await _uiDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await Launcher.LaunchFileAsync(storageFile);
                });
            }
        }

        public void LogFault(string featureName, Exception ex, string? message = null)
        {
            lock (_semaphore)
            {
                LogFaultAsync(featureName, ex, message).ForgetSafely();
            }
        }

        private async Task LogFaultAsync(string featureName, Exception ex, string? message)
        {
            await TaskScheduler.Default;

            try
            {
                using (await _semaphore.WaitAsync(CancellationToken.None))
                {
                    StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;

                    StorageFile logFile = await localCacheFolder.CreateFileAsync(LogFileName, CreationCollisionOption.OpenIfExists);

                    var logsLine = new List<string>
                    {
                        $"# - [{DateTime.Now.ToString("G", DateTimeFormatInfo.InvariantInfo)}]",
                        $"Feature name: {featureName}",
                        $"Custom message: {message}",
                        $"Exception message: {ex.Message}",
                        $"Exception stack trace:",
                        ex.StackTrace,
                        string.Empty // empty line
                    };

                    await FileIO.AppendLinesAsync(logFile, logsLine);
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
