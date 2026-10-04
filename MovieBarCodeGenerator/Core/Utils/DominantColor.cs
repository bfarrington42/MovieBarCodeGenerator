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
/// Finds the most common color of a bitmap using per-channel histogram buckets,
/// keeping the average of the winning bucket so the result is representative
/// instead of posterized.
/// </summary>
internal static class DominantColor
{
    private const int BitsPerChannel = 5;
    private const int BucketsPerChannel = 1 << BitsPerChannel;

    /// <param name="mask">
    /// Optional row-major mask (one byte per pixel, same dimensions as
    /// <paramref name="image"/>). A pixel participates only when its mask value
    /// is non-zero. Null means the whole image participates.
    /// </param>
    /// <param name="minChannelBrightness">
    /// Pixels whose red, green, and blue channels are all at or below this
    /// value are ignored (letterbox residue and edge-blur fringe).
    /// </param>
    internal static Color ComputeDominantColor(Bitmap image, byte[] mask = null, int minChannelBrightness = 0)
    {
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        var data = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var counts = new int[BucketsPerChannel * BucketsPerChannel * BucketsPerChannel];
            var sumR = new long[counts.Length];
            var sumG = new long[counts.Length];
            var sumB = new long[counts.Length];

            int stride = Math.Abs(data.Stride);
            var row = new byte[stride];
            for (int y = 0; y < data.Height; y++)
            {
                Marshal.Copy(IntPtr.Add(data.Scan0, y * stride), row, 0, stride);
                for (int x = 0; x < data.Width; x++)
                {
                    int rawB = row[(x * 4) + 0];
                    int rawG = row[(x * 4) + 1];
                    int rawR = row[(x * 4) + 2];
                    if (mask != null && mask[(y * data.Width) + x] == 0)
                    {
                        continue;
                    }

                    if (minChannelBrightness > 0 && rawR <= minChannelBrightness && rawG <= minChannelBrightness && rawB <= minChannelBrightness)
                    {
                        continue;
                    }

                    int b = rawB >> (8 - BitsPerChannel);
                    int g = rawG >> (8 - BitsPerChannel);
                    int r = rawR >> (8 - BitsPerChannel);
                    int bucket = ((r * BucketsPerChannel) + g) * BucketsPerChannel + b;
                    counts[bucket]++;
                    sumR[bucket] += rawR;
                    sumG[bucket] += rawG;
                    sumB[bucket] += rawB;
                }
            }

            int best = 0;
            for (int i = 1; i < counts.Length; i++)
            {
                if (counts[i] > counts[best])
                {
                    best = i;
                }
            }

            if (counts[best] == 0)
            {
                return Color.Black;
            }

            return Color.FromArgb(
                (int)(sumR[best] / counts[best]),
                (int)(sumG[best] / counts[best]),
                (int)(sumB[best] / counts[best]));
        }
        finally
        {
            image.UnlockBits(data);
        }
    }
}
