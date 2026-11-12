using System.Drawing.Drawing2D;
using static Tilde.App.NativeMethods;

namespace Tilde.App;

/// <summary>
/// Draws the tray icon, a tilde (~), at runtime. A light-colored stroke inside
/// a dark outline stays readable on both light and dark taskbars.
/// </summary>
internal static class TrayIcons
{
    private static readonly Color OnColor = Color.FromArgb(255, 255, 196, 0);   // warm yellow
    private static readonly Color OffColor = Color.FromArgb(255, 175, 175, 175); // gray
    private static readonly Color OutlineColor = Color.FromArgb(255, 32, 32, 32);

    public static Icon Create(bool enabled)
    {
        // The small icon size already accounts for display scaling.
        int size = SystemInformation.SmallIconSize.Width;
        using var bitmap = new Bitmap(size, size);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using GraphicsPath tilde = TildeShape(size);

            // Dimmed when off: gray and partly transparent.
            int alpha = enabled ? 255 : 200;
            using var outline = RoundPen(Color.FromArgb(alpha, OutlineColor), size * 0.30f);
            using var stroke = RoundPen(Color.FromArgb(alpha, enabled ? OnColor : OffColor), size * 0.16f);
            g.DrawPath(outline, tilde);
            g.DrawPath(stroke, tilde);
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

    /// <summary>One wave: up on the left, down on the right.</summary>
    private static GraphicsPath TildeShape(float s)
    {
        var path = new GraphicsPath();
        path.AddBezier(0.16f * s, 0.62f * s, 0.24f * s, 0.30f * s, 0.40f * s, 0.30f * s, 0.50f * s, 0.50f * s);
        path.AddBezier(0.50f * s, 0.50f * s, 0.60f * s, 0.70f * s, 0.76f * s, 0.70f * s, 0.84f * s, 0.38f * s);
        return path;
    }

    private static Pen RoundPen(Color color, float width) =>
        new(color, width) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
}
