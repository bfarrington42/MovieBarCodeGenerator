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
/// Paints each bar the most common color of its frame, giving
/// poster-like barcodes instead of the smeared average.
/// </summary>
public class DominantColorBarGenerator : IBarGenerator
{
    private const int SampleSize = 64;
    private const int BitsPerChannel = 5;
    private const int BucketsPerChannel = 1 << BitsPerChannel;

    public DominantColorBarGenerator(
        string displayName,
        string fileNameSuffix = "_dominant")
    {
        _displayName = displayName;
        FileNameSuffix = fileNameSuffix ?? "";
    }

    public string Name => "DominantColor";
    private readonly string _displayName;
    public string DisplayName => _displayName ?? Name;
    public string FileNameSuffix { get; }

    public Image GetBar(BitmapStream source, int barWidth, int barHeight)
    {
        using var sourceImage = Image.FromStream(source, true, false);
        using var thumbnail = new GdiBarGenerator().GetResizedImage(sourceImage, SampleSize, SampleSize);

        var color = ComputeDominantColor(thumbnail);

        var bar = new Bitmap(barWidth, barHeight);
        bar.SetResolution(thumbnail.HorizontalResolution, thumbnail.VerticalResolution);

        using (var g = Graphics.FromImage(bar))
        using (var brush = new SolidBrush(color))
        {
            g.FillRectangle(brush, 0, 0, barWidth, barHeight);
        }

        return bar;
    }

    private static Color ComputeDominantColor(Bitmap image)
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
                    int b = row[(x * 4) + 0] >> (8 - BitsPerChannel);
                    int g = row[(x * 4) + 1] >> (8 - BitsPerChannel);
                    int r = row[(x * 4) + 2] >> (8 - BitsPerChannel);
                    int bucket = ((r * BucketsPerChannel) + g) * BucketsPerChannel + b;
                    counts[bucket]++;
                    sumR[bucket] += row[(x * 4) + 2];
                    sumG[bucket] += row[(x * 4) + 1];
                    sumB[bucket] += row[(x * 4) + 0];
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
