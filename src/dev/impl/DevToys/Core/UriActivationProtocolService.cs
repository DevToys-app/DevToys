#nullable enable

using System;
using System.Composition;
using System.Globalization;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Core.Threading;
using Windows.ApplicationModel;
using Windows.System;

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
                string? uriToLaunch = Constants.UriActivationProtocolName;

                try
                {

                    if (!string.IsNullOrWhiteSpace(arguments))
                    {
                        uriToLaunch += $"?{Constants.UriActivationProtocolToolArgument}={arguments}";
                    }

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
    }
}
