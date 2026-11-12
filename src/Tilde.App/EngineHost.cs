using Tilde.Core;
using static Tilde.App.NativeMethods;

namespace Tilde.App;

/// <summary>
/// Connects the platform-neutral <see cref="HoldEngine"/> to Windows: a
/// WinForms timer for time, SendInput for typing, and the foreground window
/// for focus.
/// </summary>
internal sealed class EngineHost : IEngineHost, IDisposable
{
    // A WinForms timer ticks on the UI thread, the same thread as the hook,
    // so the engine never needs locks.
    private readonly System.Windows.Forms.Timer _timer = new();

    public EngineHost()
    {
        _timer.Tick += (_, _) =>
        {
            _timer.Stop(); // one-shot: the engine restarts it if it wants another tick
            TimerFired?.Invoke();
        };
    }

    /// <summary>Raised when a timer started with <see cref="StartTimer"/> elapses.</summary>
    public event Action? TimerFired;

    public long FocusToken => GetForegroundWindow().ToInt64();

    public void StartTimer(int milliseconds)
    {
        _timer.Stop();
        _timer.Interval = milliseconds;
        _timer.Start();
    }

    public void StopTimer() => _timer.Stop();

    public void Replace(string text) => InputSender.Replace(text);

    public void Dispose() => _timer.Dispose();
}
