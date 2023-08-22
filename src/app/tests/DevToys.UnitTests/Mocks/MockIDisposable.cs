namespace DevToys.UnitTests.Mocks;

internal class MockIDisposable : IDisposable
{
    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    ~MockIDisposable()
    {
        Dispose(false);
    }
}
