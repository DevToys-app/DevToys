namespace DevToys.Api;

internal sealed class ThrottledProgress<T> : Progress<T>
{
    private readonly TimeSpan _dueTime;
    private readonly object _locker = new();
    private (T? Value, bool HasValue) _current;
    private Task? _task;

    public ThrottledProgress(Action<T> handler, TimeSpan dueTime)
        : base(handler)
    {
        if (dueTime < TimeSpan.Zero || dueTime.TotalMilliseconds > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dueTime));
        }

        _dueTime = dueTime;
    }

    protected override void OnReport(T value)
    {
        lock (_locker)
        {
            if (_task is null)
            {
                base.OnReport(value);
                _task = Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(_dueTime);
                        lock (_locker)
                        {
                            if (_current.HasValue)
                            {
                                base.OnReport(_current.Value!);
                                _current = (default, false);
                            }
                            else
                            {
                                _task = null;
                                break;
                            }
                        }
                    }
                });
            }
            else
            {
                _current = (value, true);
            }
        }
    }

    public void Flush()
    {
        lock (_locker)
        {
            if (_current.HasValue)
            {
                base.OnReport(_current.Value!);
                _current = (default, false);
            }
        }
    }
}
