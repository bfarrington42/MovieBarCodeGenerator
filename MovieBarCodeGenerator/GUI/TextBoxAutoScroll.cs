using Krypton.Toolkit;
using System.Drawing;
using System.Windows.Forms;

namespace MovieBarCodeGenerator.GUI;

/// <summary>
/// Shows a text box's scrollbars only while its content actually overflows,
/// since WinForms has no built-in automatic mode. Only switches the
/// ScrollBars property when the needed state changes, because switching
/// recreates the control handle.
/// </summary>
public static class TextBoxAutoScroll
{    public static void Update(KryptonTextBox box)
    {
        Update(box, MeasureTextWidth(box, box.Text));
    }

    public static void Update(KryptonTextBox box, int maxLineWidth)
    {
        if (box.IsDisposed)
        {
            return;
        }

        int lastLine = box.GetLineFromCharIndex(Math.Max(0, box.TextLength - 1));
        int visibleLines = Math.Max(1, box.ClientSize.Height / Math.Max(1, box.Font.Height));
        bool needVertical = (lastLine + 1) > visibleLines;
        bool needHorizontal = !box.WordWrap && maxLineWidth > box.ClientSize.Width;

        ScrollBars wanted = DecideWanted(needVertical, needHorizontal);

        if (box.ScrollBars != wanted)
        {
            int selectionStart = box.SelectionStart;
            int selectionLength = box.SelectionLength;
            box.ScrollBars = wanted;
            box.SelectionStart = selectionStart;
            box.SelectionLength = selectionLength;
            box.ScrollToCaret();
        }
    }

    public static ScrollBars DecideWanted(bool needVertical, bool needHorizontal)
    {
        if (needVertical)
        {
            return needHorizontal ? ScrollBars.Both : ScrollBars.Vertical;
        }

        return needHorizontal ? ScrollBars.Horizontal : ScrollBars.None;
    }

    public static int MeasureTextWidth(KryptonTextBox box, string text)
    {
        int max = 0;
        using var graphics = box.CreateGraphics();
        foreach (string line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            int width = TextRenderer.MeasureText(
                graphics,
                line,
                box.Font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
            if (width > max)
            {
                max = width;
            }
        }

        return max;
    }
}
