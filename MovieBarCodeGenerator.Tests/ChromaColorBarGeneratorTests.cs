using MovieBarCodeGenerator.Core;
using MovieBarCodeGenerator.Core.Generators;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class ChromaColorBarGeneratorTests
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
        var generator = new ChromaColorBarGenerator("Audio harmony");

        using var image = generator.Generate(SineSamples(440, 80000), sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        Assert.AreEqual(10, image.Width);
        Assert.AreEqual(32, image.Height);
    }

    [Test]
    public void Generate_Constant_Tone_Paints_Violet_Family()
    {
        var generator = new ChromaColorBarGenerator("Audio harmony");

        using var image = generator.Generate(SineSamples(440, 80000), sampleRate: 8000, width: 10, height: 32, barWidth: 1);
        using var bitmap = new Bitmap(image);
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                var pixel = bitmap.GetPixel(x, y);
                Assert.AreNotEqual(Color.Black.ToArgb(), pixel.ToArgb(), $"Pixel ({x}, {y}) should not be black.");
                Assert.AreEqual(255, pixel.B);
                Assert.That(pixel.R, Is.InRange(100, 160));
                Assert.LessOrEqual(pixel.G, 4);
            }
        }
    }

    [Test]
    public void Generate_Silence_Renders_Black()
    {
        var generator = new ChromaColorBarGenerator("Audio harmony");

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
        var generator = new ChromaColorBarGenerator("Audio harmony");
        var samples = SineSamples(440, 8000);

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 8000, width: 0, height: 32, barWidth: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 8000, width: 10, height: 0, barWidth: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 8000, width: 10, height: 32, barWidth: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(samples, sampleRate: 0, width: 10, height: 32, barWidth: 1));
    }

    [Test]
    public void RequiresAudio_Is_True()
    {
        IAudioBarGenerator generator = new ChromaColorBarGenerator("Audio harmony");

        Assert.IsTrue(generator.RequiresAudio);
        Assert.AreEqual("_harmony", generator.FileNameSuffix);
    }

    [Test]
    public void GetBar_Throws_For_Audio_Only_Generator()
    {
        var generator = new ChromaColorBarGenerator("Audio harmony");

        using var stream = new BitmapStream(new MemoryStream(new byte[] { 1, 2, 3, 4 }));
        Assert.Throws<NotSupportedException>(() => generator.GetBar(stream, barWidth: 1, barHeight: 8));
    }
}
