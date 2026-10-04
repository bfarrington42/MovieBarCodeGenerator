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

namespace MovieBarCodeGenerator.Core.Generators;

/// <summary>
/// Crops a fraction off the top and bottom of each frame (letterbox bars)
/// before scaling it into a bar, so widescreen movies average the picture
/// instead of the black bars. Default guestimate is 1/8th, adjust if needed.
/// </summary>
public class LetterboxCropBarGenerator : IBarGenerator
{
    public double CropFraction { get; }

    public LetterboxCropBarGenerator(
        string displayName,
        string fileNameSuffix = "_cropped",
        double cropFraction = 0.125)
    {
        if (cropFraction < 0 || cropFraction >= 0.5)
        {
            throw new ArgumentOutOfRangeException(nameof(cropFraction), "Crop fraction must be between 0 (inclusive) and 0.5 (exclusive).");
        }

        _displayName = displayName;
        FileNameSuffix = fileNameSuffix ?? "";
        CropFraction = cropFraction;
    }

    public string Name => $"LetterboxCrop-CropFraction={CropFraction}";
    private readonly string _displayName;
    public string DisplayName => _displayName ?? Name;
    public string FileNameSuffix { get; }

    public Image GetBar(BitmapStream source, int barWidth, int barHeight)
    {
        using var sourceImage = Image.FromStream(source, true, false);
        int cropY = (int)(sourceImage.Height * CropFraction);
        int cropHeight = Math.Max(1, sourceImage.Height - (2 * cropY));

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
                new Rectangle(0, cropY, sourceImage.Width, cropHeight),
                GraphicsUnit.Pixel);
        }

        return bar;
    }
}
