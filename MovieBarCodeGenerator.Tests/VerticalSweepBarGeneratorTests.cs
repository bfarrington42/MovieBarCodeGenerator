using MovieBarCodeGenerator.Core;
using MovieBarCodeGenerator.Core.Generators;
using NUnit.Framework;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class VerticalSweepBarGeneratorTests
{
    private static readonly Color[] ColumnColors =
    {
        Color.Red,
        Color.Lime,
        Color.Blue,
        Color.Yellow,
        Color.Magenta,
        Color.Cyan,
    };

    private static BitmapStream CreateStripedFrameStream()
    {
        var bitmap = new Bitmap(ColumnColors.Length, 4, PixelFormat.Format32bppArgb);
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                bitmap.SetPixel(x, y, ColumnColors[x]);
            }
        }

        var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Png);
        bitmap.Dispose();
        memory.Position = 0;
        return new BitmapStream(memory);
    }

    private static void AssertBarIsUniformColor(Image bar, Color expected)
    {
        using var bitmap = new Bitmap(bar);
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Assert.AreEqual(expected.ToArgb(), bitmap.GetPixel(x, y).ToArgb(), $"Pixel ({x}, {y})");
            }
        }
    }

    [Test]
    public void GetBar_Selects_Column_Matching_Frame_Index()
    {
        var generator = new VerticalSweepBarGenerator("Vertical sweep");

        for (int frameIndex = 0; frameIndex < ColumnColors.Length; frameIndex++)
        {
            using var stream = CreateStripedFrameStream();
            using var bar = generator.GetBar(stream, barWidth: 1, barHeight: 4, frameIndex, frameCount: ColumnColors.Length);
            AssertBarIsUniformColor(bar, ColumnColors[frameIndex]);
        }
    }

    [Test]
    public void GetBar_Wraps_Around_When_Frame_Index_Reaches_Frame_Width()
    {
        var generator = new VerticalSweepBarGenerator("Vertical sweep");

        using (var stream = CreateStripedFrameStream())
        using (var bar = generator.GetBar(stream, barWidth: 1, barHeight: 4, frameIndex: ColumnColors.Length, frameCount: ColumnColors.Length + 1))
        {
            AssertBarIsUniformColor(bar, ColumnColors[0]);
        }

        using (var stream = CreateStripedFrameStream())
        using (var bar = generator.GetBar(stream, barWidth: 1, barHeight: 4, frameIndex: ColumnColors.Length + 1, frameCount: ColumnColors.Length + 2))
        {
            AssertBarIsUniformColor(bar, ColumnColors[1]);
        }
    }

    [Test]
    public void GetBar_Without_Frame_Index_Defaults_To_First_Column()
    {
        var generator = new VerticalSweepBarGenerator("Vertical sweep");

        using var stream = CreateStripedFrameStream();
        using var bar = generator.GetBar(stream, barWidth: 1, barHeight: 4);
        AssertBarIsUniformColor(bar, ColumnColors[0]);
    }

    [TestCase(0, 6, 0)]
    [TestCase(5, 6, 5)]
    [TestCase(6, 6, 0)]
    [TestCase(13, 6, 1)]
    [TestCase(-1, 6, 5)]
    [TestCase(0, 0, 0)]
    [Test]
    public void ResolveColumn_Returns_Expected_Values(int frameIndex, int frameWidth, int expected)
    {
        Assert.AreEqual(expected, VerticalSweepBarGenerator.ResolveColumn(frameIndex, frameWidth));
    }
}
