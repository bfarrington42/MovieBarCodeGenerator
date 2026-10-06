using MovieBarCodeGenerator.Core;
using MovieBarCodeGenerator.Core.Generators;
using NUnit.Framework;
using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class VideoProcessingTests
{

    public string FfmpegExecutablePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../ffmpeg.exe");
    public string TestVideoFileName = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.mkv");
    public const int TestVideoWidth = 1280;
    public const int TestVideoHeight = 720;
    public const int TestVideoDuration = 10;

    private void CreateTestVideoIfNecessary()
    {
        if (!File.Exists(TestVideoFileName))
        {
            var commandArguments = $"-f lavfi -i testsrc=duration={TestVideoDuration}:size={TestVideoWidth}x{TestVideoHeight}:rate=30 {TestVideoFileName}";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = FfmpegExecutablePath,
                Arguments = commandArguments,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            process.WaitForExit(10000);
        }
    }

    [Test]
    public void FfFmpegExecutableExists()
    {
        Assert.IsTrue(File.Exists(FfmpegExecutablePath));
    }

    [Test]
    public void TestFileCanBeCreated()
    {
        CreateTestVideoIfNecessary();

        Assert.IsTrue(File.Exists(TestVideoFileName));
    }

    [Test]
    public void FfmpegWrapper_GetMediaDuration_Returns_Correct_Value()
    {
        CreateTestVideoIfNecessary();
        var ffmpegWrapper = new FfmpegWrapper(FfmpegExecutablePath);

        var mediaInfo = ffmpegWrapper.GetMediaInfo(TestVideoFileName, CancellationToken.None);

        Assert.AreEqual(TimeSpan.FromSeconds(TestVideoDuration), mediaInfo.Duration);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(10)]
    [TestCase(21)]
    public async Task FfmpegWrapper_GetImagesFromMedia_Returns_Expected_Values(int requestedFrameCount)
    {
        CreateTestVideoIfNecessary();
        var ffmpegWrapper = new FfmpegWrapper(FfmpegExecutablePath);

        var images = ffmpegWrapper.GetImagesFromMedia(
            TestVideoFileName,
            requestedFrameCount,
            CancellationToken.None)
            .ToList();

        CollectionAssert.AllItemsAreNotNull(images);
        Assert.AreEqual(requestedFrameCount, images.Count);
        foreach (var bitmapStream in images)
        {
            var imageInfo = await Task.Run(() => ImageFileInfo.Load(bitmapStream));
            Assert.AreEqual(TestVideoWidth, imageInfo.Frames[0].Width);
            Assert.AreEqual(TestVideoHeight, imageInfo.Frames[0].Height);
        }
    }

    public string TestAudioVideoFileName = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_av.mkv");
    public string TestTwoToneFileName = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_twotone.mkv");
    public string TestLetterboxFileName = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_letterbox.mkv");

    private void CreateTestAudioVideoIfNecessary()
    {
        if (!File.Exists(TestAudioVideoFileName))
        {
            var commandArguments = "-f lavfi -i testsrc=duration=3:size=320x240:rate=10 -f lavfi -i sine=frequency=440:duration=3 -shortest "
                + $"\"{TestAudioVideoFileName}\"";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = FfmpegExecutablePath,
                Arguments = commandArguments,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            process.WaitForExit(15000);
        }

        // Chirp audio (rising pitch) for spectral grading tests
        if (!File.Exists(TestTwoToneFileName))
        {
            var commandArguments = "-f lavfi -i testsrc=duration=3:size=320x240:rate=10 -f lavfi -i aevalsrc=sin(2*PI*(200*t+1800*t*t/6)):duration=3 -shortest "
                + $"\"{TestTwoToneFileName}\"";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = FfmpegExecutablePath,
                Arguments = commandArguments,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            process.WaitForExit(15000);
        }
    }

    private void CreateTestLetterboxIfNecessary()
    {
        CreateTestAudioVideoIfNecessary();

        // 320x240 padded to 320x320: 40px bars top and bottom, exactly the
        // default 1/8 crop, so cropping restores the source pixels.
        if (!File.Exists(TestLetterboxFileName))
        {
            var commandArguments = $"-i \"{TestAudioVideoFileName}\" -vf pad=320:320:0:40 -c:a copy \"{TestLetterboxFileName}\"";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = FfmpegExecutablePath,
                Arguments = commandArguments,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            process.WaitForExit(15000);
        }
    }

    private static BarCodeParameters CreateParameters(string inputPath, params IBarGenerator[] generators)
    {
        return new BarCodeParameters
        {
            InputPath = inputPath,
            GeneratorOutputPaths = generators.ToDictionary(g => g, g => "dummy.png"),
            Width = 60,
            Height = 32,
            BarWidth = 1,
        };
    }

    private static int CountDifferingPixels(Bitmap a, Bitmap b)
    {
        Assert.AreEqual(a.Width, b.Width);
        Assert.AreEqual(a.Height, b.Height);

        int count = 0;
        for (int x = 0; x < a.Width; x++)
        {
            for (int y = 0; y < a.Height; y++)
            {
                if (a.GetPixel(x, y).ToArgb() != b.GetPixel(x, y).ToArgb())
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static double MeanAbsoluteDifference(Bitmap a, Bitmap b)
    {
        Assert.AreEqual(a.Width, b.Width);
        Assert.AreEqual(a.Height, b.Height);

        double total = 0;
        for (int x = 0; x < a.Width; x++)
        {
            for (int y = 0; y < a.Height; y++)
            {
                var pixelA = a.GetPixel(x, y);
                var pixelB = b.GetPixel(x, y);
                total += Math.Abs(pixelA.R - pixelB.R) + Math.Abs(pixelA.G - pixelB.G) + Math.Abs(pixelA.B - pixelB.B);
            }
        }

        return total / (a.Width * a.Height * 3);
    }

    [Test]
    public void ImageStreamProcessor_OverlayWaveform_Applies_To_All_Modes()
    {
        CreateTestAudioVideoIfNecessary();
        var ffmpeg = new FfmpegWrapper(FfmpegExecutablePath);
        var processor = new ImageStreamProcessor();

        var normal = new MagicScalerBarGenerator("Normal", average: false);
        var scanline = new ScanlineBarGenerator("Scanline");

        var plain = processor.CreateBarCodes(
            CreateParameters(TestAudioVideoFileName, normal, scanline),
            ffmpeg,
            CancellationToken.None);
        var overlaid = processor.CreateBarCodes(
            CreateParameters(TestAudioVideoFileName, normal, scanline),
            ffmpeg,
            CancellationToken.None,
            overlayWaveform: true,
            overlayStrength: 0.7);

        try
        {
            CollectionAssert.AreEquivalent(plain.Keys.ToArray(), overlaid.Keys.ToArray());
            foreach (var generator in plain.Keys)
            {
                Assert.Greater(
                    CountDifferingPixels(plain[generator], overlaid[generator]),
                    0,
                    $"Expected the overlay to change the {generator.DisplayName} output.");
            }
        }
        finally
        {
            foreach (var bitmap in plain.Values.Concat(overlaid.Values))
            {
                bitmap.Dispose();
            }
        }
    }

    [Test]
    public void ImageStreamProcessor_OverlayWaveform_Silent_Input_Falls_Back_To_Plain()
    {
        // test.mkv has no audio track: the overlay is skipped with a warning.
        CreateTestVideoIfNecessary();
        var ffmpeg = new FfmpegWrapper(FfmpegExecutablePath);
        var processor = new ImageStreamProcessor();

        var normal = new MagicScalerBarGenerator("Normal", average: false);

        var plain = processor.CreateBarCodes(
            CreateParameters(TestVideoFileName, normal),
            ffmpeg,
            CancellationToken.None);
        var overlaid = processor.CreateBarCodes(
            CreateParameters(TestVideoFileName, normal),
            ffmpeg,
            CancellationToken.None,
            overlayWaveform: true,
            overlayStrength: 0.7);

        try
        {
            Assert.AreEqual(0, CountDifferingPixels(plain[normal], overlaid[normal]));
        }
        finally
        {
            foreach (var bitmap in plain.Values.Concat(overlaid.Values))
            {
                bitmap.Dispose();
            }
        }
    }

    [Test]
    public void ImageStreamProcessor_SpectralBrightness_Applies_To_All_Modes()
    {
        CreateTestAudioVideoIfNecessary();
        var ffmpeg = new FfmpegWrapper(FfmpegExecutablePath);
        var processor = new ImageStreamProcessor();

        var normal = new MagicScalerBarGenerator("Normal", average: false);
        var scanline = new ScanlineBarGenerator("Scanline");

        var plain = processor.CreateBarCodes(
            CreateParameters(TestTwoToneFileName, normal, scanline),
            ffmpeg,
            CancellationToken.None);
        var graded = processor.CreateBarCodes(
            CreateParameters(TestTwoToneFileName, normal, scanline),
            ffmpeg,
            CancellationToken.None,
            spectralBrightness: true,
            brightnessIntensity: 0.7);

        try
        {
            CollectionAssert.AreEquivalent(plain.Keys.ToArray(), graded.Keys.ToArray());
            foreach (var generator in plain.Keys)
            {
                Assert.Greater(
                    CountDifferingPixels(plain[generator], graded[generator]),
                    0,
                    $"Expected spectral brightness to change the {generator.DisplayName} output.");
            }
        }
        finally
        {
            foreach (var bitmap in plain.Values.Concat(graded.Values))
            {
                bitmap.Dispose();
            }
        }
    }

    [Test]
    public void ImageStreamProcessor_SpectralBrightness_Silent_Input_Falls_Back_To_Plain()
    {
        // test.mkv has no audio track: grading is skipped with a warning.
        CreateTestVideoIfNecessary();
        var ffmpeg = new FfmpegWrapper(FfmpegExecutablePath);
        var processor = new ImageStreamProcessor();

        var normal = new MagicScalerBarGenerator("Normal", average: false);

        var plain = processor.CreateBarCodes(
            CreateParameters(TestVideoFileName, normal),
            ffmpeg,
            CancellationToken.None);
        var graded = processor.CreateBarCodes(
            CreateParameters(TestVideoFileName, normal),
            ffmpeg,
            CancellationToken.None,
            spectralBrightness: true,
            brightnessIntensity: 0.7);

        try
        {
            Assert.AreEqual(0, CountDifferingPixels(plain[normal], graded[normal]));
        }
        finally
        {
            foreach (var bitmap in plain.Values.Concat(graded.Values))
            {
                bitmap.Dispose();
            }
        }
    }

    [Test]
    public void ImageStreamProcessor_Smoothed_Changes_Normal_Output()
    {
        CreateTestAudioVideoIfNecessary();
        var ffmpeg = new FfmpegWrapper(FfmpegExecutablePath);
        var processor = new ImageStreamProcessor();

        var normal = new MagicScalerBarGenerator("Normal", average: false);

        var plain = processor.CreateBarCodes(
            CreateParameters(TestAudioVideoFileName, normal),
            ffmpeg,
            CancellationToken.None);
        var smoothed = processor.CreateBarCodes(
            CreateParameters(TestAudioVideoFileName, normal),
            ffmpeg,
            CancellationToken.None,
            smoothed: true);

        try
        {
            Assert.Greater(CountDifferingPixels(plain[normal], smoothed[normal]), 0);
        }
        finally
        {
            foreach (var bitmap in plain.Values.Concat(smoothed.Values))
            {
                bitmap.Dispose();
            }
        }
    }

    [Test]
    public void ImageStreamProcessor_Smoothed_Bars_Are_Vertically_Uniform()
    {
        CreateTestAudioVideoIfNecessary();
        var ffmpeg = new FfmpegWrapper(FfmpegExecutablePath);
        var processor = new ImageStreamProcessor();

        var normal = new MagicScalerBarGenerator("Normal", average: false);

        var result = processor.CreateBarCodes(
            CreateParameters(TestAudioVideoFileName, normal),
            ffmpeg,
            CancellationToken.None,
            smoothed: true);

        try
        {
            var bitmap = result[normal];
            for (int x = 0; x < bitmap.Width; x++)
            {
                int first = bitmap.GetPixel(x, 0).ToArgb();
                for (int y = 1; y < bitmap.Height; y++)
                {
                    Assert.AreEqual(first, bitmap.GetPixel(x, y).ToArgb(), $"Column {x} should be uniform after smoothing.");
                }
            }
        }
        finally
        {
            foreach (var bitmap in result.Values)
            {
                bitmap.Dispose();
            }
        }
    }

    [Test]
    public void ImageStreamProcessor_Smoothed_Leaves_Solid_Bars_Unchanged()
    {
        CreateTestAudioVideoIfNecessary();
        var ffmpeg = new FfmpegWrapper(FfmpegExecutablePath);
        var processor = new ImageStreamProcessor();

        var dominant = new DominantColorBarGenerator("Dominant color");

        var plain = processor.CreateBarCodes(
            CreateParameters(TestAudioVideoFileName, dominant),
            ffmpeg,
            CancellationToken.None);
        var smoothed = processor.CreateBarCodes(
            CreateParameters(TestAudioVideoFileName, dominant),
            ffmpeg,
            CancellationToken.None,
            smoothed: true);

        try
        {
            Assert.AreEqual(0, CountDifferingPixels(plain[dominant], smoothed[dominant]));
        }
        finally
        {
            foreach (var bitmap in plain.Values.Concat(smoothed.Values))
            {
                bitmap.Dispose();
            }
        }
    }

    [Test]
    public void ImageStreamProcessor_Cropped_Changes_Output_On_Letterboxed_Input()
    {
        CreateTestLetterboxIfNecessary();
        var ffmpeg = new FfmpegWrapper(FfmpegExecutablePath);
        var processor = new ImageStreamProcessor();

        var normal = new MagicScalerBarGenerator("Normal", average: false);

        var plain = processor.CreateBarCodes(
            CreateParameters(TestLetterboxFileName, normal),
            ffmpeg,
            CancellationToken.None);
        var cropped = processor.CreateBarCodes(
            CreateParameters(TestLetterboxFileName, normal),
            ffmpeg,
            CancellationToken.None,
            cropLetterbox: true);

        try
        {
            Assert.Greater(CountDifferingPixels(plain[normal], cropped[normal]), 0);
        }
        finally
        {
            foreach (var bitmap in plain.Values.Concat(cropped.Values))
            {
                bitmap.Dispose();
            }
        }
    }

    [Test]
    public void ImageStreamProcessor_Cropped_Letterboxed_Matches_Plain_Unletterboxed()
    {
        CreateTestLetterboxIfNecessary();
        var ffmpeg = new FfmpegWrapper(FfmpegExecutablePath);
        var processor = new ImageStreamProcessor();

        var normal = new MagicScalerBarGenerator("Normal", average: false);

        var plain = processor.CreateBarCodes(
            CreateParameters(TestAudioVideoFileName, normal),
            ffmpeg,
            CancellationToken.None);
        var cropped = processor.CreateBarCodes(
            CreateParameters(TestLetterboxFileName, normal),
            ffmpeg,
            CancellationToken.None,
            cropLetterbox: true);

        try
        {
            // Re-encoding the padded clip is lossy, so exact equality is out
            // of reach; the means must match to well under one LSB.
            Assert.Less(MeanAbsoluteDifference(plain[normal], cropped[normal]), 3.0);
        }
        finally
        {
            foreach (var bitmap in plain.Values.Concat(cropped.Values))
            {
                bitmap.Dispose();
            }
        }
    }

    [Test]
    public void ImageStreamProcessor_Cropped_Unletterboxed_Matches_Plain()
    {
        // No bars: frames pass through untouched, so the output is identical.
        CreateTestAudioVideoIfNecessary();
        var ffmpeg = new FfmpegWrapper(FfmpegExecutablePath);
        var processor = new ImageStreamProcessor();

        var normal = new MagicScalerBarGenerator("Normal", average: false);

        var plain = processor.CreateBarCodes(
            CreateParameters(TestAudioVideoFileName, normal),
            ffmpeg,
            CancellationToken.None);
        var cropped = processor.CreateBarCodes(
            CreateParameters(TestAudioVideoFileName, normal),
            ffmpeg,
            CancellationToken.None,
            cropLetterbox: true);

        try
        {
            Assert.AreEqual(0, CountDifferingPixels(plain[normal], cropped[normal]));
        }
        finally
        {
            foreach (var bitmap in plain.Values.Concat(cropped.Values))
            {
                bitmap.Dispose();
            }
        }
    }
}
