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

    // TODO: CLI tests (file patterns, various flags combinations...), parameters validation tests, and actual barcode creation tests.

    public string TestAudioVideoFileName = Path.Combine(TestContext.CurrentContext.TestDirectory, "test_av.mkv");

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
}
