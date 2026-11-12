using Tilde.Core;

namespace Tilde.App;

/// <summary>
/// The whole app while it runs: tray icon, menu, settings, and the wiring
/// between the keyboard hook and the hold engine. There is no main window.
/// Menu layout is PRODUCT.md §6.
/// </summary>
internal sealed class TrayContext : ApplicationContext
{
    private static readonly (string Label, int Ms)[] HoldDelays =
    [
        ("Short (0.3 s)", Settings.ShortHoldDelayMs),
        ("Medium (0.5 s)", Settings.MediumHoldDelayMs),
        ("Long (0.8 s)", Settings.LongHoldDelayMs),
    ];

    private readonly Settings _settings;
    private readonly AccentMap _map;
    private readonly EngineHost _host;
    private readonly HoldEngine _engine;
    private readonly KeyboardHook _hook;
    private readonly IAutoStart _autoStart;

    private readonly Icon _onIcon = TrayIcons.Create(enabled: true);
    private readonly Icon _offIcon = TrayIcons.Create(enabled: false);
    private readonly NotifyIcon _tray;
    private readonly ToolStripMenuItem _enabledItem;
    private readonly ToolStripMenuItem _autoStartItem;
    private readonly List<(ToolStripMenuItem Item, int Ms)> _delayItems = [];

    private HowToWindow? _howTo;
    private AboutWindow? _about;

    public TrayContext()
    {
        _settings = SettingsStore.Load();
        _map = AccentMap.Load(_settings.Language);
        _autoStart = AutoStart.Create();

        _host = new EngineHost();
        _engine = new HoldEngine(_host, _map, _settings);
        _host.TimerFired += _engine.OnTimer;
        _hook = new KeyboardHook(_engine, new KeyTranslator(), _map, () => _settings.Enabled);
        _hook.Install();

        // ---- Menu ----
        var menu = new ContextMenuStrip();

        _enabledItem = new ToolStripMenuItem("Enabled", null, (_, _) => ToggleEnabled());
        menu.Items.Add(_enabledItem);

        var language = new ToolStripMenuItem("Language");
        language.DropDownItems.Add(new ToolStripMenuItem("Spanish") { Checked = true });
        menu.Items.Add(language);

        var holdDelay = new ToolStripMenuItem("Hold delay");
        foreach (var (label, ms) in HoldDelays)
        {
            var item = new ToolStripMenuItem(label, null, (_, _) => SetHoldDelay(ms));
            holdDelay.DropDownItems.Add(item);
            _delayItems.Add((item, ms));
        }
        menu.Items.Add(holdDelay);

        _autoStartItem = new ToolStripMenuItem("Start with Windows", null, (_, _) => ToggleAutoStart());
        menu.Items.Add(_autoStartItem);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("How to use", null, (_, _) => ShowHowTo());
        menu.Items.Add("About", null, (_, _) => ShowAbout());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());

        // Re-read the autostart state each time, in case it was changed outside Tilde.
        menu.Opening += (_, _) => UpdateMenu();

        _tray = new NotifyIcon { ContextMenuStrip = menu, Visible = true };
        _tray.DoubleClick += (_, _) => ToggleEnabled();
        UpdateMenu();

        if (!_settings.HowToShown)
        {
            ShowHowTo();
            _settings.HowToShown = true;
            SettingsStore.Save(_settings);
        }
    }

    private void ToggleEnabled()
    {
        _settings.Enabled = !_settings.Enabled;
        if (!_settings.Enabled)
        {
            _engine.Reset(); // drop any hold in progress so nothing is typed after turning off
        }
        SettingsStore.Save(_settings);
        UpdateMenu();
    }

    private void SetHoldDelay(int ms)
    {
        _settings.HoldDelayMs = ms; // the engine reads this on the next key press
        SettingsStore.Save(_settings);
        UpdateMenu();
    }

    private void ToggleAutoStart()
    {
        bool turnOn = !_autoStart.IsEnabled;
        try
        {
            _autoStart.SetEnabled(turnOn);
            _settings.StartWithWindows = turnOn;
            SettingsStore.Save(_settings);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            MessageBox.Show("Tilde couldn't change the Start with Windows setting.\n\n" + ex.Message,
                            "Tilde", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        UpdateMenu();
    }

    private void UpdateMenu()
    {
        _enabledItem.Checked = _settings.Enabled;
        _autoStartItem.Checked = _autoStart.IsEnabled;
        foreach (var (item, ms) in _delayItems)
        {
            item.Checked = _settings.HoldDelayMs == ms;
        }
        _tray.Icon = _settings.Enabled ? _onIcon : _offIcon;
        _tray.Text = _settings.Enabled ? "Tilde: on" : "Tilde: off";
    }

    private void ShowHowTo()
    {
        if (_howTo is null || _howTo.IsDisposed)
        {
            _howTo = new HowToWindow(_map);
        }
        _howTo.ShowAndActivate();
    }

    private void ShowAbout()
    {
        if (_about is null || _about.IsDisposed)
        {
            _about = new AboutWindow();
        }
        _about.ShowAndActivate();
    }

    protected override void ExitThreadCore()
    {
        _engine.Reset();
        _hook.Dispose();
        _tray.Visible = false; // otherwise a dead icon lingers until the mouse passes over it
        _tray.Dispose();
        _howTo?.Dispose();
        _about?.Dispose();
        _onIcon.Dispose();
        _offIcon.Dispose();
        _host.Dispose();
        base.ExitThreadCore();
    }
}
