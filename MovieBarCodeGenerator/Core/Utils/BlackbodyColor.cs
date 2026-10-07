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
/// Maps color temperatures to RGB with Tanner Helland's blackbody
/// approximation (ember orange through neutral white to icy blue).
/// Out-of-range temperatures clamp to the ends.
/// </summary>
public static class BlackbodyColor
{
    public const double MinTemperature = 1000.0;
    public const double MaxTemperature = 40000.0;

    /// <summary>
    /// Default mapping endpoints. Ember orange for the dullest bars, pale blue for the brightest.
    /// </summary>
    public const double DefaultMinTemperature = 1500.0;
    public const double DefaultMaxTemperature = 10000.0;

    /// <summary>
    /// Full pipeline from a color temperature in Kelvin to its visible color.
    /// </summary>
    public static Color TemperatureToColor(double kelvin)
    {
        double clamped = Math.Min(MaxTemperature, Math.Max(MinTemperature, kelvin));
        double temp = clamped / 100.0;

        double r;
        double g;
        double b;
        if (temp <= 66)
        {
            r = 255;
        }
        else
        {
            r = 329.698727446 * Math.Pow(temp - 60, -0.1332047592);
        }

        if (temp <= 66)
        {
            g = 99.4708025861 * Math.Log(temp) - 161.1195681661;
        }
        else
        {
            g = 288.1221695283 * Math.Pow(temp - 60, -0.0755148492);
        }

        if (temp >= 66)
        {
            b = 255;
        }
        else if (temp <= 19)
        {
            b = 0;
        }
        else
        {
            b = 138.5177312231 * Math.Log(temp - 10) - 305.0447927307;
        }

        return Color.FromArgb(ToByte(r), ToByte(g), ToByte(b));
    }

    private static byte ToByte(double channel)
        => (byte)Math.Min(255, Math.Max(0, Math.Round(channel)));
}
