using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System;
using System.Drawing;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class HsvColorTests
{
    private static Bitmap SolidBarcode(int width, int height, Color color)
    {
        var bitmap = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(color);
        }

        return bitmap;
    }

    [TestCase(255, 255, 255, 0, 0, 1)]
    [TestCase(0, 0, 0, 0, 0, 0)]
    [TestCase(255, 0, 0, 0, 1, 1)]
    [TestCase(0, 255, 0, 120, 1, 1)]
    [TestCase(0, 0, 255, 240, 1, 1)]
    [TestCase(0, 0, 128, 240, 1, 128 / 255.0)]
    [Test]
    public void RgbToHsv_Returns_Expected_Values(byte r, byte g, byte b, double h, double s, double v)
    {
        HsvColor.RgbToHsv(r, g, b, out double actualH, out double actualS, out double actualV);

        Assert.That(actualH, Is.EqualTo(h).Within(0.5));
        Assert.That(actualS, Is.EqualTo(s).Within(0.01));
        Assert.That(actualV, Is.EqualTo(v).Within(0.01));
    }

    [TestCase(255, 255, 255)]
    [TestCase(0, 0, 0)]
    [TestCase(255, 0, 0)]
    [TestCase(0, 255, 0)]
    [TestCase(0, 0, 255)]
    [TestCase(0, 0, 128)]
    [TestCase(122, 122, 204)]
    [Test]
    public void Hsv_Round_Trip_Preserves_Color(byte r, byte g, byte b)
    {
        HsvColor.RgbToHsv(r, g, b, out double h, out double s, out double v);
        HsvColor.HsvToRgb(h, s, v, out byte actualR, out byte actualG, out byte actualB);

        Assert.AreEqual(r, actualR);
        Assert.AreEqual(g, actualG);
        Assert.AreEqual(b, actualB);
    }

    [Test]
    public void ApplyColumnGains_Unit_Gains_Leave_Image_Untouched()
    {
        using var bitmap = SolidBarcode(width: 10, height: 8, Color.Navy);
        HsvColor.ApplyColumnGains(bitmap, new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }, barWidth: 1);

        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Assert.AreEqual(Color.Navy.ToArgb(), bitmap.GetPixel(x, y).ToArgb(), $"Pixel ({x}, {y}) should be untouched.");
            }
        }
    }

    [Test]
    public void ApplyColumnGains_Scales_Value_Keeping_Hue_And_Saturation()
    {
        using var bitmap = SolidBarcode(width: 4, height: 4, Color.Navy);
        HsvColor.ApplyColumnGains(bitmap, new float[] { 0.5f }, barWidth: 1);

        // Navy (0,0,128) at half value.
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Assert.AreEqual(Color.FromArgb(0, 0, 64).ToArgb(), bitmap.GetPixel(x, y).ToArgb(), $"Pixel ({x}, {y}) should be darkened.");
            }
        }
    }

    [Test]
    public void ApplyColumnGains_Clamps_Value_At_Full_Brightness()
    {
        using var bitmap = SolidBarcode(width: 4, height: 4, Color.Navy);
        HsvColor.ApplyColumnGains(bitmap, new float[] { 2f }, barWidth: 1);

        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Assert.AreEqual(Color.Blue.ToArgb(), bitmap.GetPixel(x, y).ToArgb(), $"Pixel ({x}, {y}) should be navy at full value.");
            }
        }
    }

    [Test]
    public void ApplyColumnGains_Maps_Columns_By_BarWidth()
    {
        using var bitmap = SolidBarcode(width: 4, height: 2, Color.Navy);
        HsvColor.ApplyColumnGains(bitmap, new float[] { 0.5f, 2f }, barWidth: 2);

        for (int y = 0; y < bitmap.Height; y++)
        {
            Assert.AreEqual(Color.FromArgb(0, 0, 64).ToArgb(), bitmap.GetPixel(0, y).ToArgb());
            Assert.AreEqual(Color.FromArgb(0, 0, 64).ToArgb(), bitmap.GetPixel(1, y).ToArgb());
            Assert.AreEqual(Color.Blue.ToArgb(), bitmap.GetPixel(2, y).ToArgb());
            Assert.AreEqual(Color.Blue.ToArgb(), bitmap.GetPixel(3, y).ToArgb());
        }
    }

    [Test]
    public void ApplyColumnGains_Null_Or_Empty_Gains_Are_No_Ops()
    {
        using var bitmap = SolidBarcode(width: 4, height: 4, Color.Navy);
        HsvColor.ApplyColumnGains(bitmap, null, barWidth: 1);
        HsvColor.ApplyColumnGains(bitmap, Array.Empty<float>(), barWidth: 1);

        Assert.AreEqual(Color.Navy.ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
    }

    [Test]
    public void ApplyColumnGains_Rejects_Invalid_Arguments()
    {
        using var bitmap = SolidBarcode(width: 4, height: 4, Color.Navy);
        Assert.Throws<ArgumentNullException>(() => HsvColor.ApplyColumnGains(null, new float[] { 1f }, barWidth: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => HsvColor.ApplyColumnGains(bitmap, new float[] { 1f }, barWidth: 0));
    }
}
