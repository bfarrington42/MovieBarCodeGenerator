using MovieBarCodeGenerator.Core;
using MovieBarCodeGenerator.Core.Generators;
using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class SubjectColorBarGeneratorTests
{
    private static BitmapStream CreateFrameStream(Color background, params (Color color, Rectangle rect)[] shapes)
    {
        var bitmap = new Bitmap(64, 64, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        using (var brush = new SolidBrush(background))
        {
            g.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
            foreach (var shape in shapes)
            {
                using var shapeBrush = new SolidBrush(shape.color);
                g.FillRectangle(shapeBrush, shape.rect);
            }
        }

        var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Png);
        bitmap.Dispose();
        memory.Position = 0;
        return new BitmapStream(memory);
    }

    private static void AssertBarIsUniformColor(Image bar, Color expected, int tolerance = 4)
    {
        using var bitmap = new Bitmap(bar);
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                var actual = bitmap.GetPixel(x, y);
                Assert.Multiple(() =>
                {
                    Assert.That(Math.Abs(actual.R - expected.R), Is.LessThanOrEqualTo(tolerance), $"Red at ({x}, {y})");
                    Assert.That(Math.Abs(actual.G - expected.G), Is.LessThanOrEqualTo(tolerance), $"Green at ({x}, {y})");
                    Assert.That(Math.Abs(actual.B - expected.B), Is.LessThanOrEqualTo(tolerance), $"Blue at ({x}, {y})");
                });
            }
        }
    }

    [Test]
    public void GetBar_Returns_Subject_Color_Instead_Of_Background()
    {
        var generator = new SubjectColorBarGenerator("Subject color");

        using var stream = CreateFrameStream(Color.Black, (Color.White, new Rectangle(16, 16, 32, 32)));
        using var bar = generator.GetBar(stream, barWidth: 4, barHeight: 4);
        AssertBarIsUniformColor(bar, Color.White);
    }

    [Test]
    public void GetBar_Whole_Frame_Dominant_Is_Background_For_Same_Frame()
    {
        var dominant = new DominantColorBarGenerator("Dominant color");

        using var stream = CreateFrameStream(Color.Black, (Color.White, new Rectangle(16, 16, 32, 32)));
        using var bar = dominant.GetBar(stream, barWidth: 4, barHeight: 4);
        AssertBarIsUniformColor(bar, Color.Black);
    }

    [Test]
    public void GetBar_Prefers_Larger_Blob()
    {
        var generator = new SubjectColorBarGenerator("Subject color");

        using var stream = CreateFrameStream(
            Color.Black,
            (Color.Red, new Rectangle(8, 8, 32, 32)),
            (Color.Blue, new Rectangle(44, 44, 12, 12)));
        using var bar = generator.GetBar(stream, barWidth: 4, barHeight: 4);
        AssertBarIsUniformColor(bar, Color.Red);
    }

    [Test]
    public void GetBar_Falls_Back_To_Background_When_No_Blob_Found()
    {
        var generator = new SubjectColorBarGenerator("Subject color");

        using var stream = CreateFrameStream(Color.Black);
        using var bar = generator.GetBar(stream, barWidth: 4, barHeight: 4);
        AssertBarIsUniformColor(bar, Color.Black);
    }

    [Test]
    public void GetBar_Ignores_Letterbox_Bars()
    {
        var generator = new SubjectColorBarGenerator("Subject color");

        var bitmap = new Bitmap(64, 64, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            using var black = new SolidBrush(Color.Black);
            g.FillRectangle(black, 0, 0, bitmap.Width, bitmap.Height);
            using var green = new SolidBrush(Color.Lime);
            g.FillRectangle(green, new Rectangle(16, 20, 32, 24));
        }

        var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Png);
        bitmap.Dispose();
        memory.Position = 0;

        using var stream = new BitmapStream(memory);
        using var bar = generator.GetBar(stream, barWidth: 4, barHeight: 4);
        AssertBarIsUniformColor(bar, Color.Lime);
    }
}
