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
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Options;
using NWaves.Windows;
using System.Drawing;

namespace MovieBarCodeGenerator.Core.Generators;

/// <summary>
/// Audio-only barcode. Each bar is painted the hue of its dominant pitch
/// class (chromatic order, C == red), with saturation from harmonic
/// concentration — tonal passages render vivid, diffuse percussion washes
/// toward white, silence renders black.
/// </summary>
public class ChromaColorBarGenerator : IAudioBarGenerator
{
    /// <summary>
    /// Bars with fewer samples than this carry no meaningful harmonic content and render black.
    /// </summary>
    private const int MinChunkLength = 64;

    /// <summary>
    /// Fixed chroma analysis window. A center excerpt (zero-padded when short) keeps resolution uniform across bars.
    /// </summary>
    private const int AnalysisWindow = 2048;

    public ChromaColorBarGenerator(
        string displayName,
        string fileNameSuffix = "_harmony")
    {
        _displayName = displayName;
        FileNameSuffix = fileNameSuffix ?? "";
    }

    public string Name => "Harmony";
    private readonly string _displayName;
    public string DisplayName => _displayName ?? Name;
    public string FileNameSuffix { get; }

    public bool RequiresAudio => true;

    public Image GetBar(BitmapStream source, int barWidth, int barHeight)
    {
        throw new NotSupportedException($"{nameof(ChromaColorBarGenerator)} is audio-only and cannot render from a video frame.");
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

            var options = new ChromaOptions
            {
                SamplingRate = sampleRate,
                FeatureCount = ChromaColor.PitchClassCount,
                FrameSize = AnalysisWindow,
                HopSize = AnalysisWindow,
                FftSize = AnalysisWindow,
                Window = WindowType.Hann,
            };
            var extractor = new ChromaExtractor(options);

            int barCount = Math.Max(1, width / barWidth);
            var window = new float[AnalysisWindow];
            var features = new float[ChromaColor.PitchClassCount];

            for (int bar = 0; bar < barCount; bar++)
            {
                int start = (int)((long)bar * samples.Length / barCount);
                int end = (int)((long)(bar + 1) * samples.Length / barCount);
                if (end <= start)
                {
                    end = Math.Min(samples.Length, start + 1);
                }

                Color color = Color.Black;
                int chunkLength = end - start;
                if (chunkLength >= MinChunkLength)
                {
                    Array.Clear(window, 0, window.Length);
                    int excerptLength = Math.Min(chunkLength, AnalysisWindow);
                    int excerptStart = start + ((chunkLength - excerptLength) / 2);
                    Array.Copy(samples, excerptStart, window, (AnalysisWindow - excerptLength) / 2, excerptLength);

                    extractor.ProcessFrame(window, features);
                    color = ChromaColor.ChromaToColor(features);
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
