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
        private readonly IThread _thread;

        [ImportingConstructor]
        public UriActivationProtocolService(IThread thread)
        {
            _thread = thread;
        }

        public async Task<bool> LaunchNewAppInstance(string? arguments = null)
        {
            return await _thread.RunOnUIThreadAsync(async () =>
            {
                try
                {
                    string uriToLaunch = Constants.UriActivationProtocolName;

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
                    // TODO: log this.
                }

                return false;
            });
        }
    }
}
