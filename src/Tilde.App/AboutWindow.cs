using System.Reflection;

namespace Tilde.App;

/// <summary>Version and the privacy promises from PRODUCT.md §8.</summary>
internal sealed class AboutWindow : InfoWindow
{
    public AboutWindow() : base($"About {AppInfo.ProductName}")
    {
        Version? version = Assembly.GetExecutingAssembly().GetName().Version;
        AddText($"{AppInfo.ProductName} {version?.ToString(3)}", 14f, bold: true);
        AddText("Press and hold a key to type Spanish accents.");

        AddText("Privacy", 11f, bold: true);
        AddText($"•  {AppInfo.ProductName} makes no network connections.\n" +
                $"•  {AppInfo.ProductName} does not record, store, or send anything you type.\n" +
                $"•  {AppInfo.ProductName} does not need administrator rights.\n" +
                "•  The source code is public.");

        AddText("Source code: github.com/joebillman/tilde\nMIT License");
        AddOkButton();
    }
}
