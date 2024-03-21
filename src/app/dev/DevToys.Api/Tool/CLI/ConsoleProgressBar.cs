using System.Text;

namespace DevToys.Api;

/// <summary>
/// A progress bar for the console.
/// </summary>
public class ConsoleProgressBar : IDisposable, IProgress<double>
{
    private const int BlockCount = 10;
    private readonly TimeSpan _animationInterval = TimeSpan.FromMilliseconds(125);
    private const string Animation = @"|/-\";

    private readonly Timer _timer;

    private double _currentPercent = 0;
    private string _currentText = string.Empty;
    private bool _disposed = false;
    private int _animationIndex = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleProgressBar"/> class.
    /// </summary>
    public ConsoleProgressBar()
    {
        _timer = new Timer(TimerHandler);

        // A progress bar is only for temporary display in a console window.
        // If the console output is redirected to a file, draw nothing.
        // Otherwise, we'll end up with a lot of garbage in the target file.
        if (!Console.IsOutputRedirected)
        {
            ResetTimer();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_timer)
        {
            _disposed = true;
            UpdateText(string.Empty);
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public void Report(double value)
    {
        Guard.IsBetweenOrEqualTo(value, 0, 100);

        Interlocked.Exchange(ref _currentPercent, value);
    }

    private void TimerHandler(object? state)
    {
        lock (_timer)
        {
            if (_disposed)
            {
                return;
            }

            int progressBlockCount = (int)(_currentPercent / 100 * BlockCount);
            string text
                = string.Format("[{0}{1}] {2}% {3}",
                    new string('#', progressBlockCount),
                    new string('-', BlockCount - progressBlockCount),
                    Math.Floor(_currentPercent),
                    Animation[_animationIndex++ % Animation.Length]);

            UpdateText(text);

            ResetTimer();
        }
    }

    private void UpdateText(string text)
    {
        // Get length of common portion
        int commonPrefixLength = 0;
        int commonLength = Math.Min(_currentText.Length, text.Length);
        while (commonPrefixLength < commonLength && text[commonPrefixLength] == _currentText[commonPrefixLength])
        {
            commonPrefixLength++;
        }

        // Backtrack to the first differing character
        var outputBuilder = new StringBuilder();
        outputBuilder.Append('\b', _currentText.Length - commonPrefixLength);

        // Output new suffix
        outputBuilder.Append(text.AsSpan(commonPrefixLength));

        // If the new text is shorter than the old one: delete overlapping characters
        int overlapCount = _currentText.Length - text.Length;
        if (overlapCount > 0)
        {
            outputBuilder.Append(' ', overlapCount);
            outputBuilder.Append('\b', overlapCount);
        }

        Console.Write(outputBuilder);
        _currentText = text;
    }

    private void ResetTimer()
    {
        _timer.Change(_animationInterval, TimeSpan.FromMilliseconds(-1));
    }
}
