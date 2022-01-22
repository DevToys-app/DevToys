#nullable enable

namespace DevToys.Helpers
{
    public static class StorageFileHelper
    {
        public static readonly string[] SizesStrings
            = {
                LanguageManager.Instance.Common.Bytes,
                LanguageManager.Instance.Common.Kilobytes,
                LanguageManager.Instance.Common.Megabytes,
                LanguageManager.Instance.Common.Gigabytes,
                LanguageManager.Instance.Common.Terabytes
            };

        public static string HumanizeFileSize(double fileSize, string fileSizeDisplay)
        {
            int order = 0;
            while (fileSize >= 1024 && order < SizesStrings.Length - 1)
            {
                order++;
                fileSize /= 1024;
            }

            string fileSizeString = string.Format(fileSizeDisplay, fileSize, SizesStrings[order]);
            return fileSizeString;
        }
    }
}
