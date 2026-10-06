//Copyright 2026 Billy Farrington

//This file is part of MovieBarCodeGenerator.

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MovieBarCodeGenerator.Core.Utils;

/// <summary>
/// RGB/HSV conversion plus mask-driven "value" blending in HSV space
/// (the equivalent to GIMP's "HSV Value" layer mode). Hue and saturation
/// come from the target image, value comes from the mask. The underlying
/// colors keep their vividness and only get brighter instead of being
/// washed out.
/// </summary>
public static class HsvColor
{
    // Adapted from here https://github.com/python/cpython/blob/3.9/Lib/colorsys.py#L124
    /// <param name="h">Hue in degrees, 0 =< h < 360</param>
    /// <param name="s">Saturation, 0 to 1.</param>
    /// <param name="v">Value, 0 to 1.</param>
    public static void RgbToHsv(byte r, byte g, byte b, out double h, out double s, out double v)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;

        v = max;
        s = max <= 0 ? 0 : delta / max;

        if (delta <= 0)
        {
            h = 0;
            return;
        }

        if (max == rd)
        {
            h = 60 * (((gd - bd) / delta) % 6);
        }
        else if (max == gd)
        {
            h = 60 * (((bd - rd) / delta) + 2);
        }
        else
        {
            h = 60 * (((rd - gd) / delta) + 4);
        }

        if (h < 0)
        {
            h += 360;
        }
    }

    // Adapted from here https://github.com/python/cpython/blob/3.9/Lib/colorsys.py#L143
    /// <param name="h">Hue in degrees, 0 (inclusive) to 360 (exclusive).</param>
    /// <param name="s">Saturation, 0 to 1.</param>
    /// <param name="v">Value, 0 to 1.</param>
    public static void HsvToRgb(double h, double s, double v, out byte r, out byte g, out byte b)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
        double m = v - c;

        double rd;
        double gd;
        double bd;
        if (h < 60)
        {
            rd = c;
            gd = x;
            bd = 0;
        }
        else if (h < 120)
        {
            rd = x;
            gd = c;
            bd = 0;
        }
        else if (h < 180)
        {
            rd = 0;
            gd = c;
            bd = x;
        }
        else if (h < 240)
        {
            rd = 0;
            gd = x;
            bd = c;
        }
        else if (h < 300)
        {
            rd = x;
            gd = 0;
            bd = c;
        }
        else
        {
            rd = c;
            gd = 0;
            bd = x;
        }

        r = ToByte((rd + m) * 255);
        g = ToByte((gd + m) * 255);
        b = ToByte((bd + m) * 255);
    }

    /// <summary>
    /// Blends <paramref name="target"/> in place, GIMP "HSV Value" style.
    /// Transparent mask pixels are untouched. Any other mask pixel takes the
    /// mask pixel's own value while keeping the target's hue and saturation,
    /// mixed by mask alpha and <paramref name="strength"/> (layer opacity in GIMP).
    /// A white mask at 1 is GIMP's "HSV Value" at full opacity.
    /// <paramref name="strength"/> 0 leaves the image unchanged and would just
    /// waste cycles. Both bitmaps must share dimensions.
    /// </summary>
    public static void BlendMaskValue(Bitmap target, Bitmap mask, double strength)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (mask == null)
        {
            throw new ArgumentNullException(nameof(mask));
        }

        if (strength < 0 || strength > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(strength), "Strength must be between 0 and 1. Note that 0 is pointless.");
        }

        if (target.Width != mask.Width || target.Height != mask.Height)
        {
            throw new ArgumentException("Target and mask must share dimensions.");
        }

        if (strength <= 0)
        {
            return;
        }

        var rect = new Rectangle(0, 0, target.Width, target.Height);
        var targetData = target.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        var maskData = mask.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            int stride = Math.Abs(targetData.Stride);
            var targetRow = new byte[stride];
            var maskRow = new byte[stride];
            for (int y = 0; y < target.Height; y++)
            {
                Marshal.Copy(IntPtr.Add(targetData.Scan0, y * stride), targetRow, 0, stride);
                Marshal.Copy(IntPtr.Add(maskData.Scan0, y * stride), maskRow, 0, stride);
                for (int x = 0; x < target.Width; x++)
                {
                    byte ma = maskRow[(x * 4) + 3];
                    if (ma == 0)
                    {
                        continue;
                    }

                    // Coverage comes from the mask alpha, so transparency
                    // marks the absence of waveform and even black works as
                    // a real waveform color.
                    double coverage = (ma / 255.0) * strength;

                    byte mb = maskRow[(x * 4) + 0];
                    byte mg = maskRow[(x * 4) + 1];
                    byte mr = maskRow[(x * 4) + 2];

                    byte b = targetRow[(x * 4) + 0];
                    byte g = targetRow[(x * 4) + 1];
                    byte r = targetRow[(x * 4) + 2];
                    byte a = targetRow[(x * 4) + 3];

                    // Value comes from the mask pixel (the waveform layer),
                    // hue and saturation come from the target, then mixed by
                    // coverage like layer opacity.
                    RgbToHsv(mr, mg, mb, out _, out _, out double maskV);
                    RgbToHsv(r, g, b, out double h, out double s, out _);
                    HsvToRgb(h, s, maskV, out byte cr, out byte cg, out byte cb);

                    targetRow[(x * 4) + 0] = ToByte(b * (1 - coverage) + cb * coverage);
                    targetRow[(x * 4) + 1] = ToByte(g * (1 - coverage) + cg * coverage);
                    targetRow[(x * 4) + 2] = ToByte(r * (1 - coverage) + cr * coverage);
                    targetRow[(x * 4) + 3] = a;
                }

                Marshal.Copy(targetRow, 0, IntPtr.Add(targetData.Scan0, y * stride), stride);
            }
        }
        finally
        {
            target.UnlockBits(targetData);
            mask.UnlockBits(maskData);
        }
    }

    private static byte ToByte(double value)
        => (byte)Math.Min(255, Math.Max(0, Math.Round(value)));

    /// <summary>
    /// Scales each column's value in place by its bar gain (column
    /// <c>x</c> uses <c>gains[x / barWidth]</c>, clamped to the last gain),
    /// keeping hue and saturation. A gain of exactly 1 leaves pixels
    /// byte-identical. Values clamp to the valid range.
    /// </summary>
    public static void ApplyColumnGains(Bitmap target, float[] gains, int barWidth)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (barWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(barWidth));
        }

        if (gains == null || gains.Length == 0)
        {
            return;
        }

        var rect = new Rectangle(0, 0, target.Width, target.Height);
        var targetData = target.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            int stride = Math.Abs(targetData.Stride);
            var targetRow = new byte[stride];
            for (int y = 0; y < target.Height; y++)
            {
                Marshal.Copy(IntPtr.Add(targetData.Scan0, y * stride), targetRow, 0, stride);
                for (int x = 0; x < target.Width; x++)
                {
                    float gain = gains[Math.Min(x / barWidth, gains.Length - 1)];
                    if (gain == 1f)
                    {
                        continue;
                    }

                    byte b = targetRow[(x * 4) + 0];
                    byte g = targetRow[(x * 4) + 1];
                    byte r = targetRow[(x * 4) + 2];

                    RgbToHsv(r, g, b, out double h, out double s, out double v);
                    HsvToRgb(h, s, Math.Min(1, Math.Max(0, v * gain)), out r, out g, out b);

                    targetRow[(x * 4) + 0] = b;
                    targetRow[(x * 4) + 1] = g;
                    targetRow[(x * 4) + 2] = r;
                }

                Marshal.Copy(targetRow, 0, IntPtr.Add(targetData.Scan0, y * stride), stride);
            }
        }
        finally
        {
            target.UnlockBits(targetData);
        }
    }
}
