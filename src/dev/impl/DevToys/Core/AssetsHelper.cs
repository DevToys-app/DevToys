#nullable enable

using DevToys.Core.Threading;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DevToys.Core
{
    public static class AssetsHelper
    {
        public static async Task<string> GetReleaseNoteAsync()
        {
            string result = await GetLocalFileContentAsync($"Assets\\ReleaseNote.txt").ConfigureAwait(false);

            return result;
        }

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
                return File.ReadAllText(file.Path, Encoding.UTF8);
            }

            return string.Empty;
        }
    }
}
