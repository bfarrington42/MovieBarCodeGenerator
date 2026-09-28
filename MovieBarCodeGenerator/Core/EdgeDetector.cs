//Copyright 2011-2021 Melvyn Laily
//https://zerowidthjoiner.net

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

namespace MovieBarCodeGenerator.Core;

/// <summary>
/// Color-aware Sobel edge detection shared by the subject-color barcode mode
/// and end-credits detection.
/// </summary>
public static class EdgeDetector
{
    /// <summary>
    /// Returns a row-major binary edge map. The gradient is computed per
    /// channel and the strongest channel wins, so isoluminant color
    /// boundaries (e.g. red on black) are still detected.
    /// </summary>
    public static byte[] DetectEdges(Bitmap image, int threshold)
    {
        int width = image.Width;
        int height = image.Height;

        var rect = new Rectangle(0, 0, width, height);
        var data = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        byte[] rgb;
        try
        {
            rgb = new byte[width * height * 3];
            int stride = Math.Abs(data.Stride);
            var row = new byte[stride];
            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(IntPtr.Add(data.Scan0, y * stride), row, 0, stride);
                for (int x = 0; x < width; x++)
                {
                    rgb[((y * width) + x) * 3 + 0] = row[(x * 4) + 0];
                    rgb[((y * width) + x) * 3 + 1] = row[(x * 4) + 1];
                    rgb[((y * width) + x) * 3 + 2] = row[(x * 4) + 2];
                }
            }
        }
        finally
        {
            image.UnlockBits(data);
        }

        var edges = new byte[width * height];
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int strongest = 0;
                for (int c = 0; c < 3; c++)
                {
                    int gx = -rgb[(((y - 1) * width) + x - 1) * 3 + c] - (2 * rgb[((y * width) + x - 1) * 3 + c]) - rgb[(((y + 1) * width) + x - 1) * 3 + c]
                        + rgb[(((y - 1) * width) + x + 1) * 3 + c] + (2 * rgb[((y * width) + x + 1) * 3 + c]) + rgb[(((y + 1) * width) + x + 1) * 3 + c];
                    int gy = -rgb[(((y - 1) * width) + x - 1) * 3 + c] - (2 * rgb[(((y - 1) * width) + x) * 3 + c]) - rgb[(((y - 1) * width) + x + 1) * 3 + c]
                        + rgb[(((y + 1) * width) + x - 1) * 3 + c] + (2 * rgb[(((y + 1) * width) + x) * 3 + c]) + rgb[(((y + 1) * width) + x + 1) * 3 + c];
                    int magnitude = Math.Abs(gx) + Math.Abs(gy);
                    if (magnitude > strongest)
                    {
                        strongest = magnitude;
                    }
                }

                edges[(y * width) + x] = strongest >= threshold ? (byte)1 : (byte)0;
            }
        }

        return edges;
    }

    /// <summary>
    /// Returns the fraction of pixels classified as edges (0 to 1).
    /// </summary>
    public static double ComputeEdgeDensity(Bitmap image, int threshold)
    {
        var edges = DetectEdges(image, threshold);
        int count = 0;
        for (int i = 0; i < edges.Length; i++)
        {
            if (edges[i] != 0)
            {
                count++;
            }
        }

        return edges.Length == 0 ? 0 : (double)count / edges.Length;
    }
}
