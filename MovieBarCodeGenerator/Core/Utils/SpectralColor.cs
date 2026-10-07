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
/// Maps audio frequencies to visible colors. 20Hz-20kHz is log-mapped onto
/// the visible light frequency range (highest audio to highest light, so
/// bass renders red and treble violet), converted to wavelength, then to
/// RGB with Dan Bruton's approximation (380-780nm bands, edge falloff,
/// 0.8 gamma). Frequencies outside hearing clamp to the ends. Silence is
/// the caller's to handle (there is no frequency to map).
/// </summary>
public static class SpectralColor
{
    public const double MinAudioFrequency = 20.0;
    public const double MaxAudioFrequency = 20000.0;
    public const double MinWavelengthNm = 380.0;
    public const double MaxWavelengthNm = 780.0;
    public const double SpeedOfLight = 299792458.0;

    private const double Gamma = 0.8;

    /// <summary>
    /// Full pipeline from an audio frequency in Hz to its visible color, over the absolute hearing range.
    /// </summary>
    public static Color FrequencyToColor(double frequencyHz)
        => FrequencyToColor(frequencyHz, MinAudioFrequency, MaxAudioFrequency);

    /// <summary>
    /// Full pipeline from an audio frequency in Hz to its visible color,
    /// with the dullest <paramref name="minFrequencyHz"/> mapping to deep
    /// red and the brightest <paramref name="maxFrequencyHz"/> to violet.
    /// Pass the movie's own peak range for per-movie normalization.
    /// </summary>
    public static Color FrequencyToColor(double frequencyHz, double minFrequencyHz, double maxFrequencyHz)
    {
        if (minFrequencyHz <= 0 || maxFrequencyHz <= minFrequencyHz)
        {
            throw new ArgumentOutOfRangeException(nameof(minFrequencyHz), "Frequency range must be positive with max above min.");
        }

        double clamped = Math.Min(maxFrequencyHz, Math.Max(minFrequencyHz, frequencyHz));
        double t = (Math.Log10(clamped) - Math.Log10(minFrequencyHz))
            / (Math.Log10(maxFrequencyHz) - Math.Log10(minFrequencyHz));

        double redLight = SpeedOfLight / (MaxWavelengthNm * 1e-9);
        double violetLight = SpeedOfLight / (MinWavelengthNm * 1e-9);
        double lightFrequency = Math.Pow(10, Math.Log10(redLight) + t * (Math.Log10(violetLight) - Math.Log10(redLight)));

        // t in [0,1] means visible by construction - clamp only float noise at the edges (an epsilon below 380 would read as black)
        double wavelengthNm = Math.Min(MaxWavelengthNm, Math.Max(MinWavelengthNm, SpeedOfLight / lightFrequency * 1e9));
        return WavelengthToRgb(wavelengthNm);
    }

    /// <summary>
    /// Dan Bruton's wavelength-to-RGB approximation. Out-of-range wavelengths (invisible light) return black.
    /// </summary>
    public static Color WavelengthToRgb(double wavelengthNm)
    {
        if (wavelengthNm < MinWavelengthNm || wavelengthNm > MaxWavelengthNm)
        {
            return Color.Black;
        }

        double r;
        double g;
        double b;
        if (wavelengthNm < 440)
        {
            r = -(wavelengthNm - 440) / (440 - 380);
            g = 0;
            b = 1;
        }
        else if (wavelengthNm < 490)
        {
            r = 0;
            g = (wavelengthNm - 440) / (490 - 440);
            b = 1;
        }
        else if (wavelengthNm < 510)
        {
            r = 0;
            g = 1;
            b = -(wavelengthNm - 510) / (510 - 490);
        }
        else if (wavelengthNm < 580)
        {
            r = (wavelengthNm - 510) / (580 - 510);
            g = 1;
            b = 0;
        }
        else if (wavelengthNm < 645)
        {
            r = 1;
            g = -(wavelengthNm - 645) / (645 - 580);
            b = 0;
        }
        else
        {
            r = 1;
            g = 0;
            b = 0;
        }

        double factor;
        if (wavelengthNm < 420)
        {
            factor = 0.3 + (0.7 * (wavelengthNm - MinWavelengthNm) / (420 - MinWavelengthNm));
        }
        else if (wavelengthNm > 700)
        {
            factor = 0.3 + (0.7 * (MaxWavelengthNm - wavelengthNm) / (MaxWavelengthNm - 700));
        }
        else
        {
            factor = 1;
        }

        return Color.FromArgb(
            ToByte(r, factor),
            ToByte(g, factor),
            ToByte(b, factor));
    }

    private static byte ToByte(double channel, double factor)
        => (byte)Math.Min(255, Math.Max(0, Math.Round(255 * Math.Pow(channel * factor, Gamma))));
}
