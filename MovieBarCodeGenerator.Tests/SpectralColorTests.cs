using MovieBarCodeGenerator.Core;
using MovieBarCodeGenerator.Core.Generators;
using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class SpectralColorTests
{
    private static float[] SineSamples(float frequency, int count, int sampleRate = 8000)
    {
        var samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * frequency * i / sampleRate);
        }

        return samples;
    }

    [Test]
    public void WavelengthToRgb_Red_Is_Pure_Red()
    {
        Assert.AreEqual(Color.FromArgb(255, 0, 0).ToArgb(), SpectralColor.WavelengthToRgb(650).ToArgb());
    }

    [Test]
    public void WavelengthToRgb_Green_Dominates_At_550()
    {
        var color = SpectralColor.WavelengthToRgb(550);

        Assert.AreEqual(255, color.G);
        Assert.AreEqual(0, color.B);
        Assert.Greater(color.R, 0);
    }

    [Test]
    public void WavelengthToRgb_Blue_Dominates_At_475()
    {
        var color = SpectralColor.WavelengthToRgb(475);

        Assert.AreEqual(255, color.B);
        Assert.AreEqual(0, color.R);
        Assert.Greater(color.G, 0);
    }

    [Test]
    public void WavelengthToRgb_Invisible_Light_Is_Black()
    {
        Assert.AreEqual(Color.Black.ToArgb(), SpectralColor.WavelengthToRgb(300).ToArgb());
        Assert.AreEqual(Color.Black.ToArgb(), SpectralColor.WavelengthToRgb(900).ToArgb());
    }

    [Test]
    public void FrequencyToColor_Bass_Is_Red()
    {
        var color = SpectralColor.FrequencyToColor(55);

        Assert.Greater(color.R, 200);
        Assert.Less(color.G, 50);
        Assert.Less(color.B, 50);
    }

    [Test]
    public void FrequencyToColor_High_Treble_Is_Blue()
    {
        var color = SpectralColor.FrequencyToColor(4000);

        Assert.AreEqual(255, color.B);
        Assert.AreEqual(0, color.R);
        Assert.Greater(color.G, 0);
    }

    [Test]
    public void FrequencyToColor_Clamps_Outside_Hearing()
    {
        Assert.AreEqual(
            SpectralColor.FrequencyToColor(20).ToArgb(),
            SpectralColor.FrequencyToColor(5).ToArgb());
        Assert.AreEqual(
            SpectralColor.FrequencyToColor(20000).ToArgb(),
            SpectralColor.FrequencyToColor(30000).ToArgb());
    }

    [Test]
    public void Generate_Returns_Expected_Dimensions()
    {
        var generator = new SpectralColorBarGenerator("Audio spectrum");

        using var image = generator.Generate(SineSamples(440, 80000), sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        Assert.AreEqual(10, image.Width);
        Assert.AreEqual(32, image.Height);
    }

    [Test]
    public void Generate_Constant_Tone_Paints_Uniform_Mapped_Color()
    {
        var generator = new SpectralColorBarGenerator("Audio spectrum");
        var expected = SpectralColor.FrequencyToColor(440);

        Assert.AreNotEqual(Color.Black.ToArgb(), expected.ToArgb());

        using var image = generator.Generate(SineSamples(440, 80000), sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        using var bitmap = new Bitmap(image);
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Assert.AreEqual(expected.ToArgb(), bitmap.GetPixel(x, y).ToArgb(), $"Pixel ({x}, {y}) should match the mapped tone color.");
            }
        }
    }

    [Test]
    public void Generate_Silence_Renders_Black()
    {
        var generator = new SpectralColorBarGenerator("Audio spectrum");

        using var image = generator.Generate(new float[80000], sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        using var bitmap = new Bitmap(image);
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Assert.AreEqual(Color.Black.ToArgb(), bitmap.GetPixel(x, y).ToArgb());
            }
        }
    }

    [Test]
    public void Generate_Respects_BarWidth_Chunking()
    {
        var generator = new SpectralColorBarGenerator("Audio spectrum");

        using var image = generator.Generate(SineSamples(440, 80000), sampleRate: 8000, width: 100, height: 64, barWidth: 4);
        Assert.AreEqual(100, image.Width);
        Assert.AreEqual(64, image.Height);
    }

    [Test]
    public void Generate_Rejects_Invalid_Arguments()
    {
        var generator = new SpectralColorBarGenerator("Audio spectrum");
        var samples = SineSamples(440, 8000);

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 8000, width: 0, height: 32, barWidth: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 8000, width: 10, height: 0, barWidth: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 8000, width: 10, height: 32, barWidth: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 0, width: 10, height: 32, barWidth: 1));
    }

    [Test]
    public void RequiresAudio_Is_True()
    {
        IAudioBarGenerator generator = new SpectralColorBarGenerator("Audio spectrum");

        Assert.IsTrue(generator.RequiresAudio);
        Assert.AreEqual("_spectrum", generator.FileNameSuffix);
    }

    [Test]
    public void GetBar_Throws_For_Audio_Only_Generator()
    {
        var generator = new SpectralColorBarGenerator("Audio spectrum");

        using var stream = new BitmapStream(new MemoryStream(new byte[] { 1, 2, 3, 4 }));
        Assert.Throws<NotSupportedException>(() => generator.GetBar(stream, barWidth: 1, barHeight: 8));
    }

    [Test]
    public void FrequencyToColor_CustomRange_Maps_Ends_To_Red_And_Violet()
    {
        var dull = SpectralColor.FrequencyToColor(100, minFrequencyHz: 100, maxFrequencyHz: 1000);
        var bright = SpectralColor.FrequencyToColor(1000, minFrequencyHz: 100, maxFrequencyHz: 1000);

        Assert.Greater(dull.R, 50);
        Assert.AreEqual(0, dull.G);
        Assert.AreEqual(0, dull.B);

        Assert.AreEqual(bright.R, bright.B);
        Assert.Greater(bright.B, 50);
        Assert.AreEqual(0, bright.G);
    }

    [Test]
    public void FrequencyToColor_CustomRange_Rejects_Invalid_Range()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SpectralColor.FrequencyToColor(100, minFrequencyHz: 0, maxFrequencyHz: 1000));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpectralColor.FrequencyToColor(100, minFrequencyHz: 100, maxFrequencyHz: 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpectralColor.FrequencyToColor(100, minFrequencyHz: 500, maxFrequencyHz: 100));
    }

    [Test]
    public void Generate_Two_Tone_Spans_Red_To_Violet()
    {
        var generator = new SpectralColorBarGenerator("Audio spectrum");
        var samples = SineSamples(200, 40000).Concat(SineSamples(2000, 40000)).ToArray();

        using var image = generator.Generate(samples, sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        using var bitmap = new Bitmap(image);

        var dull = bitmap.GetPixel(0, 0);
        var bright = bitmap.GetPixel(9, 0);

        // Halves internally uniform (aligned halves, exact bins).
        for (int x = 0; x < 5; x++)
        {
            Assert.AreEqual(dull.ToArgb(), bitmap.GetPixel(x, 0).ToArgb());
        }

        for (int x = 5; x < 10; x++)
        {
            Assert.AreEqual(bright.ToArgb(), bitmap.GetPixel(x, 0).ToArgb());
        }

        Assert.Greater(dull.R, dull.B);
        Assert.GreaterOrEqual(bright.B, bright.R);
        Assert.AreNotEqual(dull.ToArgb(), bright.ToArgb());
    }

    [Test]
    public void Generate_Quiet_Bar_Renders_Black()
    {
        var generator = new SpectralColorBarGenerator("Audio spectrum");
        var loud = SineSamples(440, 72000);
        var quiet = SineSamples(440, 8000).Select(s => s * 1e-6f).ToArray();
        var samples = loud.Concat(quiet).ToArray();

        using var image = generator.Generate(samples, sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        using var bitmap = new Bitmap(image);

        Assert.AreNotEqual(Color.Black.ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
        for (int y = 0; y < bitmap.Height; y++)
        {
            Assert.AreEqual(Color.Black.ToArgb(), bitmap.GetPixel(9, y).ToArgb(), "Near-silent bar should gate to black.");
        }
    }

    [Test]
    public void Generate_Near_Uniform_Falls_Back_To_Absolute()
    {
        var generator = new SpectralColorBarGenerator("Audio spectrum");
        var samples = SineSamples(440, 40000).Concat(SineSamples(460, 40000)).ToArray();

        using var image = generator.Generate(samples, sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        using var bitmap = new Bitmap(image);

        int minR = 255, maxR = 0, minG = 255, maxG = 0, minB = 255, maxB = 0;
        for (int x = 0; x < bitmap.Width; x++)
        {
            var pixel = bitmap.GetPixel(x, 0);
            Assert.AreNotEqual(Color.Black.ToArgb(), pixel.ToArgb());
            minR = Math.Min(minR, pixel.R);
            maxR = Math.Max(maxR, pixel.R);
            minG = Math.Min(minG, pixel.G);
            maxG = Math.Max(maxG, pixel.G);
            minB = Math.Min(minB, pixel.B);
            maxB = Math.Max(maxB, pixel.B);
        }

        Assert.LessOrEqual(maxR - minR, 12);
        Assert.LessOrEqual(maxG - minG, 12);
        Assert.LessOrEqual(maxB - minB, 12);
    }
}
