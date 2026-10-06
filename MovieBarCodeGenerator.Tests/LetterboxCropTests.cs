using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System;
using System.Drawing;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class LetterboxCropTests
{
    private static Bitmap LetterboxedFrame(int width, int height, int barHeight, Color content, Color? barColor = null)
    {
        var bitmap = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(barColor ?? Color.Black);
            using var brush = new SolidBrush(content);
            g.FillRectangle(brush, 0, barHeight, width, height - (2 * barHeight));
        }

        return bitmap;
    }

    [Test]
    public void CropFrame_Removes_Bars_Keeping_Content()
    {
        using var frame = LetterboxedFrame(width: 100, height: 100, barHeight: 12, Color.Red);
        using var cropped = LetterboxCrop.CropFrame(frame, cropFraction: 0.125);

        // (int)(100 * 0.125) = 12 rows top and bottom.
        Assert.AreEqual(100, cropped.Width);
        Assert.AreEqual(76, cropped.Height);
        Assert.AreEqual(Color.Red.ToArgb(), cropped.GetPixel(50, 0).ToArgb());
        Assert.AreEqual(Color.Red.ToArgb(), cropped.GetPixel(50, 75).ToArgb());
    }

    [Test]
    public void CropFrame_Preserves_Resolution()
    {
        using var frame = LetterboxedFrame(width: 100, height: 100, barHeight: 12, Color.Red);
        frame.SetResolution(96, 96);
        using var cropped = LetterboxCrop.CropFrame(frame, cropFraction: 0.125);

        Assert.AreEqual(frame.HorizontalResolution, cropped.HorizontalResolution);
        Assert.AreEqual(frame.VerticalResolution, cropped.VerticalResolution);
    }

    [Test]
    public void CropFrame_Tiny_Frame_Keeps_Dimensions()
    {
        using var frame = new Bitmap(4, 4);
        using var cropped = LetterboxCrop.CropFrame(frame, cropFraction: 0.125);

        // (int)(4 * 0.125) = 0: nothing to cut.
        Assert.AreEqual(4, cropped.Width);
        Assert.AreEqual(4, cropped.Height);
    }

    [Test]
    public void CropFrame_Rejects_Invalid_Arguments()
    {
        using var frame = new Bitmap(100, 100);
        Assert.Throws<ArgumentNullException>(() => LetterboxCrop.CropFrame(null, cropFraction: 0.125));
        Assert.Throws<ArgumentOutOfRangeException>(() => LetterboxCrop.CropFrame(frame, cropFraction: -0.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => LetterboxCrop.CropFrame(frame, cropFraction: 0.5));
    }

    [Test]
    public void DefaultCropFraction_Is_One_Eighth()
    {
        Assert.AreEqual(0.125, LetterboxCrop.DefaultCropFraction);
    }

    [Test]
    public void TryDetectBars_Finds_Exact_Bars()
    {
        using var frame = LetterboxedFrame(width: 100, height: 100, barHeight: 12, Color.Red);

        bool found = LetterboxCrop.TryDetectBars(frame, maxCropFraction: 0.4, out int topBars, out int bottomBars);

        Assert.IsTrue(found);
        Assert.AreEqual(12, topBars);
        Assert.AreEqual(12, bottomBars);
    }

    [Test]
    public void TryDetectBars_No_Bars_Returns_False()
    {
        using var frame = new Bitmap(100, 100);
        using (var g = Graphics.FromImage(frame))
        {
            g.Clear(Color.Red);
        }

        bool found = LetterboxCrop.TryDetectBars(frame, maxCropFraction: 0.4, out int topBars, out int bottomBars);

        Assert.IsFalse(found);
        Assert.AreEqual(0, topBars);
        Assert.AreEqual(0, bottomBars);
    }

    [Test]
    public void TryDetectBars_One_Sided_Bars_Returns_False()
    {
        using var frame = new Bitmap(100, 100);
        using (var g = Graphics.FromImage(frame))
        {
            g.Clear(Color.Red);
            using var brush = new SolidBrush(Color.Black);
            g.FillRectangle(brush, 0, 0, 100, 12);
        }

        bool found = LetterboxCrop.TryDetectBars(frame, maxCropFraction: 0.4, out int topBars, out int bottomBars);

        Assert.IsFalse(found);
        Assert.AreEqual(0, topBars);
        Assert.AreEqual(0, bottomBars);
    }

    [Test]
    public void TryDetectBars_All_Black_Frame_Returns_False()
    {
        using var frame = new Bitmap(100, 100);
        using (var g = Graphics.FromImage(frame))
        {
            g.Clear(Color.Black);
        }

        bool found = LetterboxCrop.TryDetectBars(frame, maxCropFraction: 0.4, out int topBars, out int bottomBars);

        Assert.IsFalse(found);
        Assert.AreEqual(0, topBars);
        Assert.AreEqual(0, bottomBars);
    }

    [Test]
    public void TryDetectBars_Limited_Range_Bars_Are_Found()
    {
        // Broadcast black is 16, not 0.
        using var frame = LetterboxedFrame(width: 100, height: 100, barHeight: 12, Color.Red, barColor: Color.FromArgb(16, 16, 16));

        bool found = LetterboxCrop.TryDetectBars(frame, maxCropFraction: 0.4, out int topBars, out int bottomBars);

        Assert.IsTrue(found);
        Assert.AreEqual(12, topBars);
        Assert.AreEqual(12, bottomBars);
    }

    [Test]
    public void TryDetectBars_Tolerates_Speckle_Noise()
    {
        using var frame = LetterboxedFrame(width: 100, height: 100, barHeight: 12, Color.Red);
        // One bright pixel per bar row: 99% still black.
        for (int y = 0; y < 12; y++)
        {
            frame.SetPixel(y, y, Color.White);
            frame.SetPixel(y, 99 - y, Color.White);
        }

        bool found = LetterboxCrop.TryDetectBars(frame, maxCropFraction: 0.4, out int topBars, out int bottomBars);

        Assert.IsTrue(found);
        Assert.AreEqual(12, topBars);
        Assert.AreEqual(12, bottomBars);
    }

    [Test]
    public void TryDetectBars_Dark_Gray_Is_Not_Black()
    {
        using var frame = LetterboxedFrame(width: 100, height: 100, barHeight: 12, Color.Red, barColor: Color.FromArgb(40, 40, 40));

        bool found = LetterboxCrop.TryDetectBars(frame, maxCropFraction: 0.4, out int topBars, out int bottomBars);

        Assert.IsFalse(found);
    }

    [Test]
    public void TryDetectBars_Rejects_Invalid_Arguments()
    {
        using var frame = new Bitmap(100, 100);
        Assert.Throws<ArgumentNullException>(() => LetterboxCrop.TryDetectBars(null, 0.4, out _, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => LetterboxCrop.TryDetectBars(frame, 0, out _, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => LetterboxCrop.TryDetectBars(frame, 0.5, out _, out _));
    }

    [Test]
    public void CropFrame_Without_Bars_Returns_Identical_Copy()
    {
        using var frame = new Bitmap(100, 100);
        using (var g = Graphics.FromImage(frame))
        {
            g.Clear(Color.Red);
        }

        using var cropped = LetterboxCrop.CropFrame(frame, cropFraction: 0.125);

        Assert.AreEqual(100, cropped.Width);
        Assert.AreEqual(100, cropped.Height);
        Assert.AreEqual(Color.Red.ToArgb(), cropped.GetPixel(50, 50).ToArgb());
    }

    [Test]
    public void CropToBars_Cuts_Exact_Rows()
    {
        using var frame = LetterboxedFrame(width: 100, height: 100, barHeight: 12, Color.Red);
        using var cropped = LetterboxCrop.CropToBars(frame, topBars: 12, bottomBars: 12);

        Assert.AreEqual(100, cropped.Width);
        Assert.AreEqual(76, cropped.Height);
        Assert.AreEqual(Color.Red.ToArgb(), cropped.GetPixel(50, 0).ToArgb());
    }

    [Test]
    public void CropToBars_Rejects_Invalid_Arguments()
    {
        using var frame = new Bitmap(100, 100);
        Assert.Throws<ArgumentNullException>(() => LetterboxCrop.CropToBars(null, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => LetterboxCrop.CropToBars(frame, -1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => LetterboxCrop.CropToBars(frame, 60, 60));
    }
}
