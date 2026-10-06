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
using System.Threading;

namespace MovieBarCodeGenerator.Core.Utils;

/// <summary>
/// Per-bar brightness gains from the audio spectral centroid ("brighter
/// sound, brighter bar"). Each bar's sample range is analyzed with a
/// Hann-windowed FFT, the centroid is taken on a perceptual (log) scale and
/// normalized per movie (5th to 95th percentile, so outliers cannot compress
/// the range), then mapped to a magnified gain around 1.0. Silent bars,
/// degenerate input, near-uniform audio, and uniform audio yield neutral
/// gain. Chunking mirrors <see cref="WaveformBarGenerator"/> so the
/// gains stay aligned with video bars and the waveform overlay.
/// </summary>
public static class SpectralBrightness
{
    /// <summary>
    /// Bars with fewer samples than this carry no meaningful spectral
    /// content and get neutral gain.
    /// </summary>
    private const int MinChunkLength = 64;

    /// <summary>
    /// Minimum max-to-min centroid ratio for grading (about a major third).
    /// Below this the audio counts as uniform and every bar gets neutral
    /// gain instead of stretching small variation (e.g. encoder shimmer on
    /// a pure tone) across the full range. Real program material spans far
    /// more than this.
    /// </summary>
    private const double MinSpreadRatio = 1.25;

    /// <summary>
    /// Normalization percentiles. The bulk of the movie spans the full
    /// range; outlier bars outside clamp to the extremes.
    /// </summary>
    private const double LowPercentile = 5.0;
    private const double HighPercentile = 95.0;

    /// <summary>
    /// Widens the normalized deviation, so the bulk of the movie visibly
    /// grades instead of clustering around neutral. At full intensity the
    /// extremes reach 0 and 2 (clamped to valid brightness on application).
    /// </summary>
    private const double GainMagnify = 2.0;

    /// <summary>
    /// Computes one brightness gain per bar from mono normalized samples.
    /// <paramref name="intensity"/> 0 returns all-neutral gains; 1 spans the
    /// full magnified range (0 for the dullest bars, 2 for the brightest,
    /// clamped to valid brightness when applied).
    /// </summary>
    public static float[] ComputeGains(float[] samples, int sampleRate, int width, int barWidth, double intensity, CancellationToken cancellationToken = default)
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

        if (double.IsNaN(intensity) || intensity < 0 || intensity > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(intensity), "Intensity must be between 0 and 1.");
        }

        int barCount = Math.Max(1, width / barWidth);
        var gains = new float[barCount];
        for (int i = 0; i < gains.Length; i++)
        {
            gains[i] = 1f;
        }

        if (samples == null || samples.Length == 0 || intensity <= 0)
        {
            return gains;
        }

        // Per-bar log-centroids; invalid bars stay neutral and are excluded
        // from the per-movie normalization.
        var logCentroids = new double[barCount];
        var valid = new bool[barCount];
        int validCount = 0;

        RealFft fft = null;
        int fftSize = 0;

        for (int bar = 0; bar < barCount; bar++)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
                // Hann window over the real samples; padding stays zero.
                double window = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (chunkLength - 1)));
                padded[i] = (float)(samples[start + i] * window);
            }

            float[] spectrum = fft.MagnitudeSpectrum(new DiscreteSignal(sampleRate, padded), normalize: false).Samples;

            double weightedSum = 0;
            double magnitudeSum = 0;
            for (int bin = 0; bin < spectrum.Length; bin++)
            {
                double magnitude = spectrum[bin];
                magnitudeSum += magnitude;
                weightedSum += magnitude * bin * sampleRate / fftSize;
            }

            if (magnitudeSum <= 0)
            {
                continue;
            }

            double centroid = weightedSum / magnitudeSum;
            if (centroid <= 0)
            {
                continue;
            }

            double logCentroid = Math.Log(centroid);
            logCentroids[bar] = logCentroid;
            valid[bar] = true;
            validCount++;
        }

        if (validCount == 0)
        {
            return gains;
        }

        var ordered = new double[validCount];
        int orderedIndex = 0;
        for (int bar = 0; bar < barCount; bar++)
        {
            if (valid[bar])
            {
                ordered[orderedIndex++] = logCentroids[bar];
            }
        }

        Array.Sort(ordered);
        double low = Percentile(ordered, LowPercentile);
        double high = Percentile(ordered, HighPercentile);
        if (high <= low || high - low < Math.Log(MinSpreadRatio))
        {
            return gains;
        }

        double range = high - low;
        for (int bar = 0; bar < barCount; bar++)
        {
            if (valid[bar])
            {
                double t = (logCentroids[bar] - low) / range;
                t = Math.Min(1, Math.Max(0, t));
                gains[bar] = (float)(1 + intensity * GainMagnify * (t - 0.5));
            }
        }

        return gains;
    }

    private static double Percentile(double[] ordered, double percent)
    {
        int index = (int)Math.Ceiling(percent / 100 * ordered.Length) - 1;
        index = Math.Min(ordered.Length - 1, Math.Max(0, index));
        return ordered[index];
    }
}
