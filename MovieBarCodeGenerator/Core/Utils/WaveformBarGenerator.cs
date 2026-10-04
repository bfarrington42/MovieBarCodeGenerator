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

namespace MovieBarCodeGenerator.Core.Utils;

/// <summary>
/// Peak waveform drawing shared by audio-based barcode modes. One min/max
/// column per bar, painted in the requested color over the current <see
/// cref="Graphics"/> surface. No background is painted, so callers can
/// render onto an existing barcode (overlay) and stay column-aligned with
/// video bars of the same run. Silent sections contribute no pixels, leaving
/// the underlying image untouched.
/// </summary>
public static class WaveformBarGenerator
{
    /// <summary>
    /// Draws peak columns from mono normalized samples (-1..1).
    /// <paramref name="barWidth"/> mirrors the video pipeline so the audio
    /// columns stay aligned with video bars of the same run.
    /// </summary>
    public static void DrawWaveform(Graphics g, float[] samples, int width, int height, int barWidth, Color color)
    {
        if (g == null)
        {
            throw new ArgumentNullException(nameof(g));
        }

        ValidateDimensions(width, height, barWidth);

        if (samples == null || samples.Length == 0)
        {
            return;
        }

        int barCount = Math.Max(1, width / barWidth);
        int midY = height / 2;
        float halfHeight = Math.Max(1, height / 2f);

        using (var wavePen = new Pen(color))
        {
            for (int bar = 0; bar < barCount; bar++)
            {
                int start = (int)((long)bar * samples.Length / barCount);
                int end = (int)((long)(bar + 1) * samples.Length / barCount);
                if (end <= start)
                {
                    end = Math.Min(samples.Length, start + 1);
                }

                float min = float.MaxValue;
                float max = float.MinValue;
                for (int i = start; i < end; i++)
                {
                    float s = samples[i];
                    if (s < min)
                    {
                        min = s;
                    }

                    if (s > max)
                    {
                        max = s;
                    }
                }

                int yTop = midY - (int)(max * halfHeight);
                int yBottom = midY - (int)(min * halfHeight);
                yTop = Math.Min(height - 1, Math.Max(0, yTop));
                yBottom = Math.Min(height - 1, Math.Max(0, yBottom));
                if (yBottom < yTop)
                {
                    (yTop, yBottom) = (yBottom, yTop);
                }

                int x = bar * barWidth;
                int w = Math.Min(barWidth, width - x);
                for (int dx = 0; dx < w; dx++)
                {
                    g.DrawLine(wavePen, x + dx, yTop, x + dx, Math.Max(yTop, yBottom));
                }
            }
        }
    }

    /// <summary>
    /// Renders the waveform as a white on transparent mask (the default). Dimensions are
    /// validated even for empty samples.
    /// </summary>
    public static Bitmap RenderMask(float[] samples, int width, int height, int barWidth)
        => RenderMask(samples, width, height, barWidth, Color.White);

    /// <summary>
    /// Renders the waveform in the given color on a transparent background,
    /// for consumers that blend or composite it themselves (e.g. an HSV
    /// overlay, where the mask color's value is the blended value).
    /// Transparency marks the absence of waveform, so even black works as a
    /// real waveform color. Dimensions are validated even for empty samples.
    /// </summary>
    public static Bitmap RenderMask(float[] samples, int width, int height, int barWidth, Color color)
    {
        ValidateDimensions(width, height, barWidth);

        var mask = new Bitmap(width, height);
        using (var g = Graphics.FromImage(mask))
        {
            g.Clear(Color.Transparent);
            DrawWaveform(g, samples, width, height, barWidth, color);
        }

        return mask;
    }

    private static void ValidateDimensions(int width, int height, int barWidth)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        if (barWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(barWidth));
        }
    }
}
