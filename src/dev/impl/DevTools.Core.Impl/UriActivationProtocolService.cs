#nullable enable

using DevTools.Core.Threading;
using System;
using System.Composition;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;

namespace DevTools.Core.Impl
{
    [Export(typeof(IUriActivationProtocolService))]
    [Shared]
    internal sealed class UriActivationProtocolService : IUriActivationProtocolService
    {
        private readonly ILogger _logger;
        private readonly IThread _thread;

        [ImportingConstructor]
        public UriActivationProtocolService(ILogger logger, IThread thread)
        {
            _logger = logger;
            _thread = thread;
        }

        public async Task<bool> LaunchNewAppInstance(string? arguments = null)
        {
            return await _thread.RunOnUIThreadAsync(async () =>
            {
                string uriToLaunch = Constants.UriActivationProtocolName;

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
                    _logger.LogFault("Launch new app instance", ex, $"Launch URI: {uriToLaunch}");
                }

                return false;
            });
        }
    }
}
