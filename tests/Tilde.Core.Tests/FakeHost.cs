using Tilde.Core;

namespace Tilde.Core.Tests;

/// <summary>
/// Stands in for the Windows app in tests. It records what the engine asks
/// for, and the test decides when time passes by calling engine.OnTimer().
/// </summary>
internal sealed class FakeHost : IEngineHost
{
    public List<int> TimerStarts { get; } = [];
    public int TimerStops { get; private set; }
    public List<string> Replacements { get; } = [];
    public bool TimerRunning { get; private set; }
    public long FocusToken { get; set; } = 1;

    public void StartTimer(int milliseconds)
    {
        TimerStarts.Add(milliseconds);
        TimerRunning = true;
    }

    public void StopTimer()
    {
        TimerStops++;
        TimerRunning = false;
    }

    public void Replace(string text) => Replacements.Add(text);
}
