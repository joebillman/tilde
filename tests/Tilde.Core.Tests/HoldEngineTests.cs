using Tilde.Core;

namespace Tilde.Core.Tests;

/// <summary>Engine tests T1–T22 from docs/TESTING.md §2.</summary>
public class HoldEngineTests
{
    // Windows virtual-key codes, used only as opaque key IDs here.
    private const int KeyE = 0x45, KeyN = 0x4E, KeyS = 0x53, KeyU = 0x55;
    private const int KeySlash = 0xBF, Key1 = 0x31;

    private readonly FakeHost _host = new();
    private readonly Settings _settings = new();
    private readonly HoldEngine _engine;

    public HoldEngineTests()
    {
        _engine = new HoldEngine(_host, AccentMap.Load("es"), _settings);
    }

    /// <summary>Simulate the timer firing: only if one is running, like a real timer.</summary>
    private void Tick()
    {
        // Tests call _engine.OnTimer() directly instead when checking that a
        // late or stray tick does nothing.
        Assert.True(_host.TimerRunning, "Tick() expects a running timer");
        _engine.OnTimer();
    }

    [Fact]
    public void T1_Tap_TypesNormallyWithNoReplacement()
    {
        Assert.False(_engine.OnKeyDown(KeyE, 'e'));
        _engine.OnKeyUp(KeyE);

        Assert.Equal([500], _host.TimerStarts);
        Assert.False(_host.TimerRunning);
        Assert.Empty(_host.Replacements);
        Assert.Null(_engine.HeldKey);
    }

    [Fact]
    public void T2_Hold_ReplacesWithSingleVariantAndStops()
    {
        _engine.OnKeyDown(KeyE, 'e');
        Tick();

        Assert.Equal(["é"], _host.Replacements);
        Assert.Equal([500], _host.TimerStarts); // no cycle timer for one variant
    }

    [Fact]
    public void T3_RepeatsWhilePending_AreSwallowed()
    {
        Assert.False(_engine.OnKeyDown(KeyE, 'e'));
        Assert.True(_engine.OnKeyDown(KeyE, null));
        Assert.True(_engine.OnKeyDown(KeyE, null));
        Assert.True(_engine.OnKeyDown(KeyE, null));
        Assert.Empty(_host.Replacements);
    }

    [Fact]
    public void T4_RepeatAfterReplace_IsSwallowed()
    {
        _engine.OnKeyDown(KeyE, 'e');
        Tick();

        Assert.True(_engine.OnKeyDown(KeyE, null));
        Assert.Equal(["é"], _host.Replacements);
    }

    [Fact]
    public void T5_HoldingU_CyclesThroughVariantsAndOriginal()
    {
        _engine.OnKeyDown(KeyU, 'u');
        Tick();
        Tick();
        Tick();
        Tick();

        Assert.Equal(["ú", "ü", "u", "ú"], _host.Replacements);
        Assert.Equal([500, 600, 600, 600, 600], _host.TimerStarts);
    }

    [Fact]
    public void T6_Release_CommitsAndLateTimerDoesNothing()
    {
        _engine.OnKeyDown(KeyU, 'u');
        Tick();
        _engine.OnKeyUp(KeyU);
        _engine.OnTimer();

        Assert.Equal(["ú"], _host.Replacements);
        Assert.False(_host.TimerRunning);
    }

    [Fact]
    public void T7_UnmappedKeyWhileHolding_CommitsAndPassesThrough()
    {
        _engine.OnKeyDown(KeyE, 'e');

        Assert.False(_engine.OnKeyDown(KeyS, null));
        Assert.False(_host.TimerRunning);
        _engine.OnTimer();

        Assert.Empty(_host.Replacements);
        Assert.Null(_engine.HeldKey);
    }

    [Fact]
    public void T8_MappedKeyWhileHolding_StartsItsOwnHold()
    {
        _engine.OnKeyDown(KeyE, 'e');
        Assert.False(_engine.OnKeyDown(KeyN, 'n'));
        Tick();

        Assert.Equal(["ñ"], _host.Replacements);
        Assert.Equal(KeyN, _engine.HeldKey);
    }

    [Fact]
    public void T9_ReleasingOldKey_DoesNotCancelNewHold()
    {
        _engine.OnKeyDown(KeyE, 'e');
        _engine.OnKeyDown(KeyN, 'n');
        _engine.OnKeyUp(KeyE);
        Tick();

        Assert.Equal(["ñ"], _host.Replacements);
    }

    [Fact]
    public void T10_Uppercase_GivesUppercaseVariant()
    {
        _engine.OnKeyDown(KeyE, 'E');
        Tick();

        Assert.Equal(["É"], _host.Replacements);
    }

    [Fact]
    public void T11_UppercaseU_Cycles()
    {
        _engine.OnKeyDown(KeyU, 'U');
        Tick();
        Tick();
        Tick();

        Assert.Equal(["Ú", "Ü", "U"], _host.Replacements);
    }

    [Fact]
    public void T12_QuestionMark_GivesInvertedQuestionMark()
    {
        _engine.OnKeyDown(KeySlash, '?');
        Tick();

        Assert.Equal(["¿"], _host.Replacements);
    }

    [Fact]
    public void T13_ExclamationMark_GivesInvertedExclamationMark()
    {
        _engine.OnKeyDown(Key1, '!');
        Tick();

        Assert.Equal(["¡"], _host.Replacements);
    }

    [Fact]
    public void T14_NullMappedChar_PassesThroughAndStaysIdle()
    {
        Assert.False(_engine.OnKeyDown(KeyS, null));

        Assert.Empty(_host.TimerStarts);
        Assert.Null(_engine.HeldKey);
    }

    [Fact]
    public void T15_FocusChange_CancelsHold()
    {
        _engine.OnKeyDown(KeyE, 'e');
        _host.FocusToken = 2;
        Tick();

        Assert.Empty(_host.Replacements);
        Assert.False(_host.TimerRunning);
    }

    [Fact]
    public void T16_AfterFocusChange_RepeatsAreStillSwallowed()
    {
        _engine.OnKeyDown(KeyE, 'e');
        _host.FocusToken = 2;
        Tick();

        Assert.True(_engine.OnKeyDown(KeyE, null));
        Assert.Empty(_host.Replacements);
    }

    [Fact]
    public void T17_AfterFocusChange_ReleaseAndFreshHoldWorks()
    {
        _engine.OnKeyDown(KeyE, 'e');
        _host.FocusToken = 2;
        Tick();

        _engine.OnKeyUp(KeyE);
        Assert.False(_engine.OnKeyDown(KeyE, 'e'));
        Tick();

        Assert.Equal(["é"], _host.Replacements);
    }

    [Fact]
    public void T18_FocusChangeWhileCycling_StopsCycling()
    {
        _engine.OnKeyDown(KeyU, 'u');
        Tick();
        _host.FocusToken = 2;
        Tick();
        _engine.OnTimer();

        Assert.Equal(["ú"], _host.Replacements);
        Assert.False(_host.TimerRunning);
    }

    [Fact]
    public void T19_Reset_CancelsHold()
    {
        _engine.OnKeyDown(KeyE, 'e');
        _engine.Reset();
        _engine.OnTimer();

        Assert.Empty(_host.Replacements);
        Assert.Null(_engine.HeldKey);
        Assert.False(_host.TimerRunning);
    }

    [Fact]
    public void T20_KeyUpForKeyNeverPressed_DoesNothing()
    {
        _engine.OnKeyUp(KeyE);

        Assert.Null(_engine.HeldKey);
        Assert.Empty(_host.TimerStarts);
        Assert.Empty(_host.Replacements);
    }

    [Fact]
    public void T21_HoldDelaySetting_IsUsed()
    {
        _settings.HoldDelayMs = 300;
        _engine.OnKeyDown(KeyE, 'e');

        Assert.Equal([300], _host.TimerStarts);
    }

    [Fact]
    public void T22_TimerWhileIdle_DoesNothing()
    {
        _engine.OnTimer();

        Assert.Empty(_host.Replacements);
        Assert.Empty(_host.TimerStarts);
        Assert.Null(_engine.HeldKey);
    }
}
