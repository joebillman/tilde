using System.Drawing.Imaging;
using Tilde.App;

// Regenerates every image the package and the .exe need from TildeGlyph.
// Run from the repository root:  dotnet run --project packaging/AssetGenerator
// The outputs are committed, so the package build doesn't need to run this.

string root = FindRepoRoot();
string assets = Path.Combine(root, "packaging", "Assets");
string listing = Path.Combine(root, "packaging", "listing");
Directory.CreateDirectory(assets);
Directory.CreateDirectory(listing);

int[] scales = [100, 125, 150, 200, 400];

// App list, Start menu, and title bar icon. "scale-N" versions follow the
// display's scaling; "targetsize-N" are exact sizes for the taskbar and Explorer.
// "altform-unplated" tells Windows to show the icon without a colored plate.
foreach (int scale in scales)
{
    Save(Render(Scaled(44, scale), glyphShare: 0.95f), assets, $"Square44x44Logo.scale-{scale}.png");
}
foreach (int size in new[] { 16, 24, 32, 48, 256 })
{
    using var image = Render(size, glyphShare: 1f);
    Save(image, assets, $"Square44x44Logo.targetsize-{size}.png");
    Save(image, assets, $"Square44x44Logo.targetsize-{size}_altform-unplated.png");
}

// Start menu tile (the glyph sits in the middle with room around it) and the
// small logo Windows uses in Settings and the installer.
foreach (int scale in scales)
{
    Save(Render(Scaled(150, scale), glyphShare: 0.6f), assets, $"Square150x150Logo.scale-{scale}.png");
    Save(Render(Scaled(50, scale), glyphShare: 1f), assets, $"StoreLogo.scale-{scale}.png");
}

// For the Store listing in Partner Center (uploaded by hand).
Save(Render(300, glyphShare: 0.85f), listing, "AppTile-300.png");

// Vector master and the .exe icon.
File.WriteAllText(Path.Combine(root, "packaging", "tilde-keys.svg"), TildeGlyph.ToSvg());
WriteIco(Path.Combine(root, "src", "Tilde.App", "Tilde.ico"), [16, 20, 24, 32, 40, 48, 64, 256]);

Console.WriteLine($"Wrote assets to {assets}, {listing}, the SVG, and Tilde.ico.");
return;

static int Scaled(int baseSize, int scale) => (int)Math.Round(baseSize * scale / 100.0);

// Draw the glyph centered on a transparent square; glyphShare is how much of
// the square it fills.
static Bitmap Render(int size, float glyphShare)
{
    var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
    using Graphics g = Graphics.FromImage(bitmap);
    float glyph = size * glyphShare;
    float offset = (size - glyph) / 2;
    TildeGlyph.Draw(g, new RectangleF(offset, offset, glyph, glyph), enabled: true);
    return bitmap;
}

static void Save(Bitmap image, string folder, string name) =>
    image.Save(Path.Combine(folder, name), ImageFormat.Png);

// An .ico file is a small header, a directory of images, then the images
// themselves. Since Windows Vista each image can simply be a PNG.
static void WriteIco(string path, int[] sizes)
{
    var pngs = sizes.Select(size =>
    {
        using Bitmap image = Render(size, glyphShare: 1f);
        using var stream = new MemoryStream();
        image.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }).ToArray();

    using var file = new BinaryWriter(File.Create(path));
    file.Write((ushort)0);            // reserved
    file.Write((ushort)1);            // type: icon
    file.Write((ushort)sizes.Length); // number of images

    int dataOffset = 6 + 16 * sizes.Length;
    for (int i = 0; i < sizes.Length; i++)
    {
        file.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i])); // width (0 means 256)
        file.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i])); // height
        file.Write((byte)0);           // palette size
        file.Write((byte)0);           // reserved
        file.Write((ushort)1);         // color planes
        file.Write((ushort)32);        // bits per pixel
        file.Write(pngs[i].Length);    // size of the PNG data
        file.Write(dataOffset);        // where it starts
        dataOffset += pngs[i].Length;
    }
    foreach (byte[] png in pngs)
    {
        file.Write(png);
    }
}

static string FindRepoRoot()
{
    for (var dir = new DirectoryInfo(Environment.CurrentDirectory); dir is not null; dir = dir.Parent)
    {
        if (File.Exists(Path.Combine(dir.FullName, "Tilde.sln")))
        {
            return dir.FullName;
        }
    }
    throw new InvalidOperationException("Run this from inside the Tilde repository.");
}
