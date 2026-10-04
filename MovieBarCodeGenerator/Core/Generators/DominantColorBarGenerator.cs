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
/// Paints each bar the most common color of its frame, giving
/// poster-like barcodes instead of the smeared average.
/// </summary>
public class DominantColorBarGenerator : IBarGenerator
{
    private const int SampleSize = 64;

    public DominantColorBarGenerator(
        string displayName,
        string fileNameSuffix = "_dominant")
    {
        _displayName = displayName;
        FileNameSuffix = fileNameSuffix ?? "";
    }

    public string Name => "DominantColor";
    private readonly string _displayName;
    public string DisplayName => _displayName ?? Name;
    public string FileNameSuffix { get; }

    public Image GetBar(BitmapStream source, int barWidth, int barHeight)
    {
        using var sourceImage = Image.FromStream(source, true, false);
        using var thumbnail = new GdiBarGenerator().GetResizedImage(sourceImage, SampleSize, SampleSize);

        var color = ComputeDominantColor(thumbnail);

        var bar = new Bitmap(barWidth, barHeight);
        bar.SetResolution(thumbnail.HorizontalResolution, thumbnail.VerticalResolution);

        using (var g = Graphics.FromImage(bar))
        using (var brush = new SolidBrush(color))
        {
            g.FillRectangle(brush, 0, 0, barWidth, barHeight);
        }

        return bar;
    }

    private static Color ComputeDominantColor(Bitmap image)
        => DominantColor.ComputeDominantColor(image);
    }
