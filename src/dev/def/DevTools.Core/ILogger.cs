using System;
using System.Threading.Tasks;

namespace DevTools.Core
{
    public interface ILogger
    {
        Task OpenLogsAsync();

        void LogFault(
            string featureName,
            Exception ex,
            string? message = null);
    }
}
