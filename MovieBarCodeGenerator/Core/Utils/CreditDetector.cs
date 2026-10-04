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

namespace MovieBarCodeGenerator.Core.Utils;

/// <summary>
/// Detects end credits by scanning the last minutes of the video at 1 fps.
/// Credit rolls are dark background with bright text, so a credit frame has
/// low mean brightness but high edge density: that combination separates real
/// credits both from bright content and from smooth dark scenes (fades, night
/// shots). Returns the content duration, or null when no plausible credits
/// boundary is found (and everything is kept).
/// </summary>
public static class CreditDetector
{
    private const double SampleFps = 1.0;
    private const int SampleSize = 32;
    private static readonly TimeSpan TailScanWindow = TimeSpan.FromMinutes(15);
    private const double CreditBrightnessCap = 45.0;
    private const int EdgeSobelThreshold = 50;
    private const double EdgeDensityThreshold = 0.05;
    private const int MaxGapSeconds = 10;
    private const int MinCreditsSeconds = 60;
    private const double MinTrimFraction = 0.01;
    private const double MaxTrimFraction = 0.40;

    public static TimeSpan? FindContentEnd(FfmpegWrapper ffmpeg, string inputPath, CancellationToken cancellationToken, Action<string> log = null)
    {
        var mediaInfo = ffmpeg.GetMediaInfo(inputPath, cancellationToken, log);
        if (mediaInfo.Duration <= TimeSpan.Zero)
        {
            return null;
        }

        var scanStart = mediaInfo.Duration > TailScanWindow ? mediaInfo.Duration - TailScanWindow : TimeSpan.Zero;
        log?.Invoke(scanStart > TimeSpan.Zero
            ? $"Scanning brightness and edges over the last {TailScanWindow:g} to detect end credits..."
            : "Scanning brightness and edges to detect end credits...");

        var brightness = new List<double>();
        var edgeDensity = new List<double>();
        foreach (var bitmapStream in ffmpeg.GetPreviewFrames(inputPath, SampleFps, SampleSize, cancellationToken, log, scanStart > TimeSpan.Zero ? scanStart : null))
        {
            using (bitmapStream)
            using (var image = Image.FromStream(bitmapStream, true, false))
            using (var bitmap = new Bitmap(image))
            {
                brightness.Add(MeanBrightness(bitmap));
                edgeDensity.Add(EdgeDetector.ComputeEdgeDensity(bitmap, EdgeSobelThreshold));
            }
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (brightness.Any() == false)
        {
            log?.Invoke("Credits detection: no frames sampled, keeping the full video.");
            return null;
        }

        int cutoffIndex = FindCreditsStart(brightness, edgeDensity);
        if (cutoffIndex < 0)
        {
            log?.Invoke("Credits detection: no credit roll found, keeping the full video.");
            return null;
        }

        if (cutoffIndex == 0 && scanStart > TimeSpan.Zero)
        {
            log?.Invoke("Credits detection: the credit roll reaches the start of the scan window, keeping the full video.");
            return null;
        }

        double cutoffSeconds = scanStart.TotalSeconds + (cutoffIndex / SampleFps);
        double trimFraction = 1 - (cutoffSeconds / mediaInfo.Duration.TotalSeconds);

        if (trimFraction < MinTrimFraction)
        {
            log?.Invoke("Credits detection: no credit roll found, keeping the full video.");
            return null;
        }

        if (trimFraction > MaxTrimFraction)
        {
            log?.Invoke($"Credits detection: credits run is {trimFraction:P0} of the video, refusing to trim (limit {MaxTrimFraction:P0}). Keeping the full video.");
            return null;
        }

        var contentEnd = TimeSpan.FromSeconds(cutoffSeconds);
        log?.Invoke($"Credits detection: excluding the last {mediaInfo.Duration - contentEnd:g} (content ends at {contentEnd:g}).");
        return contentEnd;
    }

    /// <summary>
    /// Finds the first frame of the trailing credit roll: a sustained run of
    /// dim but edge-dense frames (credit text) reaching near the end of the
    /// sample. Short dark gaps inside the run (black pages between credits)
    /// and any trailing non-credit frames (logos after the credits) are
    /// tolerated. Returns the cutoff frame index, or -1 when there is no
    /// plausible credit roll.
    /// </summary>
    public static int FindCreditsStart(IReadOnlyList<double> brightness, IReadOnlyList<double> edgeDensity)
    {
        if (brightness == null || edgeDensity == null || brightness.Count == 0 || brightness.Count != edgeDensity.Count)
        {
            return -1;
        }

        // Median-smooth so isolated bright flashes (title cards) and flicker
        // don't break up the run.
        var smoothBrightness = MedianSmooth(brightness, 5);
        var smoothEdges = MedianSmooth(edgeDensity, 3);

        bool IsCredit(int i) => smoothBrightness[i] < CreditBrightnessCap && smoothEdges[i] >= EdgeDensityThreshold;

        int runStart = -1;
        int runEnd = -1;
        int gap = 0;
        for (int i = smoothBrightness.Count - 1; i >= 0; i--)
        {
            if (IsCredit(i))
            {
                gap = 0;
                runStart = i;
                if (runEnd < 0)
                {
                    runEnd = i;
                }
            }
            else
            {
                gap++;
                if (runEnd >= 0 && gap > MaxGapSeconds)
                {
                    break;
                }
            }
        }

        if (runStart < 0)
        {
            return -1;
        }

        if (runEnd - runStart + 1 < MinCreditsSeconds)
        {
            return -1;
        }

        return runStart;
    }

    private static List<double> MedianSmooth(IReadOnlyList<double> values, int window)
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
                    total += (0.21 * row[(x * 3) + 2]) + (0.72 * row[(x * 3) + 1]) + (0.07 * row[x * 3]);
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
