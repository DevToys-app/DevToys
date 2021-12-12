#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Core;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.StartScreen;

namespace DevToys.Core
{
    [Export(typeof(IUriActivationProtocolService))]
    [Shared]
    internal sealed class UriActivationProtocolService : IUriActivationProtocolService
    {
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

        public async Task<bool> PinToolToStart(MatchedToolProvider toolProvider)
        {
            try
            {
                var tile
                    = new SecondaryTile(
                        tileId: toolProvider.Metadata.ProtocolName,
                        displayName: toolProvider.ToolProvider.SearchDisplayName,
                        arguments: GenerateLaunchArguments(toolProvider.Metadata.ProtocolName),
                        new Uri("ms-appx:///Assets/Logo/Square150x150Logo.png"),
                        TileSize.Default);

                return await tile.RequestCreateAsync();
            }
            catch (Exception ex)
            {
                Logger.LogFault("Pin to start", ex);
            }

            return false;
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
    }
}
