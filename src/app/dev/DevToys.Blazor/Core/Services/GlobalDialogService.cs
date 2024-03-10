namespace DevToys.Blazor.Core.Services;

public sealed class GlobalDialogService
{
    private readonly object _syncLock = new();

    internal bool IsDialogOpened { get; private set; }

    internal bool TryOpenDialog(out IDisposable? session)
    {
        lock (_syncLock)
        {
            if (IsDialogOpened)
            {
                // Only one dialog should open at the same time.
                session = null;
                return false;
            }

            IsDialogOpened = true;
            session = new DialogSession(this);
            return true;
        }
    }

    private class DialogSession : IDisposable
    {
        private readonly GlobalDialogService _dialogService;
        private bool _isDisposed;

        public DialogSession(GlobalDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public void Dispose()
        {
            lock (_dialogService._syncLock)
            {
                if (!_isDisposed)
                {
                    _isDisposed = true;
                    _dialogService.IsDialogOpened = false;
                }
            }
        }
    }
}
