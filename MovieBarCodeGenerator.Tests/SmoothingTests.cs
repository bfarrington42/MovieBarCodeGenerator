using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System;
using System.Drawing;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class SmoothingTests
{
    [Test]
    public void SmoothVertically_Averages_Each_Column()
    {
        using var bitmap = new Bitmap(1, 4);
        bitmap.SetPixel(0, 0, Color.FromArgb(0, 0, 0));
        bitmap.SetPixel(0, 1, Color.FromArgb(85, 85, 85));
        bitmap.SetPixel(0, 2, Color.FromArgb(170, 170, 170));
        bitmap.SetPixel(0, 3, Color.FromArgb(255, 255, 255));

        Smoothing.SmoothVertically(bitmap);

        // (0 + 85 + 170 + 255) / 4 = 127 (truncated).
        for (int y = 0; y < bitmap.Height; y++)
        {
            Assert.AreEqual(Color.FromArgb(127, 127, 127).ToArgb(), bitmap.GetPixel(0, y).ToArgb(), $"Row {y} should hold the column average.");
        }
    }

    [Test]
    public void SmoothVertically_Leaves_Solid_Columns_Untouched()
    {
        using var bitmap = new Bitmap(3, 5);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Navy);
        }

        Smoothing.SmoothVertically(bitmap);

        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Assert.AreEqual(Color.Navy.ToArgb(), bitmap.GetPixel(x, y).ToArgb(), $"Pixel ({x}, {y}) should be untouched.");
            }
        }
    }

    [Test]
    public void SmoothVertically_Averages_Columns_Independently()
    {
        using var bitmap = new Bitmap(2, 2);
        bitmap.SetPixel(0, 0, Color.FromArgb(0, 0, 0));
        bitmap.SetPixel(0, 1, Color.FromArgb(200, 200, 200));
        bitmap.SetPixel(1, 0, Color.FromArgb(255, 0, 0));
        bitmap.SetPixel(1, 1, Color.FromArgb(255, 0, 0));

        Smoothing.SmoothVertically(bitmap);

        Assert.AreEqual(Color.FromArgb(100, 100, 100).ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
        Assert.AreEqual(Color.FromArgb(100, 100, 100).ToArgb(), bitmap.GetPixel(0, 1).ToArgb());
        Assert.AreEqual(Color.FromArgb(255, 0, 0).ToArgb(), bitmap.GetPixel(1, 0).ToArgb());
        Assert.AreEqual(Color.FromArgb(255, 0, 0).ToArgb(), bitmap.GetPixel(1, 1).ToArgb());
    }

    [Test]
    public void SmoothVertically_Rejects_Null()
    {
        Assert.Throws<ArgumentNullException>(() => Smoothing.SmoothVertically(null));
    }
}
