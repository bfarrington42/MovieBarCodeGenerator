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

namespace MovieBarCodeGenerator.Core.Generators;

/// <summary>
/// Optional extension for bar generators that render from the audio track
/// instead of video frames (e.g. waveform). The pipeline detects this
/// interface, extracts the audio once via ffmpeg/NWaves, and calls
/// <see cref="Generate"/> a single time per output image. Audio extraction
/// skips video decoding entirely.
/// </summary>
public interface IAudioBarGenerator : IBarGenerator
{
    /// <summary>
    /// When true, a silent or missing audio track is an error if every
    /// selected generator is audio-only, and the generator is skipped with
    /// a log warning in mixed video+audio runs.
    /// </summary>
    bool RequiresAudio { get; }

    /// <summary>
    /// Renders the full output image from mono normalized samples (-1..1).
    /// <paramref name="barWidth"/> mirrors the video pipeline so the audio
    /// columns stay aligned with video bars of the same run.
    /// </summary>
    Bitmap Generate(float[] samples, int sampleRate, int width, int height, int barWidth);
}
