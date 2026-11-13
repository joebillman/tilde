using Tilde.Core;

namespace Tilde.App;

/// <summary>
/// Edit the list of programs Tilde stays out of (PRODUCT.md R13, §6):
/// a text box with one program file name per line.
/// </summary>
internal sealed class ExcludedAppsWindow : InfoWindow
{
    private readonly TextBox _textBox;

    /// <param name="current">The list as it is now.</param>
    /// <param name="save">Called with the cleaned-up list when the user clicks Save.</param>
    public ExcludedAppsWindow(IEnumerable<string> current, Action<List<string>> save) : base("Excluded apps")
    {
        AddText($"{AppInfo.ProductName} stays off while one of these programs is in front, for example a game.");
        AddText("Type one program file name per line, such as minecraft.exe. To find a " +
                "name, open Task Manager, go to the Details tab, and look in the Name column.");

        _textBox = new TextBox
        {
            Multiline = true,
            AcceptsReturn = true,
            ScrollBars = ScrollBars.Vertical,
            Size = new Size(420, 180),
            Font = new Font("Consolas", 10f),
            Text = ExclusionList.Format(current),
            Margin = new Padding(0, 0, 0, 12),
        };
        AddControl(_textBox);

        var saveButton = new Button { Text = "Save", AutoSize = true };
        saveButton.Click += (_, _) =>
        {
            save(ExclusionList.Parse(_textBox.Text));
            Close();
        };
        var cancelButton = new Button { Text = "Cancel", AutoSize = true };
        cancelButton.Click += (_, _) => Close();

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Margin = new Padding(0),
        };
        buttons.Controls.Add(saveButton);
        buttons.Controls.Add(cancelButton);
        AddControl(buttons);

        // Enter adds a line in the text box, so only Escape gets a shortcut.
        CancelButton = cancelButton;

        // A focused text box starts with everything selected, so one stray key
        // would replace the whole list. Put the cursor at the end instead.
        Shown += (_, _) =>
        {
            _textBox.SelectionStart = _textBox.TextLength;
            _textBox.SelectionLength = 0;
        };
    }
}
