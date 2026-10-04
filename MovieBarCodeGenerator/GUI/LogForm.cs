using System.Drawing;
using System.Windows.Forms;

namespace MovieBarCodeGenerator.GUI;

// TODO: Get this to re-dock to the main window after the user moves it

/// <summary>
/// Floating log window owned by the main form. Closing it hides it instead
/// so it can be reopened from the main form. Logging stays thread-safe.
/// </summary>
partial class LogForm : Krypton.Toolkit.KryptonForm
{
    private readonly Krypton.Toolkit.KryptonTextBox _logBox;
    private int _maxLineWidth;

    public event EventHandler UserClosed;

    public LogForm()
    {
        _logBox = new Krypton.Toolkit.KryptonTextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.None,
            WordWrap = false,
        };
        _logBox.StateCommon.Content.Font = new Font("Courier New", 8.25f);
        _logBox.StateCommon.Border.Rounding = 3;
        _logBox.Resize += (s, e) => TextBoxAutoScroll.Update(_logBox);
        Controls.Add(_logBox);

        FormBorderStyle = FormBorderStyle.Sizable;
        ShowInTaskbar = false;
        ShowIcon = false;
        StartPosition = FormStartPosition.Manual;
        ClientSize = new Size(800, 600);
        Text = "Log";
    }

    /// <summary>
    /// Matches the log box to the dark/light mode
    /// </summary>
    public void ApplyThemeColors(bool dark)
    {
        if (dark)
        {
            _logBox.StateCommon.Back.Color1 = Color.FromArgb(113, 113, 113);
            _logBox.StateCommon.Content.Color1 = Color.FromArgb(255, 255, 255);
        }
        else
        {
            _logBox.StateCommon.Back.Color1 = Color.FromArgb(227, 230, 232);
            _logBox.StateCommon.Content.Color1 = Color.FromArgb(59, 59, 59);
        }
    }

    public void AppendLog(string value)
    {
        if (value == null)
        {
            return;
        }

        if (InvokeRequired)
        {
            Invoke(new Action(() => AppendLog(value)));
            return;
        }

        string line = $"{DateTime.Now:u} - " + value;
        _logBox.AppendText(line + Environment.NewLine);
        int appendedWidth = TextBoxAutoScroll.MeasureTextWidth(_logBox, line);
        if (appendedWidth > _maxLineWidth)
        {
            _maxLineWidth = appendedWidth;
        }

        TextBoxAutoScroll.Update(_logBox, _maxLineWidth);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        WindowCorners.ApplyRoundedCorners(this);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            UserClosed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            base.OnFormClosing(e);
        }
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        // 
        // LogForm
        // 
        this.ClientSize = new System.Drawing.Size(284, 261);
        this.Name = "LogForm";
        this.ResumeLayout(false);

    }
}
