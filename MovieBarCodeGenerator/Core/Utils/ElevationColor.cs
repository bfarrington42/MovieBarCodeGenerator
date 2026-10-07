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
/// Geographic elevation (hypsometric) color ramp. Deep purple basins
/// through blue water and green lowlands, yellow foothills and brown
/// highlands, up to white peaks. Piecewise linear between control points.
/// </summary>
public static class ElevationColor
{
    private static readonly double[] Positions = { 0.0, 0.25, 0.45, 0.65, 0.85, 1.0 };

    private static readonly Color[] Colors =
    {
        Color.FromArgb(75, 0, 130),
        Color.FromArgb(30, 100, 170),
        Color.FromArgb(60, 170, 70),
        Color.FromArgb(230, 210, 80),
        Color.FromArgb(180, 110, 50),
        Color.FromArgb(255, 255, 255),
    };

    /// <summary>
    /// Maps a normalized position to the ramp, clamping outside [0, 1]. Endpoints return the exact stop colors.
    /// </summary>
    public static Color PositionToColor(double position)
    {
        double t = Math.Min(1, Math.Max(0, position));

        int segment = 0;
        while (segment < Positions.Length - 2 && t > Positions[segment + 1])
        {
            segment++;
        }

        double span = Positions[segment + 1] - Positions[segment];
        double local = span <= 0 ? 0 : (t - Positions[segment]) / span;

        Color low = Colors[segment];
        Color high = Colors[segment + 1];
        return Color.FromArgb(
            Lerp(low.R, high.R, local),
            Lerp(low.G, high.G, local),
            Lerp(low.B, high.B, local));
    }

    /// <summary>
    /// Maps an audio frequency onto the ramp over the given range, so the range midpoint lands on the ramp midpoint.
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
        return PositionToColor(t);
    }

    private static byte Lerp(byte from, byte to, double t)
        => (byte)Math.Min(255, Math.Max(0, Math.Round(from + ((to - from) * t))));
}
