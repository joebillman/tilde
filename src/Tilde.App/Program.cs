using Tilde.Core;

namespace Tilde.App;

internal static class Program
{
    /// <summary>
    /// Temporary M2 entry point: default settings, the keyboard hook, and a bare
    /// tray icon with Exit. M3 replaces this with TrayContext and saved settings.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        InputSender.CheckStructSize();

        var settings = new Settings();
        var map = AccentMap.Load(settings.Language);

        using var host = new EngineHost();
        var engine = new HoldEngine(host, map, settings);
        host.TimerFired += engine.OnTimer;

        using var hook = new KeyboardHook(engine, new KeyTranslator(), map, () => settings.Enabled);
        hook.Install();

        var menu = new ContextMenuStrip();
        menu.Items.Add("Exit", null, (_, _) => Application.Exit());
        using var tray = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Tilde (development build)",
            ContextMenuStrip = menu,
            Visible = true,
        };

        Application.Run();

        engine.Reset();
        tray.Visible = false;
    }
}
