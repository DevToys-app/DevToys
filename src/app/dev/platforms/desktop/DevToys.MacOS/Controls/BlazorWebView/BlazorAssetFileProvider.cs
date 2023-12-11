using System.Collections;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace DevToys.MacOS.Controls.BlazorWebView;

/// <summary>
/// A minimal implementation of an <see cref="IFileProvider"/> to be used by the <see cref="BlazorWkWebView"/> and <see cref="BlazorWebViewManager"/> types.
/// </summary>
internal sealed class BlazorAssetFileProvider : IFileProvider
{
    private readonly string _bundleRootDir;

    public BlazorAssetFileProvider(string contentRootDir)
    {
        _bundleRootDir = Path.Combine(NSBundle.MainBundle.ResourcePath, contentRootDir);
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
        => new BlazorAssetDirectoryContents(Path.Combine(_bundleRootDir, subpath));

    public IFileInfo GetFileInfo(string subpath)
        => new BlazorAssetFileInfo(Path.Combine(_bundleRootDir, subpath));

    public IChangeToken Watch(string filter)
        => NullChangeToken.Singleton;

    private sealed class BlazorAssetFileInfo : IFileInfo
    {
        private readonly string _filePath;

        internal BlazorAssetFileInfo(string filePath)
        {
            _filePath = filePath;

            Name = Path.GetFileName(_filePath);

            var fileInfo = new FileInfo(_filePath);
            Exists = fileInfo.Exists;
            Length = Exists ? fileInfo.Length : -1;
        }

        public bool Exists { get; }

        public long Length { get; }

        public string PhysicalPath => null!;

        public string Name { get; }

        public DateTimeOffset LastModified { get; } = DateTimeOffset.FromUnixTimeSeconds(0);

        public bool IsDirectory => false;

        public Stream CreateReadStream()
            => File.OpenRead(_filePath);
    }

    // This is never used by BlazorWebView or WebViewManager
    private sealed class BlazorAssetDirectoryContents : IDirectoryContents
    {
        internal BlazorAssetDirectoryContents(string filePath)
        {
        }

        public bool Exists => false;

        public IEnumerator<IFileInfo> GetEnumerator()
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator()
            => throw new NotImplementedException();
    }
}
