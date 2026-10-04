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

namespace MovieBarCodeGenerator.Core.Generators;

/// <summary>
/// Optional extension for bar generators whose output depends on which frame
/// of the sequence is being processed (e.g. sweeping a sampling position
/// across frames). The pipeline detects this interface and supplies the
/// zero-based frame index; generators that do not implement it keep working
/// exactly as before.
/// </summary>
public interface IFrameAwareBarGenerator : IBarGenerator
{
    Image GetBar(BitmapStream source, int barWidth, int barHeight, int frameIndex, int frameCount);
}
