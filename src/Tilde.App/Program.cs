using System.ComponentModel;

namespace Tilde.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Only one Tilde per user session: two hooks would both replace the
        // same letter. "Local\" scopes the name to this login session.
        using var mutex = new Mutex(initiallyOwned: true, @"Local\Tilde.SingleInstance", out bool isFirst);
        if (!isFirst)
        {
            return; // already running: exit quietly
        }

        ApplicationConfiguration.Initialize();
        InputSender.CheckStructSize();

        TrayContext tray;
        try
        {
            tray = new TrayContext();
        }
        catch (Win32Exception ex)
        {
            MessageBox.Show("Tilde couldn't start watching the keyboard.\n\n" + ex.Message,
                            "Tilde", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(tray);
    }
}
