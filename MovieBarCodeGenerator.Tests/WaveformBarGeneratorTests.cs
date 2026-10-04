using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System;
using System.Drawing;
using System.Linq;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class WaveformBarGeneratorTests
{
    private static float[] SineSamples(int count, double cycles = 4.0)
    {
        var samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * cycles * i / count);
        }

        return samples;
    }

    private static Bitmap DrawOnBlack(float[] samples, int width, int height, int barWidth, Color color)
    {
        var bitmap = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Black);
            WaveformBarGenerator.DrawWaveform(g, samples, width, height, barWidth, color);
        }

        return bitmap;
    }

    private static int CountNonBlackPixels(Bitmap bitmap)
    {
        int count = 0;
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                if (bitmap.GetPixel(x, y).ToArgb() != Color.Black.ToArgb())
                {
                    count++;
                }
            }
        }

        return count;
    }

    [Test]
    public void DrawWaveform_Draws_Waveform_For_Signal()
    {
        using var bitmap = DrawOnBlack(SineSamples(8000), width: 100, height: 64, barWidth: 1, Color.White);
        Assert.Greater(CountNonBlackPixels(bitmap), 0);
    }

    [Test]
    public void DrawWaveform_Silence_Draws_Nothing()
    {
        using var bitmap = DrawOnBlack(new float[8000], width: 100, height: 64, barWidth: 1, Color.White);
        // Silence contributes no pixels, leaving the underlying image untouched.
        Assert.AreEqual(0, CountNonBlackPixels(bitmap));
    }

    [Test]
    public void DrawWaveform_Louder_Signal_Covers_More_Rows()
    {
        var quiet = SineSamples(8000).Select(s => s * 0.1f).ToArray();
        using var quietBitmap = DrawOnBlack(quiet, width: 100, height: 64, barWidth: 1, Color.White);
        using var loudBitmap = DrawOnBlack(SineSamples(8000), width: 100, height: 64, barWidth: 1, Color.White);

        Assert.Greater(
            CountNonBlackPixels(loudBitmap),
            CountNonBlackPixels(quietBitmap));
    }

    [Test]
    public void DrawWaveform_Respects_BarWidth_Chunking()
    {
        using var bitmap = DrawOnBlack(SineSamples(8000), width: 100, height: 64, barWidth: 4, Color.White);
        Assert.AreEqual(100, bitmap.Width);
        Assert.AreEqual(64, bitmap.Height);
        Assert.Greater(CountNonBlackPixels(bitmap), 0);
    }

    [Test]
    public void DrawWaveform_Empty_Samples_Draws_Nothing()
    {
        using var bitmap = DrawOnBlack(Array.Empty<float>(), width: 100, height: 64, barWidth: 1, Color.White);
        Assert.AreEqual(0, CountNonBlackPixels(bitmap));
    }

    [Test]
    public void DrawWaveform_Rejects_Invalid_Dimensions()
    {
        using var bitmap = new Bitmap(10, 10);
        using var g = Graphics.FromImage(bitmap);
        Assert.Throws<ArgumentOutOfRangeException>(() => WaveformBarGenerator.DrawWaveform(g, SineSamples(100), width: 0, height: 10, barWidth: 1, Color.White));
        Assert.Throws<ArgumentOutOfRangeException>(() => WaveformBarGenerator.DrawWaveform(g, SineSamples(100), width: 10, height: 0, barWidth: 1, Color.White));
        Assert.Throws<ArgumentOutOfRangeException>(() => WaveformBarGenerator.DrawWaveform(g, SineSamples(100), width: 10, height: 10, barWidth: 0, Color.White));
    }

    [Test]
    public void DrawWaveform_Rejects_Null_Graphics()
    {
        Assert.Throws<ArgumentNullException>(() => WaveformBarGenerator.DrawWaveform(null, SineSamples(100), width: 10, height: 10, barWidth: 1, Color.White));
    }

    private static int CountOpaquePixels(Bitmap bitmap)
    {
        int count = 0;
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                if (bitmap.GetPixel(x, y).A != 0)
                {
                    count++;
                }
            }
        }

        return count;
    }

    [Test]
    public void RenderMask_Returns_White_On_Transparent_Mask()
    {
        using var mask = WaveformBarGenerator.RenderMask(SineSamples(8000), width: 100, height: 64, barWidth: 1);
        Assert.AreEqual(100, mask.Width);
        Assert.AreEqual(64, mask.Height);
        Assert.Greater(CountOpaquePixels(mask), 0);
    }

    [Test]
    public void RenderMask_Silence_Returns_Fully_Transparent()
    {
        using var mask = WaveformBarGenerator.RenderMask(new float[8000], width: 100, height: 64, barWidth: 1);
        Assert.AreEqual(0, CountOpaquePixels(mask));
    }

    [Test]
    public void RenderMask_Uses_Given_Color()
    {
        using var mask = WaveformBarGenerator.RenderMask(SineSamples(8000), width: 100, height: 64, barWidth: 1, Color.Red);
        int drawn = 0;
        for (int x = 0; x < mask.Width; x++)
        {
            for (int y = 0; y < mask.Height; y++)
            {
                var pixel = mask.GetPixel(x, y);
                if (pixel.A != 0)
                {
                    drawn++;
                    Assert.AreEqual(Color.Red.ToArgb(), pixel.ToArgb(), $"Mask pixel ({x}, {y}) should be exactly the given color.");
                }
            }
        }

        Assert.Greater(drawn, 0);
    }

    [Test]
    public void RenderMask_Black_Is_Opaque_Waveform_Not_Background()
    {
        using var mask = WaveformBarGenerator.RenderMask(SineSamples(8000), width: 100, height: 64, barWidth: 1, Color.Black);
        int drawn = 0;
        for (int x = 0; x < mask.Width; x++)
        {
            for (int y = 0; y < mask.Height; y++)
            {
                var pixel = mask.GetPixel(x, y);
                if (pixel.A != 0)
                {
                    drawn++;
                    Assert.AreEqual(Color.Black.ToArgb(), pixel.ToArgb(), $"Mask pixel ({x}, {y}) should be opaque black.");
                }
            }
        }

        Assert.Greater(drawn, 0);
    }

    [Test]
    public void RenderMask_Rejects_Invalid_Dimensions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WaveformBarGenerator.RenderMask(SineSamples(100), width: 0, height: 10, barWidth: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => WaveformBarGenerator.RenderMask(Array.Empty<float>(), width: 10, height: 10, barWidth: 0));
    }
}
