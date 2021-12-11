#nullable enable

using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using DevToys.Core.Threading;
using DevToys.Shared.Core;
using DevToys.Shared.Core.Threading;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DevToys.ViewModels.Tools
{
    internal abstract class ToolProviderBase
    {
        protected const string AssetsFolderPath = "ms-appx:///Assets/";

        protected TaskCompletionNotifier<IconElement> CreatePathIconFromPath(string resourceName)
        {
            Arguments.NotNullOrWhiteSpace(resourceName, nameof(resourceName));

            return new TaskCompletionNotifier<IconElement>(() =>
                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    string? pathMarkup = Application.Current.Resources[resourceName] as string;
                    Assumes.NotNullOrWhiteSpace(pathMarkup, nameof(pathMarkup));
                    return Task.FromResult<IconElement>(new PathIcon
                    {
                        Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathMarkup)
                    });
                }));
        }

        protected TaskCompletionNotifier<IconElement> CreateFontIcon(string glyph)
        {
            return new TaskCompletionNotifier<IconElement>(() =>
                ThreadHelper.RunOnUIThreadAsync(ThreadPriority.Low, () =>
                {
                    return Task.FromResult<IconElement>(
                        new FontIcon
                        {
                            Glyph = Arguments.NotNullOrWhiteSpace(glyph, nameof(glyph))
                        });
                }));
        }

        protected TaskCompletionNotifier<IconElement> CreateSvgIcon(string iconFileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Arguments.NotNullOrWhiteSpace(iconFileName, nameof(iconFileName));

            var windowFrame = ((Frame)Window.Current.Content);

            var result
                = new TaskCompletionNotifier<IconElement>(
                    () =>
                    {
                        ElementTheme actualTheme = windowFrame.ActualTheme;
                        return Task.Run(async () =>
                        {
                            await TaskScheduler.Default;

                            StorageFolder installationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

                            IStorageItem file = await installationFolder.TryGetItemAsync("Assets\\Icons\\" + iconFileName);

                            string svgFileContent = File.ReadAllText(file.Path);

                            if (actualTheme == ElementTheme.Dark)
                            {
                                svgFileContent = svgFileContent.Replace("#FF000000", "#FFFFFFFF").Replace("#000000", "#FFFFFF");
                            }
                            else
                            {
                                svgFileContent = svgFileContent.Replace("#ffffff", "#000000").Replace("#FFFFFFFF", "#000000");
                            }

                            return await ThreadHelper.RunOnUIThreadAsync(ThreadPriority.High, async () =>
                            {
                                var svgSource = new SvgImageSource();

                                using (Stream stream = GenerateStreamFromString(svgFileContent))
                                {
                                    await svgSource.SetSourceAsync(stream.AsRandomAccessStream());
                                }

                                var imageIcon = new ImageIcon
                                {
                                    Source = svgSource
                                };

                                imageIcon.SizeChanged
                                    += async (sender, args) =>
                                    {
                                        Size newSize = args.NewSize;
                                        FrameworkElement? parent = FindParentWithSmallerSize(imageIcon, new Vector2((float)newSize.Width, (float)newSize.Height));
                                        if (parent is not null)
                                        {
                                            newSize = parent.ActualSize.ToSize();
                                        }

                                        svgSource = new SvgImageSource();
                                        svgSource.RasterizePixelHeight = newSize.Height;
                                        svgSource.RasterizePixelWidth = newSize.Width;

                                        using (Stream stream = GenerateStreamFromString(svgFileContent))
                                        {
                                            await svgSource.SetSourceAsync(stream.AsRandomAccessStream());
                                        }

                                        imageIcon.Source = svgSource; // re-set it to update the image.
                                    };

                                return (IconElement)imageIcon;
                            });
                        });
                    });

            windowFrame.ActualThemeChanged
                += (sender, args) =>
                {
                    result.Reset();
                };

            return result;
        }

        private Stream GenerateStreamFromString(string input)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private FrameworkElement? FindParentWithSmallerSize(FrameworkElement origin, Vector2 currentSize)
        {
            if (VisualTreeHelper.GetParent(origin) is not FrameworkElement parent)
            {
                return null;
            }
            else if (parent.ActualSize.X >= currentSize.X && parent.ActualSize.Y >= currentSize.Y)
            {
                return FindParentWithSmallerSize(parent, currentSize);
            }

            return parent;
        }
    }
}
