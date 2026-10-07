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

using NWaves.Signals;
using NWaves.Transforms;

namespace MovieBarCodeGenerator.Core.Utils;

/// <summary>
/// Per-bar dominant audio frequencies shared by the audio-color features.
/// Each bar's sample range is analyzed with a Hann-windowed FFT (same
/// chunking as the waveform code, so bars stay time-aligned). The result
/// carries the peak frequency and normalized level per bar plus the movie
/// range to map over. Near-uniform movies fall back to the absolute hearing
/// range. Bars with nothing mappable carry NaN.
/// </summary>
public static class AudioPeaks
{
    /// <summary>
    /// Bars with fewer samples than this carry no meaningful spectral content.
    /// </summary>
    public const int MinChunkLength = 64;

    /// <summary>
    /// Bars whose peak sits this far below the movie's loudest are treated as silence rather than given a noise-floor value.
    /// </summary>
    public const double RelativeSilenceThreshold = 0.01;

    /// <summary>
    /// Below this peak spread ratio the movie counts as uniform and the range falls back to the absolute hearing range.
    /// </summary>
    public const double MinSpreadRatio = 1.25;

    public const double DefaultMinFrequency = 20.0;
    public const double DefaultMaxFrequency = 20000.0;

    public sealed class Envelope
    {
        public double[] Frequencies { get; }
        public double[] Levels { get; }
        public double MaxLevel { get; }
        public double MinFrequency { get; }
        public double MaxFrequency { get; }

        internal Envelope(double[] frequencies, double[] levels, double maxLevel, double minFrequency, double maxFrequency)
        {
            Frequencies = frequencies;
            Levels = levels;
            MaxLevel = maxLevel;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
        }
    }

    public static Envelope Compute(float[] samples, int sampleRate, int width, int barWidth)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (barWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(barWidth));
        }

        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        int barCount = Math.Max(1, width / barWidth);
        var frequencies = new double[barCount];
        var levels = new double[barCount];
        for (int bar = 0; bar < barCount; bar++)
        {
            frequencies[bar] = double.NaN;
            levels[bar] = 0;
        }

        if (samples == null || samples.Length == 0)
        {
            return new Envelope(frequencies, levels, 0, DefaultMinFrequency, DefaultMaxFrequency);
        }

        RealFft fft = null;
        int fftSize = 0;
        double maxLevel = 0;

        for (int bar = 0; bar < barCount; bar++)
        {
            int start = (int)((long)bar * samples.Length / barCount);
            int end = (int)((long)(bar + 1) * samples.Length / barCount);
            if (end <= start)
            {
                end = Math.Min(samples.Length, start + 1);
            }

            int chunkLength = end - start;
            if (chunkLength < MinChunkLength)
            {
                continue;
            }

            int requiredSize = 1;
            while (requiredSize < chunkLength)
            {
                requiredSize <<= 1;
            }

            if (fft == null || fftSize != requiredSize)
            {
                fftSize = requiredSize;
                fft = new RealFft(fftSize);
            }

            var padded = new float[fftSize];
            for (int i = 0; i < chunkLength; i++)
            {
                double window = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (chunkLength - 1)));
                padded[i] = (float)(samples[start + i] * window);
            }

            float[] spectrum = fft.MagnitudeSpectrum(new DiscreteSignal(sampleRate, padded), normalize: false).Samples;

            float peak = 0;
            int peakBin = -1;
            for (int bin = 0; bin < spectrum.Length; bin++)
            {
                if (spectrum[bin] > peak)
                {
                    peak = spectrum[bin];
                    peakBin = bin;
                }
            }

            if (peakBin >= 0)
            {
                frequencies[bar] = (double)peakBin * sampleRate / fftSize;
                levels[bar] = peak / chunkLength;
                if (levels[bar] > maxLevel)
                {
                    maxLevel = levels[bar];
                }
            }
        }

        double minFrequency = double.MaxValue;
        double maxFrequency = double.MinValue;
        for (int bar = 0; bar < barCount; bar++)
        {
            if (!double.IsNaN(frequencies[bar])
                && frequencies[bar] > 0
                && levels[bar] >= maxLevel * RelativeSilenceThreshold)
            {
                if (frequencies[bar] < minFrequency)
                {
                    minFrequency = frequencies[bar];
                }

                if (frequencies[bar] > maxFrequency)
                {
                    maxFrequency = frequencies[bar];
                }
            }
        }

        bool normalize = maxFrequency > minFrequency && maxFrequency / minFrequency >= MinSpreadRatio;
        if (!normalize)
        {
            minFrequency = DefaultMinFrequency;
            maxFrequency = DefaultMaxFrequency;
        }

        return new Envelope(frequencies, levels, maxLevel, minFrequency, maxFrequency);
    }
}
