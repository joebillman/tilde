using static Tilde.App.NativeMethods;

namespace Tilde.App;

/// <summary>
/// Makes the tray icon from <see cref="TildeGlyph"/>, at exactly the size
/// Windows will show it, so it is never blurred by scaling.
/// </summary>
internal static class TrayIcons
{
    public static Icon Create(bool enabled)
    {
        // The small icon size already accounts for display scaling.
        int size = SystemInformation.SmallIconSize.Width;
        using var bitmap = new Bitmap(size, size);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            TildeGlyph.Draw(g, new RectangleF(0, 0, size, size), enabled);
        }

        // GetHicon creates a Windows icon handle that .NET does not free, so
        // copy it into a managed Icon and release the handle ourselves.
        IntPtr handle = bitmap.GetHicon();
        try
        {
            using Icon temporary = Icon.FromHandle(handle);
            return (Icon)temporary.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }
}
