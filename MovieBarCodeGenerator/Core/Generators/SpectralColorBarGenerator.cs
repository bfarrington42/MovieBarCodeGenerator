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
using System.Drawing;

namespace MovieBarCodeGenerator.Core.Generators;

/// <summary>
/// Audio-only barcode. Each bar is painted the visible color of its dominant
/// audio frequency, normalized per movie from dullest red to brightest
/// violet (via <see cref="SpectralColor"/>). Near-uniform movies fall back
/// to the absolute hearing-range mapping. Shares its peak envelope (gating,
/// per-movie range, uniformity fallback) with the blackbody and elevation
/// generators. Silent bars render black.
/// </summary>
public class SpectralColorBarGenerator : IAudioBarGenerator
{
    public SpectralColorBarGenerator(
        string displayName,
        string fileNameSuffix = "_spectrum")
    {
        _displayName = displayName;
        FileNameSuffix = fileNameSuffix ?? "";
    }

    public string Name => "Spectral";
    private readonly string _displayName;
    public string DisplayName => _displayName ?? Name;
    public string FileNameSuffix { get; }

    public bool RequiresAudio => true;

    public Image GetBar(BitmapStream source, int barWidth, int barHeight)
    {
        throw new NotSupportedException($"{nameof(SpectralColorBarGenerator)} is audio-only and cannot render from a video frame.");
    }

    public Bitmap Generate(float[] samples, int sampleRate, int width, int height, int barWidth)
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

        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        var bitmap = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Black);

            if (samples == null || samples.Length == 0)
            {
                return bitmap;
            }

            int barCount = Math.Max(1, width / barWidth);
            var envelope = AudioPeaks.Compute(samples, sampleRate, width, barWidth);

            for (int bar = 0; bar < barCount; bar++)
            {
                Color color = Color.Black;
                if (!double.IsNaN(envelope.Frequencies[bar]) && envelope.Levels[bar] >= envelope.MaxLevel * AudioPeaks.RelativeSilenceThreshold)
                {
                    color = SpectralColor.FrequencyToColor(envelope.Frequencies[bar], envelope.MinFrequency, envelope.MaxFrequency);
                }

                int x = bar * barWidth;
                int w = Math.Min(barWidth, width - x);
                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, x, 0, w, height);
                }
            }
        }

        return bitmap;
    }
}
