using System.Drawing.Drawing2D;
using System.Globalization;

namespace Tilde.App;

/// <summary>
/// The Tilde Keys logo: one ~ wave, a warm yellow stroke inside a dark outline
/// so it reads on light and dark backgrounds. This is the single source of the
/// shape. The tray icon draws it at runtime, and packaging/AssetGenerator uses
/// this same file to make the Store images, the .ico, and an SVG copy.
/// </summary>
internal static class TildeGlyph
{
    public static readonly Color OnColor = Color.FromArgb(255, 255, 196, 0);   // warm yellow
    public static readonly Color OffColor = Color.FromArgb(255, 175, 175, 175); // gray, for "disabled"
    public static readonly Color OutlineColor = Color.FromArgb(255, 32, 32, 32);

    // The wave as two cubic Bézier curves on a 1 × 1 square: start, control,
    // control, end, then control, control, end. Up on the left, down on the right.
    private static readonly PointF[] Wave =
    [
        new(0.16f, 0.62f), new(0.24f, 0.30f), new(0.40f, 0.30f), new(0.50f, 0.50f),
        new(0.60f, 0.70f), new(0.76f, 0.70f), new(0.84f, 0.38f),
    ];

    /// <summary>Stroke widths as a share of the box. Small icons get bolder lines so they stay readable.</summary>
    private static (float Outline, float Stroke) Widths(float size) =>
        size <= 24 ? (0.34f, 0.20f) : (0.30f, 0.16f);

    /// <summary>Draw the glyph filling the square <paramref name="box"/>.</summary>
    public static void Draw(Graphics g, RectangleF box, bool enabled)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        float size = Math.Min(box.Width, box.Height);
        var (outlineWidth, strokeWidth) = Widths(size);
        int alpha = enabled ? 255 : 200; // dimmed when off

        using var path = new GraphicsPath();
        PointF[] p = Wave.Select(pt => new PointF(box.X + pt.X * size, box.Y + pt.Y * size)).ToArray();
        path.AddBeziers(p);

        using var outline = RoundPen(Color.FromArgb(alpha, OutlineColor), size * outlineWidth);
        using var stroke = RoundPen(Color.FromArgb(alpha, enabled ? OnColor : OffColor), size * strokeWidth);
        g.DrawPath(outline, path);
        g.DrawPath(stroke, path);
    }

    /// <summary>The same glyph as an SVG document, for designers and the README.</summary>
    public static string ToSvg(int size = 256)
    {
        var (outlineWidth, strokeWidth) = Widths(size);
        string P(PointF pt) => $"{F(pt.X * size)},{F(pt.Y * size)}";
        string d = $"M{P(Wave[0])} C{P(Wave[1])} {P(Wave[2])} {P(Wave[3])} C{P(Wave[4])} {P(Wave[5])} {P(Wave[6])}";
        string Path(Color c, float w) =>
            $"  <path d=\"{d}\" fill=\"none\" stroke=\"{Hex(c)}\" stroke-width=\"{F(w * size)}\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>\n";

        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {size} {size}\" width=\"{size}\" height=\"{size}\">\n" +
               "  <title>Tilde Keys</title>\n" +
               Path(OutlineColor, outlineWidth) +
               Path(OnColor, strokeWidth) +
               "</svg>\n";
    }

    private static Pen RoundPen(Color color, float width) =>
        new(color, width) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };

    private static string F(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
}
