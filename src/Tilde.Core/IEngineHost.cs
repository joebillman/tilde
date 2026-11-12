namespace Tilde.Core;

/// <summary>
/// Everything the hold engine needs from the outside world. The Windows app
/// implements this with a real timer and SendInput; tests use a fake.
/// </summary>
public interface IEngineHost
{
    /// <summary>Start a one-shot timer. Starting again replaces any pending tick.</summary>
    void StartTimer(int milliseconds);

    /// <summary>Cancel the pending tick, if any.</summary>
    void StopTimer();

    /// <summary>Delete the one character showing before the caret and type <paramref name="text"/>.</summary>
    void Replace(string text);

    /// <summary>A value that changes whenever the foreground window changes.</summary>
    long FocusToken { get; }
}
