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

using MovieBarCodeGenerator.Core.Utils;
using System.Collections.Generic;
using System.Drawing;

namespace MovieBarCodeGenerator.Core.Generators;

/// <summary>
/// Paints each bar with the dominant color of the largest object (subject) in
/// its frame instead of the whole frame. The subject is found by edge
/// detection, morphological cleanup, and largest-blob extraction on a small
/// working thumbnail. Falls back to the whole-frame dominant color when no
/// significant blob is found.
/// </summary>
public class SubjectColorBarGenerator : IBarGenerator
{
    private const int SampleSize = 64;
    private const double CropFraction = 0.125;
    private const int EdgeThreshold = 50;
    private const double MinBlobFraction = 0.02;
    private const int MinSubjectBrightness = 16;

    public SubjectColorBarGenerator(
        string displayName,
        string fileNameSuffix = "_subject")
    {
        _displayName = displayName;
        FileNameSuffix = fileNameSuffix ?? "";
    }

    public string Name => "SubjectColor";
    private readonly string _displayName;
    public string DisplayName => _displayName ?? Name;
    public string FileNameSuffix { get; }

    public Image GetBar(BitmapStream source, int barWidth, int barHeight)
    {
        using var sourceImage = Image.FromStream(source, true, false);

        // Apply cropping to get rid of any potential letterboxing as that will throw the whole thing off
        // See additional notes in LetterboxCropBarGenerator regarding doing this
        int cropY = (int)(sourceImage.Height * CropFraction);
        int cropHeight = Math.Max(1, sourceImage.Height - (2 * cropY));
        using var cropped = new Bitmap(sourceImage.Width, cropHeight);
        cropped.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
        using (var g = Graphics.FromImage(cropped))
        {
            g.DrawImage(
                sourceImage,
                new Rectangle(0, 0, cropped.Width, cropHeight),
                new Rectangle(0, cropY, sourceImage.Width, cropHeight),
                GraphicsUnit.Pixel);
        }

        // Create a thumbnail to work with since it'll be faster
        using var thumbnail = new GdiBarGenerator().GetResizedImage(cropped, SampleSize, SampleSize);

        // locate the subject
        byte[] mask = FindLargestSubjectMask(thumbnail);

        // establish fallbacks
        var color = mask == null
            ? DominantColor.ComputeDominantColor(thumbnail)
            : DominantColor.ComputeDominantColor(thumbnail, mask, MinSubjectBrightness);

        var bar = new Bitmap(barWidth, barHeight);
        bar.SetResolution(thumbnail.HorizontalResolution, thumbnail.VerticalResolution);

        using (var g = Graphics.FromImage(bar))
        using (var brush = new SolidBrush(color))
        {
            g.FillRectangle(brush, 0, 0, barWidth, barHeight);
        }

        return bar;
    }

    /// <summary>
    /// Returns a row-major foreground mask of the largest filled blob, or null
    /// when nothing significant was found. Filled interiors are preferred.
    /// When the subject touches the frame edge, the largest dilated outline is used as a fallback.
    /// </summary>
    private static byte[] FindLargestSubjectMask(Bitmap thumbnail)
    {
        int width = thumbnail.Width;
        int height = thumbnail.Height;

        byte[] edges = EdgeDetector.DetectEdges(thumbnail, EdgeThreshold);

        // Clean up the edge detection
        byte[] closed = Erode(Dilate(edges, width, height), width, height);

        // Flood-fill the background
        bool[] background = FloodBackground(closed, width, height);
        var filled = new byte[width * height];
        for (int i = 0; i < filled.Length; i++)
        {
            filled[i] = background[i] ? (byte)0 : (byte)1;
        }

        // Find the largest object (our subject)
        byte[] mask = LargestComponentMask(filled, width, height);
        if (mask != null)
        {
            return mask;
        }

        // Make sure we get the whole thing
        byte[] blobs = Dilate(Dilate(edges, width, height), width, height);

        return LargestComponentMask(blobs, width, height);
    }

    /// <summary>
    /// Returns a mask of the largest fully connected component of non-zero pixels,
    /// or null when the largest component is below the minimum blob size. We
    /// don't want to focus on any tiny objects just because it's the only one.
    /// </summary>
    private static byte[] LargestComponentMask(byte[] foreground, int width, int height)
    {
        int pixelCount = width * height;
        int[] labels = new int[pixelCount];
        int bestLabel = 0;
        int bestSize = 0;
        int nextLabel = 1;
        var queue = new Queue<int>();

        for (int i = 0; i < pixelCount; i++)
        {
            if (foreground[i] == 0 || labels[i] != 0)
            {
                continue;
            }

            int size = 0;
            labels[i] = nextLabel;
            queue.Enqueue(i);
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                size++;
                int cx = current % width;
                int cy = current / width;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0)
                        {
                            continue;
                        }

                        int nx = cx + dx;
                        int ny = cy + dy;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        {
                            continue;
                        }

                        int neighbor = (ny * width) + nx;
                        if (foreground[neighbor] != 0 && labels[neighbor] == 0)
                        {
                            labels[neighbor] = nextLabel;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            if (size > bestSize)
            {
                bestSize = size;
                bestLabel = nextLabel;
            }

            nextLabel++;
        }

        if (bestLabel == 0 || bestSize < MinBlobFraction * pixelCount)
        {
            return null;
        }

        var mask = new byte[pixelCount];
        for (int i = 0; i < pixelCount; i++)
        {
            mask[i] = labels[i] == bestLabel ? (byte)1 : (byte)0;
        }

        return mask;
    }

    /// <summary>
    /// See https://www.mathworks.com/help/images/morphological-dilation-and-erosion.html
    /// </summary>
    private static byte[] Dilate(byte[] binary, int width, int height)
    {
        var result = new byte[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool any = false;
                for (int dy = -1; dy <= 1 && !any; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < width && ny < height && binary[(ny * width) + nx] != 0)
                        {
                            any = true;
                            break;
                        }
                    }
                }

                result[(y * width) + x] = any ? (byte)1 : (byte)0;
            }
        }

        return result;
    }

    /// <summary>
    /// See https://www.mathworks.com/help/images/morphological-dilation-and-erosion.html
    /// </summary>
    private static byte[] Erode(byte[] binary, int width, int height)
    {
        var result = new byte[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool all = true;
                for (int dy = -1; dy <= 1 && all; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height || binary[(ny * width) + nx] == 0)
                        {
                            all = false;
                            break;
                        }
                    }
                }

                result[(y * width) + x] = all ? (byte)1 : (byte)0;
            }
        }

        return result;
    }

    /// <summary>
    /// Flood fills the background (non-edge pixels reachable from the image
    /// borders, 4-connected so diagonal edge gaps do not leak). Everything
    /// else is foreground.
    /// </summary>
    private static bool[] FloodBackground(byte[] edges, int width, int height)
    {
        var background = new bool[width * height];
        var queue = new Queue<int>();

        for (int x = 0; x < width; x++)
        {
            EnqueueBackgroundCandidate(edges, background, queue, width, height, x, 0);
            EnqueueBackgroundCandidate(edges, background, queue, width, height, x, height - 1);
        }

        for (int y = 0; y < height; y++)
        {
            EnqueueBackgroundCandidate(edges, background, queue, width, height, 0, y);
            EnqueueBackgroundCandidate(edges, background, queue, width, height, width - 1, y);
        }

        int[] dxs = { -1, 1, 0, 0 };
        int[] dys = { 0, 0, -1, 1 };
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            int cx = current % width;
            int cy = current / width;
            for (int d = 0; d < 4; d++)
            {
                int nx = cx + dxs[d];
                int ny = cy + dys[d];
                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                {
                    EnqueueBackgroundCandidate(edges, background, queue, width, height, nx, ny);
                }
            }
        }

        return background;
    }

    private static void EnqueueBackgroundCandidate(byte[] edges, bool[] background, Queue<int> queue, int width, int height, int x, int y)
    {
        int index = (y * width) + x;
        if (edges[index] == 0 && !background[index])
        {
            background[index] = true;
            queue.Enqueue(index);
        }
    }
}
