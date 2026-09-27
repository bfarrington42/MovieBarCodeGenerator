//Copyright 2011-2021 Melvyn Laily
//https://zerowidthjoiner.net

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
using System.Drawing.Drawing2D;

namespace MovieBarCodeGenerator.Core;

/// <summary>
/// Samples a single horizontal row of each frame and stretches it into a bar.
/// Sharper than the average-based modes and immune to letterbox bars.
/// Grabs from the middle of the frame by default.
/// </summary>
public class ScanlineBarGenerator : IBarGenerator
{
    public double RowFraction { get; }

    public ScanlineBarGenerator(
        string displayName,
        string fileNameSuffix = "_scanline",
        double rowFraction = 0.5)
    {
        if (rowFraction < 0 || rowFraction > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(rowFraction), "Row fraction must be between 0 and 1.");
        }

        _displayName = displayName;
        FileNameSuffix = fileNameSuffix ?? "";
        RowFraction = rowFraction;
    }

    public string Name => $"Scanline-RowFraction={RowFraction}";
    private readonly string _displayName;
    public string DisplayName => _displayName ?? Name;
    public string FileNameSuffix { get; }

    public Image GetBar(BitmapStream source, int barWidth, int barHeight)
    {
        using var sourceImage = Image.FromStream(source, true, false);
        int row = Math.Min(sourceImage.Height - 1, Math.Max(0, (int)(sourceImage.Height * RowFraction)));

        var bar = new Bitmap(barWidth, barHeight);
        bar.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

        using (var g = Graphics.FromImage(bar))
        {
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImage(
                sourceImage,
                new Rectangle(0, 0, barWidth, barHeight),
                new Rectangle(0, row, sourceImage.Width, 1),
                GraphicsUnit.Pixel);
        }

        return bar;
    }
}
