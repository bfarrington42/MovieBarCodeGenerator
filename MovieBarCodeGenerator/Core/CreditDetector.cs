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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace MovieBarCodeGenerator.Core;

/// <summary>
/// Detects end credits by scanning mean frame brightness at 1 fps. Credits
/// are typically dark background with sparse text, so the content ends where
/// sustained brightness stops. Returns the content duration, or null when no
/// plausible credits boundary is found (and everything is kept).
/// </summary>
public static class CreditDetector
{
    private const double SampleFps = 1.0;
    private const int SampleSize = 32;
    private const double DarkThreshold = 18.0;
    private const double MinTrimFraction = 0.01;
    private const double MaxTrimFraction = 0.40;

    public static TimeSpan? FindContentEnd(FfmpegWrapper ffmpeg, string inputPath, CancellationToken cancellationToken, Action<string> log = null)
    {
        var mediaInfo = ffmpeg.GetMediaInfo(inputPath, cancellationToken, log);
        if (mediaInfo.Duration <= TimeSpan.Zero)
        {
            return null;
        }

        log?.Invoke("Scanning brightness to detect end credits...");

        var means = new List<double>();
        foreach (var bitmapStream in ffmpeg.GetPreviewFrames(inputPath, SampleFps, SampleSize, cancellationToken, log))
        {
            using (bitmapStream)
            using (var image = Image.FromStream(bitmapStream, true, false))
            using (var bitmap = new Bitmap(image))
            {
                means.Add(MeanBrightness(bitmap));
            }
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (means.Any() == false)
        {
            log?.Invoke("Credits detection: no frames sampled, keeping the full video.");
            return null;
        }

        // Median-smooth over a 5 second window so isolated bright flashes
        // such as title cards inside the credits don't break up the dark tail.
        var smoothed = MedianSmooth(means, 5);

        // Walk back over the dark tail until sustained bright content is found.
        int cutoffIndex = smoothed.Count;
        for (int i = smoothed.Count - 1; i >= 0; i--)
        {
            if (smoothed[i] > DarkThreshold)
            {
                cutoffIndex = i + 1;
                break;
            }

            cutoffIndex = i;
        }

        double cutoffSeconds = cutoffIndex / SampleFps;
        double trimFraction = 1 - (cutoffSeconds / mediaInfo.Duration.TotalSeconds);

        if (trimFraction < MinTrimFraction)
        {
            log?.Invoke("Credits detection: no dark tail found, keeping the full video.");
            return null;
        }

        if (trimFraction > MaxTrimFraction)
        {
            log?.Invoke($"Credits detection: dark tail is {trimFraction:P0} of the video, refusing to trim (limit {MaxTrimFraction:P0}). Keeping the full video.");
            return null;
        }

        var contentEnd = TimeSpan.FromSeconds(cutoffSeconds);
        log?.Invoke($"Credits detection: excluding the last {mediaInfo.Duration - contentEnd:g} (content ends at {contentEnd:g}).");
        return contentEnd;
    }

    private static List<double> MedianSmooth(List<double> values, int window)
    {
        int radius = window / 2;
        var result = new List<double>(values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            var windowValues = new List<double>(window);
            for (int j = i - radius; j <= i + radius; j++)
            {
                windowValues.Add(values[Math.Min(values.Count - 1, Math.Max(0, j))]);
            }
            windowValues.Sort();
            result.Add(windowValues[windowValues.Count / 2]);
        }

        return result;
    }

    private static double MeanBrightness(Bitmap image)
    {
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        var data = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        try
        {
            int stride = Math.Abs(data.Stride);
            var row = new byte[stride];
            double total = 0;
            for (int y = 0; y < data.Height; y++)
            {
                Marshal.Copy(IntPtr.Add(data.Scan0, y * stride), row, 0, stride);
                for (int x = 0; x < data.Width; x++)
                {
                    total += (0.299 * row[(x * 3) + 2]) + (0.587 * row[(x * 3) + 1]) + (0.114 * row[x * 3]);
                }
            }

            return total / (data.Width * data.Height);
        }
        finally
        {
            image.UnlockBits(data);
        }
    }
}
