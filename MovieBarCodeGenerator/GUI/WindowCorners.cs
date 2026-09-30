using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MovieBarCodeGenerator.GUI;

/// <summary>
/// Applies slightly rounded window corners on Windows 11 and later.
/// Older versions ignore the preference and keep square corners.
/// </summary>
internal static class WindowCorners
{
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_ROUNDSMALL = 3;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int valueSize);

    public static void ApplyRoundedCorners(Form form)
    {
        if (Environment.OSVersion.Version >= new Version(10, 0, 22000))
        {
            int preference = DWMWCP_ROUNDSMALL;
            DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }
    }
}
