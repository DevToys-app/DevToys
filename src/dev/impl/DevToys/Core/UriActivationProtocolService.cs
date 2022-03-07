#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Shared.Core;
using DevToys.Shared.Core.Threading;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DevToys.Core
{
    [Export(typeof(IUriActivationProtocolService))]
    [Shared]
    internal sealed class UriActivationProtocolService : IUriActivationProtocolService
    {
        private readonly TileIconSizeDefinition[] ToolTileIconSizeDefinitions
            = new[]
            {
                new TileIconSizeDefinition("SmallTile.scale-100", size: 71, toolIconRatio: 3.5),
                new TileIconSizeDefinition("SmallTile.scale-125", size: 89, toolIconRatio: 3.5),
                new TileIconSizeDefinition("SmallTile.scale-150", size: 107, toolIconRatio: 3.5),
                new TileIconSizeDefinition("SmallTile.scale-200", size: 142, toolIconRatio: 3.5),
                new TileIconSizeDefinition("SmallTile.scale-400", size: 284, toolIconRatio: 3.5),
                new TileIconSizeDefinition("Square44x44Logo.scale-100", size: 44, toolIconRatio: 2.4),
                new TileIconSizeDefinition("Square44x44Logo.scale-125", size: 55, toolIconRatio: 2.5),
                new TileIconSizeDefinition("Square44x44Logo.scale-150", size: 66, toolIconRatio: 2.5),
                new TileIconSizeDefinition("Square44x44Logo.scale-200", size: 88, toolIconRatio: 2.5),
                new TileIconSizeDefinition("Square44x44Logo.scale-400", size: 176, toolIconRatio: 2.6),
                new TileIconSizeDefinition("Square150x150Logo.scale-100", size: 150, toolIconRatio: 6),
                new TileIconSizeDefinition("Square150x150Logo.scale-125", size: 188, toolIconRatio: 6),
                new TileIconSizeDefinition("Square150x150Logo.scale-150", size: 225, toolIconRatio: 6),
                new TileIconSizeDefinition("Square150x150Logo.scale-200", size: 300, toolIconRatio: 6),
                new TileIconSizeDefinition("Square150x150Logo.scale-400", size: 600, toolIconRatio: 6),
            };

        public async Task<bool> LaunchNewAppInstance(string? arguments = null)
        {
            return await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                string? uriToLaunch = GenerateLaunchUri(arguments);

                try
                {
                    var launchOptions
                        = new LauncherOptions
                        {
                            TargetApplicationPackageFamilyName = Package.Current.Id.FamilyName,
                        };

                    return
                        await Launcher.LaunchUriAsync(
                            new Uri(uriToLaunch.ToLower(CultureInfo.CurrentCulture)),
                            launchOptions);
                }
                catch (Exception ex)
                {
                    Logger.LogFault("Launch new app instance", ex, $"Launch URI: {uriToLaunch}");
                }

                return false;
            });
        }

        public async Task<bool> PinToolToStart(ToolProviderViewItem toolProvider)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var tileIconGenerationTasks = new List<Task<TileIconSizeDefinition?>>();
                for (int i = 0; i < ToolTileIconSizeDefinitions.Length; i++)
                {
                    TileIconSizeDefinition iconDefinition = ToolTileIconSizeDefinitions[i];
                    tileIconGenerationTasks.Add(GenerateCustomTileIconAsync(iconDefinition, toolProvider));
                }

                await Task.WhenAll(tileIconGenerationTasks).ConfigureAwait(true);

                string? smallTileIconName
                    = tileIconGenerationTasks
                    .Where(t => t.Result is not null && t.Result.IconName.StartsWith("SmallTile", StringComparison.Ordinal))
                    .OrderBy(t => t.Result!.Size)
                    .FirstOrDefault()?.Result!.IconName;

                string? square44IconName
                    = tileIconGenerationTasks
                    .Where(t => t.Result is not null && t.Result.IconName.StartsWith("Square44x44Logo", StringComparison.Ordinal))
                    .OrderBy(t => t.Result!.Size)
                    .FirstOrDefault()?.Result!.IconName;

                string? square150IconName
                    = tileIconGenerationTasks
                    .Where(t => t.Result is not null && t.Result.IconName.StartsWith("Square150x150Logo", StringComparison.Ordinal))
                    .OrderBy(t => t.Result!.Size)
                    .FirstOrDefault()?.Result!.IconName;

                if (string.IsNullOrEmpty(smallTileIconName) || string.IsNullOrEmpty(square44IconName) || string.IsNullOrEmpty(square150IconName))
                {
                    Logger.Log("Pin to start", "Unable to generate one of the required tile icon.");
                }

                var tile = new SecondaryTile(
                    tileId: toolProvider.Metadata.ProtocolName)
                {
                    DisplayName = toolProvider.ToolProvider.SearchDisplayName,
                    Arguments = GenerateLaunchArguments(toolProvider.Metadata.ProtocolName),
                    RoamingEnabled = false
                };
                tile.VisualElements.ShowNameOnSquare150x150Logo = true;
                tile.VisualElements.Square150x150Logo = new Uri($"ms-appdata:///local/{toolProvider.Metadata.ProtocolName}/{square150IconName}.png");
                tile.VisualElements.Square44x44Logo = new Uri($"ms-appdata:///local/{toolProvider.Metadata.ProtocolName}/{square44IconName}.png");
                tile.VisualElements.Square71x71Logo = new Uri($"ms-appdata:///local/{toolProvider.Metadata.ProtocolName}/{smallTileIconName}.png");

                await tile.RequestCreateForSelectionAsync(Window.Current.Bounds);
            }
            catch (Exception ex)
            {
                Logger.LogFault("Pin to start", ex);
                return false;
            }

            return true;
        }

        private string GenerateLaunchArguments(string? toolProtocol)
        {
            string arguments = string.Empty;

            if (!string.IsNullOrWhiteSpace(toolProtocol))
            {
                arguments += $"{Constants.UriActivationProtocolToolArgument}={toolProtocol}";
            }

            return arguments;
        }

        private string GenerateLaunchUri(string? toolProtocol)
        {
            string? uriToLaunch = Constants.UriActivationProtocolName;

            string arguments = GenerateLaunchArguments(toolProtocol);
            if (!string.IsNullOrEmpty(arguments))
            {
                uriToLaunch += "?" + arguments;
            }

            return uriToLaunch;
        }

        private async Task<TileIconSizeDefinition?> GenerateCustomTileIconAsync(TileIconSizeDefinition tileIconSizeDefinition, ToolProviderViewItem toolProvider)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                /*
                 * The code below generates the following equivalent:
                 *     <Canvas HorizontalAlignment="Left" VerticalAlignment="Top">
                 *         <Grid>
                 *             <Image Source="image.scale-100.png" Grid.ColumnSpan="3" Grid.RowSpan="3"/>
                 *             <Viewbox HorizontalAlignment="Center" VerticalAlignment="Center">
                 *                 <IconElement/>
                 *             </Viewbox>
                 *         </Grid>
                 *     </Canvas>
                 */

                StorageFolder installationFolder = Package.Current.InstalledLocation;
                if (!await installationFolder.FileExistsAsync($"Assets\\TileTemplate\\{tileIconSizeDefinition.IconName}.png", isRecursive: false))
                {
                    // Tile template file isn't installed.
                    return null;
                }

                StorageFile? backgroundIconImageFile = await installationFolder.GetFileAsync($"Assets\\TileTemplate\\{tileIconSizeDefinition.IconName}.png");

                using (IRandomAccessStream fileStream = await backgroundIconImageFile.OpenAsync(FileAccessMode.Read, StorageOpenOptions.AllowOnlyReaders))
                {
                    var backgroundIconImageSource = new BitmapImage
                    {
                        DecodePixelWidth = tileIconSizeDefinition.Size,
                        DecodePixelHeight = tileIconSizeDefinition.Size
                    };
                    await backgroundIconImageSource.SetSourceAsync(fileStream);

                    var container = new Grid()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Height = tileIconSizeDefinition.Size,
                        Width = tileIconSizeDefinition.Size,
                        MaxHeight = tileIconSizeDefinition.Size,
                        MaxWidth = tileIconSizeDefinition.Size,
                        Background = new SolidColorBrush(Colors.Transparent),
                        Margin = new Thickness(-1 * tileIconSizeDefinition.Size, -1 * tileIconSizeDefinition.Size, 0, 0),
                        RequestedTheme = ElementTheme.Dark
                    };

                    var backgroundIcon = new Image
                    {
                        Height = tileIconSizeDefinition.Size,
                        Width = tileIconSizeDefinition.Size,
                        Source = backgroundIconImageSource,
                        Stretch = Stretch.UniformToFill
                    };

                    IconElement toolIcon = await toolProvider.Icon.Task!.ConfigureAwait(true);
                    Assumes.NotNull(toolIcon, nameof(toolIcon));

                    toolIcon.Height = tileIconSizeDefinition.Size / tileIconSizeDefinition.ToolIconRatio;
                    toolIcon.Width = tileIconSizeDefinition.Size / tileIconSizeDefinition.ToolIconRatio;

                    var toolIconViewBox = new Viewbox
                    {
                        Height = tileIconSizeDefinition.Size / tileIconSizeDefinition.ToolIconRatio,
                        Width = tileIconSizeDefinition.Size / tileIconSizeDefinition.ToolIconRatio,
                        Margin = new Thickness(0, 3, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Child = toolIcon
                    };

                    container.Children.Add(backgroundIcon);
                    container.Children.Add(toolIconViewBox);

                    ((Grid)((Page)((Frame)Window.Current.Content).Content).Content).Children.Insert(0, container);

                    container.UpdateLayout();

                    var iconSizeUpdatedTask = new TaskCompletionSource<object?>();
                    long registrationToken = 0;

                    var imageIcon = toolIcon as ImageIcon;
                    if (imageIcon is not null)
                    {
                        registrationToken = imageIcon.RegisterPropertyChangedCallback(ImageIcon.SourceProperty, (s, e) =>
                        {
                            iconSizeUpdatedTask.TrySetResult(null);
                        });
                    }
                    else
                    {
                        iconSizeUpdatedTask.TrySetResult(null);
                    }

                    // Wait that the icon updates its size and maybe its color theme.
                    await Task.WhenAny(iconSizeUpdatedTask.Task, Task.Delay(500)).ConfigureAwait(true);

                    if (imageIcon is not null)
                    {
                        imageIcon.UnregisterPropertyChangedCallback(ImageIcon.SourceProperty, registrationToken);
                    }

                    ThreadHelper.ThrowIfNotOnUIThread();

                    // Create an image from the canvas.
                    var resultBitmap = new RenderTargetBitmap();
                    await resultBitmap.RenderAsync(container);

                    ((Grid)((Page)((Frame)Window.Current.Content).Content).Content).Children.Remove(container);

                    ThreadHelper.ThrowIfNotOnUIThread();

                    // Save the image on the hard drive.
                    IBuffer pixelBuffer = await resultBitmap.GetPixelsAsync();
                    byte[] pixels = pixelBuffer.ToArray();
                    var displayInformation = DisplayInformation.GetForCurrentView();
                    StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{toolProvider.Metadata.ProtocolName}\\{tileIconSizeDefinition.IconName}.png", CreationCollisionOption.ReplaceExisting);

                    ThreadHelper.ThrowIfNotOnUIThread();

                    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                        encoder.SetPixelData(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Straight,
                            (uint)resultBitmap.PixelWidth,
                            (uint)resultBitmap.PixelHeight,
                            displayInformation.RawDpiX,
                            displayInformation.RawDpiY,
                            pixels);

                        await encoder.FlushAsync();
                    }
                }

                return tileIconSizeDefinition;
            }
            catch (Exception ex)
            {
                Logger.LogFault("Generate tile icon", ex, $"Target size: {tileIconSizeDefinition.Size}; Tool icon ratio: {tileIconSizeDefinition.ToolIconRatio}; Input file name: {tileIconSizeDefinition.IconName}");
            }

            return null;
        }
    }
}
