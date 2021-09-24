#nullable enable

using DevTools.Core.Threading;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace DevTools.Common
{
    public static class AssetsHelper
    {
        public static async Task<string> GetPrivacyStatementAsync()
        {
            string result = await GetLocalFileContentAsync($"Assets\\PRIVACY-POLICY.md").ConfigureAwait(false);

            return result;
        }

        public static async Task<string> GetThirdPartyNoticesAsync()
        {
            string result = await GetLocalFileContentAsync($"Assets\\THIRD-PARTY-NOTICES.md").ConfigureAwait(false);

            return result;
        }

        public static async Task<string> GetLicenseAsync()
        {
            string result = await GetLocalFileContentAsync($"Assets\\LICENSE.md").ConfigureAwait(false);

            return result;
        }

        private static async Task<string> GetLocalFileContentAsync(string filePath)
        {
            await TaskScheduler.Default;

            StorageFolder installationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            IStorageItem file = await installationFolder.TryGetItemAsync(filePath);
            if (file != null)
            {
                return File.ReadAllText(file.Path);
            }

            return string.Empty;
        }
    }
}
