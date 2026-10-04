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

using MovieBarCodeGenerator.Core.Generators;
using MovieBarCodeGenerator.Core.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core;

public class ImageStreamProcessor
{
    /// <summary>
    /// Default waveform overlay strength
    /// </summary>
    public const double DefaultOverlayStrength = 0.6;

    public IReadOnlyDictionary<IBarGenerator, Bitmap> CreateBarCodes(
        BarCodeParameters parameters,
        FfmpegWrapper ffmpeg,
        CancellationToken cancellationToken,
        IProgress<double> progress = null,
        Action<string> log = null,
        bool excludeCredits = false,
        bool overlayWaveform = false,
        double overlayStrength = DefaultOverlayStrength,
        Color overlayWaveformColor = default)
    {
        TimeSpan? contentEnd = null;
        var barGenerators = parameters.GeneratorOutputPaths.Keys.ToArray();
        var audioGenerators = barGenerators.OfType<IAudioBarGenerator>().ToArray();
        var videoGenerators = barGenerators.Where(g => g is not IAudioBarGenerator).ToArray();

        // The waveform overlay applies to every selected video mode, like end-credits exclusion. It is a pipeline option, not a barcode generator.
        bool overlayRequested = overlayWaveform && videoGenerators.Length > 0;
        if (overlayWaveform && videoGenerators.Length == 0)
        {
            log?.Invoke("WARNING: waveform overlay is ignored when no video-based generators are selected.");
        }

        if (overlayRequested)
        {
            if (overlayStrength < 0 || overlayStrength > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(overlayStrength), "Overlay strength must be between 0 and 1.");
            }

            if (overlayWaveformColor.IsEmpty)
            {
                overlayWaveformColor = Color.White;
            }
        }

        bool needsAudio = audioGenerators.Length > 0 || overlayRequested;
        bool needsVideo = videoGenerators.Length > 0;

        // Pure audio runs have no video to trim, so credits detection is
        // meaningless there. Video runs trim both streams to the same
        // prefix, keeping per-bar audio columns aligned with video bars.
        if (needsAudio && !needsVideo && excludeCredits)
        {
            log?.Invoke("Excluding end credits is ignored when only audio-based generators are selected.");
            excludeCredits = false;
        }

        if (excludeCredits)
        {
            contentEnd = CreditDetector.FindContentEnd(ffmpeg, parameters.InputPath, cancellationToken, log);
        }

        var result = new Dictionary<IBarGenerator, Bitmap>();

        // Audio and video are two independent ffmpeg processes (s16le vs
        // rawvideo bmp), so run them concurrently to save time. Kick off
        // audio extraction on the pool while the calling thread decodes
        // video frames below. Alignment is proportional by time
        // (bar index <-> sample range), not by interleaved timestamps, so
        // no per-frame A/V sync is needed as long as both use the same
        // duration/trim.
        Task<AudioSamples> audioTask = null;
        if (needsAudio)
        {
            var audioMaxDuration = contentEnd;
            audioTask = Task.Run(() => ffmpeg.GetMonoAudioSamples(parameters.InputPath, cancellationToken, log, maxDuration: audioMaxDuration), cancellationToken);
        }

        Bitmap[] finalBitmaps = null;
        if (needsVideo)
        {
            var barCount = (int)Math.Round((double)parameters.Width / parameters.BarWidth);
            var bitmapStreamSource = ffmpeg.GetImagesFromMedia(parameters.InputPath, barCount, cancellationToken, log, autoToneMapHDR: true, maxDuration: contentEnd);

            finalBitmaps = new Bitmap[videoGenerators.Length];
            Graphics[] finalBitmapGraphics = new Graphics[videoGenerators.Length];

            int actualBarHeight = parameters.Height;

            try
            {
                for (int i = 0; i < videoGenerators.Length; i++)
                {
                    finalBitmaps[i] = new Bitmap(parameters.Width, actualBarHeight);
                    finalBitmapGraphics[i] = Graphics.FromImage(finalBitmaps[i]);
                }

                int x = 0;
                int frameIndex = 0;
                foreach (var bitmapStream in bitmapStreamSource)
                {
                    using (bitmapStream)
                    {
                        for (int i = 0; i < videoGenerators.Length; i++)
                        {
                            bitmapStream.Position = 0;
                            Image bar;
                            if (videoGenerators[i] is IFrameAwareBarGenerator frameAwareGenerator)
                            {
                                bar = frameAwareGenerator.GetBar(bitmapStream, parameters.BarWidth, actualBarHeight, frameIndex, barCount);
                            }
                            else
                            {
                                bar = videoGenerators[i].GetBar(bitmapStream, parameters.BarWidth, actualBarHeight);
                            }
                            var srcRect = new Rectangle(0, 0, bar.Width, bar.Height);
                            var destRect = new Rectangle(x, 0, parameters.BarWidth, actualBarHeight);
                            finalBitmapGraphics[i].DrawImage(bar, destRect, srcRect, GraphicsUnit.Pixel);
                        }

                        x += parameters.BarWidth;
                        frameIndex++;

                        progress?.Report((double)x / parameters.Width);
                    }
                }
            }
            finally
            {
                for (int i = 0; i < videoGenerators.Length; i++)
                {
                    finalBitmapGraphics[i]?.Dispose();
                }
            }
        }

        AudioSamples audio = null;
        if (needsAudio)
        {
            audio = audioTask.GetAwaiter().GetResult();
            cancellationToken.ThrowIfCancellationRequested();

            if (!audio.HasAudio)
            {
                if (!needsVideo)
                {
                    throw new InvalidOperationException("No audio track found: cannot generate audio-only output from a silent input.");
                }

                log?.Invoke("WARNING: no audio track found, skipping audio-based generators.");
            }
            else
            {
                foreach (var audioGenerator in audioGenerators)
                {
                    result.Add(audioGenerator, audioGenerator.Generate(audio.Samples, audio.SampleRate, parameters.Width, parameters.Height, parameters.BarWidth));
                }
            }
        }

        if (!needsVideo)
        {
            return result;
        }

        if (overlayRequested)
        {
            if (audio == null || !audio.HasAudio)
            {
                log?.Invoke("WARNING: no audio track found, skipping waveform overlay.");
            }
            else
            {
                // One shared mask for every mode. Same dimensions and bar width, so the columns stay aligned with all of them.
                using (var mask = WaveformBarGenerator.RenderMask(audio.Samples, parameters.Width, parameters.Height, parameters.BarWidth, overlayWaveformColor))
                {
                    foreach (var baseBitmap in finalBitmaps)
                    {
                        HsvColor.BlendMaskValue(baseBitmap, mask, overlayStrength);
                    }
                }
            }
        }

        for (int i = 0; i < videoGenerators.Length; i++)
        {
            result.Add(videoGenerators[i], finalBitmaps[i]);
        }

        return result;
    }
}
