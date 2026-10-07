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
/// Maps a 12-bin pitch-class (chroma) vector to a color. The dominant bin
/// sets the hue in chromatic order (C == red, stepping 30 degrees per
/// semitone), concentration sets saturation (peaky harmony is vivid, flat
/// percussion/loud spectra wash toward white), value stays full. An empty
/// (silent) vector maps to black.
/// </summary>
public static class ChromaColor
{
    public const int PitchClassCount = 12;

    /// <summary>
    /// Full pipeline from a chroma vector to its color.
    /// </summary>
    public static Color ChromaToColor(float[] chroma)
    {
        if (chroma == null)
        {
            throw new ArgumentNullException(nameof(chroma));
        }

        if (chroma.Length != PitchClassCount)
        {
            throw new ArgumentException($"Chroma vector must have {PitchClassCount} bins.", nameof(chroma));
        }

        float max = float.MinValue;
        float min = float.MaxValue;
        int dominant = 0;
        for (int bin = 0; bin < chroma.Length; bin++)
        {
            if (chroma[bin] > max)
            {
                max = chroma[bin];
                dominant = bin;
            }

            if (chroma[bin] < min)
            {
                min = chroma[bin];
            }
        }

        if (max <= 0)
        {
            return Color.Black;
        }

        double saturation = (max - min) / max;
        double hue = dominant * 30.0;

        HsvColor.HsvToRgb(hue, saturation, 1.0, out byte r, out byte g, out byte b);
        return Color.FromArgb(r, g, b);
    }
}
