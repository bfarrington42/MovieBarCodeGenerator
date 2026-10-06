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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MovieBarCodeGenerator.Core.Utils;

/// <summary>
/// Letterbox detection and cropping shared by the pipeline and the letterbox
/// barcode mode. Finds near-black bars at the top and bottom of each frame
/// and strips exactly rather than guessing, so widescreen movies average the
/// picture instead of the black bars. Frames without bars pass through untouched.
/// </summary>
public static class LetterboxCrop
{
    /// <summary>
    /// Default maximum per-side crop is 1/8th.
    /// </summary>
    public const double DefaultCropFraction = 0.125;

    /// <summary>
    /// A row counts as bar when at least this fraction of its pixels is
    /// black. Tolerant of speckle noise, but burned in subtitle text in
    /// the bars still stops detection (under-cropping that side).
    /// </summary>
    private const double RowBlackFraction = 0.99;

    /// <summary>
    /// A channel value at or below this counts as black. Above broadcast
    /// limited-range black (16) with margin for encode noise.
    /// </summary>
    private const byte BlackThreshold = 32;

    /// <summary>
    /// Frames keeping less content than this are left untouched (fades, near-black scenes, etc).
    /// </summary>
    private const double MinContentFraction = 0.5;

    /// <summary>
    /// Detects letterbox bars up to <paramref name="maxCropFraction"/> of the
    /// frame height on each side. Returns true with the exact bar counts when
    /// bars are found on both sides and enough content remains; otherwise
    /// false with both counts zeroed.
    /// </summary>
    public static bool TryDetectBars(Bitmap frame, double maxCropFraction, out int topBars, out int bottomBars)
    {
        topBars = 0;
        bottomBars = 0;

        if (frame == null)
        {
            throw new ArgumentNullException(nameof(frame));
        }

        if (maxCropFraction <= 0 || maxCropFraction >= 0.5)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCropFraction), "Crop fraction must be between and not equal to 0 and 0.5.");
        }

        int maxRows = (int)(frame.Height * maxCropFraction);
        if (maxRows <= 0)
        {
            return false;
        }

        var rect = new Rectangle(0, 0, frame.Width, frame.Height);
        var data = frame.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            topBars = CountBarRows(data, frame.Width, 0, 1, maxRows);
            bottomBars = CountBarRows(data, frame.Width, frame.Height - 1, -1, maxRows);
        }
        finally
        {
            frame.UnlockBits(data);
        }

        // Letterbox bars are symmetric by definition. One-sided darkness is
        // content (vignette, subtitles, fades), not a letterbox.
        if (topBars == 0 || bottomBars == 0)
        {
            topBars = 0;
            bottomBars = 0;
            return false;
        }

        if (frame.Height - topBars - bottomBars < frame.Height * MinContentFraction)
        {
            topBars = 0;
            bottomBars = 0;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Strips the detected bars, or returns an identical copy when no bars
    /// are found. The caller owns both the frame and the result.
    /// </summary>
    public static Bitmap CropFrame(Bitmap frame, double cropFraction)
    {
        if (TryDetectBars(frame, cropFraction, out int topBars, out int bottomBars))
        {
            return CropToBars(frame, topBars, bottomBars);
        }

        var copy = new Bitmap(frame.Width, frame.Height);
        copy.SetResolution(frame.HorizontalResolution, frame.VerticalResolution);
        using (var g = Graphics.FromImage(copy))
        {
            g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImage(
                frame,
                new Rectangle(0, 0, frame.Width, frame.Height),
                new Rectangle(0, 0, frame.Width, frame.Height),
                GraphicsUnit.Pixel);
        }

        return copy;
    }

    /// <summary>
    /// Cuts the given bar counts off the top and bottom. The caller owns
    /// both the frame and the result.
    /// </summary>
    public static Bitmap CropToBars(Bitmap frame, int topBars, int bottomBars)
    {
        if (frame == null)
        {
            throw new ArgumentNullException(nameof(frame));
        }

        int contentHeight = frame.Height - topBars - bottomBars;
        if (topBars < 0 || bottomBars < 0 || contentHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topBars), "Bar counts must leave at least one content row.");
        }

        var cropped = new Bitmap(frame.Width, contentHeight);
        cropped.SetResolution(frame.HorizontalResolution, frame.VerticalResolution);

        using (var g = Graphics.FromImage(cropped))
        {
            g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImage(
                frame,
                new Rectangle(0, 0, frame.Width, contentHeight),
                new Rectangle(0, topBars, frame.Width, contentHeight),
                GraphicsUnit.Pixel);
        }

        return cropped;
    }

    private static int CountBarRows(BitmapData data, int width, int startRow, int step, int maxRows)
    {
        int stride = Math.Abs(data.Stride);
        var row = new byte[stride];
        int count = 0;

        for (int n = 0; n < maxRows; n++)
        {
            int y = startRow + (n * step);
            Marshal.Copy(IntPtr.Add(data.Scan0, y * stride), row, 0, stride);

            int black = 0;
            for (int x = 0; x < width; x++)
            {
                if (row[(x * 4) + 0] <= BlackThreshold
                    && row[(x * 4) + 1] <= BlackThreshold
                    && row[(x * 4) + 2] <= BlackThreshold)
                {
                    black++;
                }
            }

            if ((double)black / width < RowBlackFraction)
            {
                break;
            }

            count++;
        }

        return count;
    }
}
