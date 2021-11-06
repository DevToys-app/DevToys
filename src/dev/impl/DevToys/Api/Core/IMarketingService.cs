#nullable enable

using System.Threading.Tasks;

namespace DevToys.Api.Core
{
    /// <summary>
    /// Provides a service that help to generate positive review of the DevToys app.
    /// </summary>
    /// <remarks>
    /// This service should be called when the app started, crashed, successfuly performed a task.
    /// By monitoring these events, the service will try to decide of the most ideal moment
    /// for proposing to the user to share constructive feedback to the developer.
    /// </remarks>
    public interface IMarketingService
    {
        Task NotifyAppEncounteredAProblemAsync();

        void NotifyToolSuccessfullyWorked();

        void NotifyAppJustUpdated();

        void NotifyAppStarted();

        void NotifySmartDetectionWorked();
    }
}
