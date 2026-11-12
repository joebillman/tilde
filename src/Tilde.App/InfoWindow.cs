namespace Tilde.App;

/// <summary>
/// A small fixed window with a column of content and an OK button. The How to
/// use and About windows are both built on it.
/// </summary>
internal class InfoWindow : Form
{
    private readonly FlowLayoutPanel _content;

    protected InfoWindow(string title)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16);
        Font = new Font("Segoe UI", 10f);
        Icon = TrayIcons.Create(enabled: true);

        _content = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Dock = DockStyle.Fill,
        };
        Controls.Add(_content);
    }

    protected void AddText(string text, float size = 10f, bool bold = false)
    {
        _content.Controls.Add(new Label
        {
            Text = text,
            AutoSize = true,
            MaximumSize = new Size(420, 0), // wrap long lines
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            Margin = new Padding(0, 0, 0, 8),
        });
    }

    protected void AddControl(Control control) => _content.Controls.Add(control);

    /// <summary>Call last, after the content is added.</summary>
    protected void AddOkButton()
    {
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true, Anchor = AnchorStyles.Right };
        ok.Click += (_, _) => Close();
        _content.Controls.Add(ok);
        AcceptButton = ok;
        CancelButton = ok;
    }

    /// <summary>Show the window, or bring it to the front if it is already open.</summary>
    public void ShowAndActivate()
    {
        if (!Visible)
        {
            Show();
        }
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = FormWindowState.Normal;
        }
        Activate();
    }
}
