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
/// Samples a single vertical column of each frame and stretches it into a bar.
/// The sampled column sweeps left to right across the frame width as frames
/// progress, wrapping back to the left edge when it reaches the right edge.
/// </summary>
public class VerticalSweepBarGenerator : IFrameAwareBarGenerator
{
    public VerticalSweepBarGenerator(
        string displayName,
        string fileNameSuffix = "_vertical_sweep")
    {
        _displayName = displayName;
        FileNameSuffix = fileNameSuffix ?? "";
    }

    public string Name => "VerticalSweep";
    private readonly string _displayName;
    public string DisplayName => _displayName ?? Name;
    public string FileNameSuffix { get; }

    public Image GetBar(BitmapStream source, int barWidth, int barHeight)
        => GetBar(source, barWidth, barHeight, frameIndex: 0, frameCount: 1);

    public Image GetBar(BitmapStream source, int barWidth, int barHeight, int frameIndex, int frameCount)
    {
        using var sourceImage = Image.FromStream(source, true, false);
        int column = ResolveColumn(frameIndex, sourceImage.Width);

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
                new Rectangle(column, 0, 1, sourceImage.Height),
                GraphicsUnit.Pixel);
        }

        return bar;
    }

    public static int ResolveColumn(int frameIndex, int frameWidth)
    {
        if (frameWidth <= 0)
        {
            return 0;
        }

        int column = frameIndex % frameWidth;
        if (column < 0)
        {
            column += frameWidth;
        }

        return Math.Min(frameWidth - 1, Math.Max(0, column));
    }
}
