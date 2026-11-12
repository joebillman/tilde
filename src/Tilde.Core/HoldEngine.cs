namespace Tilde.Core;

/// <summary>
/// The press-and-hold state machine (ARCHITECTURE.md §4). It decides, for each
/// key event, whether to let it through, and when to swap the typed character
/// for an accented one. It knows nothing about Windows: time and typing come
/// through <see cref="IEngineHost"/>.
///
/// Rule numbers (R1...) refer to PRODUCT.md §3.
/// </summary>
public sealed class HoldEngine
{
    private enum State
    {
        Idle,       // nothing tracked
        Pending,    // mapped key down, plain character typed, hold delay running
        Active,     // a variant replaced the character; cycling if there are several
        Suppressed, // hold cancelled by a focus change, but the key is still down
    }

    private readonly IEngineHost _host;
    private readonly AccentMap _map;
    private readonly Settings _settings;

    // The only things the engine ever remembers: the one key being held and
    // what it can turn into. Nothing about earlier keys is kept (Hard rule 2).
    private State _state = State.Idle;
    private int _heldKey;
    private char[] _cycle = [];   // variant 1 ... variant n, then the original character
    private int _cycleIndex;
    private long _focusToken;

    public HoldEngine(IEngineHost host, AccentMap map, Settings settings)
    {
        _host = host;
        _map = map;
        _settings = settings;
    }

    /// <summary>Key code of the key being tracked, or null when idle.</summary>
    public int? HeldKey => _state == State.Idle ? null : _heldKey;

    /// <summary>
    /// A key went down. <paramref name="mappedChar"/> is the character it will
    /// type if that character has variants and no shortcut modifier is down;
    /// otherwise null. Returns true if the event should be swallowed.
    /// </summary>
    public bool OnKeyDown(int key, char? mappedChar)
    {
        if (_state != State.Idle)
        {
            if (key == _heldKey)
            {
                // Auto-repeat of the held key. Swallow it so holding e never gives eeee (R3).
                return true;
            }

            // A different key commits whatever is showing (R6), then is handled normally.
            GoIdle();
        }

        if (mappedChar is not char c || !_map.IsMapped(c))
        {
            return false; // shortcuts and unmapped keys pass straight through (R8, R9)
        }

        var variants = _map.GetVariants(c);
        _cycle = [.. variants, c];
        _cycleIndex = -1;
        _heldKey = key;
        _focusToken = _host.FocusToken;
        _state = State.Pending;
        _host.StartTimer(_settings.HoldDelayMs);
        return false; // let the plain character type immediately (R1)
    }

    /// <summary>A key went up. Key-up events are never swallowed.</summary>
    public void OnKeyUp(int key)
    {
        if (_state != State.Idle && key == _heldKey)
        {
            GoIdle(); // releasing keeps whatever is showing (R5)
        }
    }

    /// <summary>The timer started by <see cref="IEngineHost.StartTimer"/> fired.</summary>
    public void OnTimer()
    {
        if (_state is State.Idle or State.Suppressed)
        {
            return;
        }

        if (_host.FocusToken != _focusToken)
        {
            // The user switched windows mid-hold. Never type into a window the
            // hold did not start in (R10). Keep swallowing repeats until key-up.
            _host.StopTimer();
            _state = State.Suppressed;
            return;
        }

        int variantCount = _cycle.Length - 1;
        if (_state == State.Pending)
        {
            _state = State.Active; // first replacement (R2)
        }
        else if (variantCount <= 1)
        {
            return; // one variant: no cycling, é stays é (R4)
        }

        // Move to the next item in the loop, wrapping around (R4).
        _cycleIndex = (_cycleIndex + 1) % _cycle.Length;
        _host.Replace(_cycle[_cycleIndex].ToString());

        if (variantCount > 1)
        {
            _host.StartTimer(_settings.CycleDelayMs);
        }
    }

    /// <summary>Forget any hold. Used when disabling, changing language, or exiting.</summary>
    public void Reset() => GoIdle();

    private void GoIdle()
    {
        _host.StopTimer();
        _state = State.Idle;
        _cycle = [];
    }
}
