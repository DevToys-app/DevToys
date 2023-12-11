using DevToys.MacOS.Core.Helpers;
using Microsoft.AspNetCore.Components;

namespace DevToys.MacOS.Controls;

internal sealed class AppKitDispatcher : Dispatcher
{
    public override bool CheckAccess()
    {
        return NSThread.IsMain;
    }

    public override Task InvokeAsync(Action workItem)
    {
        return ThreadHelper.RunOnUIThreadAsync(workItem);
    }

    public override Task InvokeAsync(Func<Task> workItem)
    {
        return ThreadHelper.RunOnUIThreadAsync(workItem);
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
    {
        return ThreadHelper.RunOnUIThreadAsync(workItem);
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
    {
        return ThreadHelper.RunOnUIThreadAsync(workItem);
    }
}
