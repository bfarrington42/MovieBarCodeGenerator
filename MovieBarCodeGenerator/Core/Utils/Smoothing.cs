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
/// Vertical smoothing shared by all barcode modes: each pixel column is
/// replaced by its solid average color, the same look the old per-mode
/// "smoothed" variants produced. Applied in the pipeline, so every current
/// and future mode gets it for free. Already-uniform bars (dominant color,
/// scanline, ...) pass through unchanged.
/// </summary>
public static class Smoothing
{
    /// <summary>
    /// Averages each pixel column vertically in place (arithmetic mean per
    /// channel, truncated). Columns that are already uniform are untouched.
    /// </summary>
    public static void SmoothVertically(Bitmap target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        var rect = new Rectangle(0, 0, target.Width, target.Height);
        var targetData = target.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            int stride = Math.Abs(targetData.Stride);
            var row = new byte[stride];
            var sumR = new long[target.Width];
            var sumG = new long[target.Width];
            var sumB = new long[target.Width];

            for (int y = 0; y < target.Height; y++)
            {
                Marshal.Copy(IntPtr.Add(targetData.Scan0, y * stride), row, 0, stride);
                for (int x = 0; x < target.Width; x++)
                {
                    sumB[x] += row[(x * 4) + 0];
                    sumG[x] += row[(x * 4) + 1];
                    sumR[x] += row[(x * 4) + 2];
                }
            }

            for (int y = 0; y < target.Height; y++)
            {
                Marshal.Copy(IntPtr.Add(targetData.Scan0, y * stride), row, 0, stride);
                for (int x = 0; x < target.Width; x++)
                {
                    row[(x * 4) + 0] = (byte)(sumB[x] / target.Height);
                    row[(x * 4) + 1] = (byte)(sumG[x] / target.Height);
                    row[(x * 4) + 2] = (byte)(sumR[x] / target.Height);
                }

                Marshal.Copy(row, 0, IntPtr.Add(targetData.Scan0, y * stride), stride);
            }
        }
        finally
        {
            target.UnlockBits(targetData);
        }
    }
}
