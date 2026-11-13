using Tilde.Core;

namespace Tilde.App;

/// <summary>Shows the key table from PRODUCT.md §4, built from the active language map.</summary>
internal sealed class HowToWindow : InfoWindow
{
    public HowToWindow(AccentMap map) : base($"How to use {AppInfo.ProductName}")
    {
        AddText("Press and hold a key", 14f, bold: true);
        AddText($"Hold a key for a moment and {AppInfo.ProductName} swaps the letter for its accent. " +
                "A quick tap types the normal letter, just like before.");
        AddControl(BuildTable(map));
        AddText("Hold Shift or use Caps Lock for capitals: Shift + e gives É.");
        AddText($"To turn {AppInfo.ProductName} off, for example while playing a game, double-click " +
                "its ~ icon in the taskbar tray, or right-click it for the menu.");
        AddOkButton();
    }

    private static TableLayoutPanel BuildTable(AccentMap map)
    {
        var table = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            Margin = new Padding(0, 4, 0, 12),
        };
        AddRow(table, "Hold", "Get", bold: true);

        // Capitals are derived automatically, so list only the lowercase
        // letters and the punctuation.
        foreach (char key in map.MappedCharacters.Where(c => !char.IsUpper(c)))
        {
            var variants = map.GetVariants(key);
            string result = variants.Count == 1
                ? variants[0].ToString()
                : $"{variants[0]}, keep holding for {string.Join(", then ", variants.Skip(1))}";
            AddRow(table, KeyLabel(key), result, bold: false);
        }
        return table;
    }

    /// <summary>On a US keyboard ? and ! need Shift, so say so.</summary>
    private static string KeyLabel(char key) => key switch
    {
        '?' => "?   (Shift + /)",
        '!' => "!   (Shift + 1)",
        _ => key.ToString(),
    };

    private static void AddRow(TableLayoutPanel table, string left, string right, bool bold)
    {
        var font = new Font("Segoe UI", 12f, bold ? FontStyle.Bold : FontStyle.Regular);
        table.Controls.Add(new Label { Text = left, AutoSize = true, Font = font, Margin = new Padding(8, 4, 24, 4) });
        table.Controls.Add(new Label { Text = right, AutoSize = true, Font = font, Margin = new Padding(8, 4, 8, 4) });
    }
}
