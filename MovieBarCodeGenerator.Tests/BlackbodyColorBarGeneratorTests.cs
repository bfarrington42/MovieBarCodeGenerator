using MovieBarCodeGenerator.Core;
using MovieBarCodeGenerator.Core.Generators;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class BlackbodyColorBarGeneratorTests
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
    public void Generate_Returns_Expected_Dimensions()
    {
        var generator = new BlackbodyColorBarGenerator("Audio blackbody");

        using var image = generator.Generate(SineSamples(440, 80000), sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        Assert.AreEqual(10, image.Width);
        Assert.AreEqual(32, image.Height);
    }

    [Test]
    public void Generate_Constant_Tone_Paints_Uniform_Color()
    {
        var generator = new BlackbodyColorBarGenerator("Audio blackbody");

        using var image = generator.Generate(SineSamples(440, 80000), sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        using var bitmap = new Bitmap(image);

        int first = bitmap.GetPixel(0, 0).ToArgb();
        Assert.AreNotEqual(Color.Black.ToArgb(), first);
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Assert.AreEqual(first, bitmap.GetPixel(x, y).ToArgb(), $"Pixel ({x}, {y}) should share the tone color.");
            }
        }
    }

    [Test]
    public void Generate_Two_Tone_Spans_Ember_To_Ice()
    {
        var generator = new BlackbodyColorBarGenerator("Audio blackbody");
        var samples = SineSamples(200, 40000).Concat(SineSamples(2000, 40000)).ToArray();

        using var image = generator.Generate(samples, sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        using var bitmap = new Bitmap(image);

        var dull = bitmap.GetPixel(0, 0);
        var bright = bitmap.GetPixel(9, 0);

        for (int x = 0; x < 5; x++)
        {
            Assert.AreEqual(dull.ToArgb(), bitmap.GetPixel(x, 0).ToArgb());
        }

        for (int x = 5; x < 10; x++)
        {
            Assert.AreEqual(bright.ToArgb(), bitmap.GetPixel(x, 0).ToArgb());
        }

        // Ember end is red-dominant; ice end carries full blue.
        Assert.Greater(dull.R - dull.B, bright.R - bright.B);
        Assert.AreEqual(255, bright.B);
    }

    [Test]
    public void Generate_Silence_Renders_Black()
    {
        var generator = new BlackbodyColorBarGenerator("Audio blackbody");

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
    public void Generate_Rejects_Invalid_Arguments()
    {
        var generator = new BlackbodyColorBarGenerator("Audio blackbody");
        var samples = SineSamples(440, 8000);

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 8000, width: 0, height: 32, barWidth: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 8000, width: 10, height: 0, barWidth: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 8000, width: 10, height: 32, barWidth: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 0, width: 10, height: 32, barWidth: 1));
    }

    [Test]
    public void RequiresAudio_Is_True()
    {
        IAudioBarGenerator generator = new BlackbodyColorBarGenerator("Audio blackbody");

        Assert.IsTrue(generator.RequiresAudio);
        Assert.AreEqual("_blackbody", generator.FileNameSuffix);
    }

    [Test]
    public void GetBar_Throws_For_Audio_Only_Generator()
    {
        var generator = new BlackbodyColorBarGenerator("Audio blackbody");

        using var stream = new BitmapStream(new MemoryStream(new byte[] { 1, 2, 3, 4 }));
        Assert.Throws<NotSupportedException>(() => generator.GetBar(stream, barWidth: 1, barHeight: 8));
    }
}
