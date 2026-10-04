using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class CreditDetectorTests
{
    private static double[] Bright(params (double value, int seconds)[] runs)
    {
        var result = new List<double>();
        foreach (var run in runs)
        {
            result.AddRange(Enumerable.Repeat(run.value, run.seconds));
        }

        return result.ToArray();
    }

    private static double[] Edgy(params (double value, int seconds)[] runs)
    {
        return Bright(runs);
    }

    [Test]
    public void FindCreditsStart_Finds_Dark_Edgy_Tail()
    {
        var brightness = Bright((120.0, 300), (12.0, 120));
        var edges = Edgy((0.01, 300), (0.20, 120));

        Assert.AreEqual(300, CreditDetector.FindCreditsStart(brightness, edges));
    }

    [Test]
    public void FindCreditsStart_Tolerates_Trailing_Logo()
    {
        var brightness = Bright((120.0, 300), (12.0, 100), (150.0, 20));
        var edges = Edgy((0.01, 300), (0.20, 100), (0.02, 20));

        Assert.AreEqual(300, CreditDetector.FindCreditsStart(brightness, edges));
    }

    [Test]
    public void FindCreditsStart_Tolerates_Black_Gap_Inside_Credits()
    {
        var brightness = Bright((120.0, 300), (12.0, 60), (5.0, 8), (12.0, 52));
        var edges = Edgy((0.01, 300), (0.20, 60), (0.0, 8), (0.20, 52));

        Assert.AreEqual(300, CreditDetector.FindCreditsStart(brightness, edges));
    }

    [Test]
    public void FindCreditsStart_Returns_Minus_One_When_All_Bright()
    {
        var brightness = Bright((120.0, 420));
        var edges = Edgy((0.10, 420));

        Assert.AreEqual(-1, CreditDetector.FindCreditsStart(brightness, edges));
    }

    [Test]
    public void FindCreditsStart_Returns_Minus_One_For_Dark_Smooth_Tail()
    {
        var brightness = Bright((120.0, 300), (8.0, 120));
        var edges = Edgy((0.01, 300), (0.005, 120));

        Assert.AreEqual(-1, CreditDetector.FindCreditsStart(brightness, edges));
    }

    [Test]
    public void FindCreditsStart_Returns_Minus_One_For_Short_Blip()
    {
        var brightness = Bright((120.0, 415), (12.0, 5));
        var edges = Edgy((0.01, 415), (0.20, 5));

        Assert.AreEqual(-1, CreditDetector.FindCreditsStart(brightness, edges));
    }

    [Test]
    public void FindCreditsStart_Stops_At_Long_Bright_Gap()
    {
        var brightness = Bright((120.0, 200), (12.0, 100), (130.0, 30), (12.0, 80), (5.0, 10));
        var edges = Edgy((0.01, 200), (0.20, 100), (0.02, 30), (0.20, 80), (0.0, 10));

        Assert.AreEqual(330, CreditDetector.FindCreditsStart(brightness, edges));
    }

    [Test]
    public void FindCreditsStart_Returns_Minus_One_For_Empty_Or_Mismatched_Input()
    {
        Assert.AreEqual(-1, CreditDetector.FindCreditsStart(new double[0], new double[0]));
        Assert.AreEqual(-1, CreditDetector.FindCreditsStart(null, null));
        Assert.AreEqual(-1, CreditDetector.FindCreditsStart(new[] { 1.0 }, new[] { 1.0, 2.0 }));
    }

    private static Bitmap FrameWithHorizontalBars(int barCount)
    {
        var bitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        using (var black = new SolidBrush(Color.Black))
        {
            g.FillRectangle(black, 0, 0, 32, 32);
            using var white = new SolidBrush(Color.White);
            for (int i = 0; i < barCount; i++)
            {
                g.FillRectangle(white, new Rectangle(2, 4 + (i * 6), 28, 1));
            }
        }

        return bitmap;
    }

    [Test]
    public void ComputeEdgeDensity_Separates_Blank_And_Text_Like_Frames()
    {
        using var blank = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(blank))
        using (var black = new SolidBrush(Color.Black))
        {
            g.FillRectangle(black, 0, 0, 32, 32);
        }

        using var textLike = FrameWithHorizontalBars(4);

        double blankDensity = EdgeDetector.ComputeEdgeDensity(blank, 50);
        double textDensity = EdgeDetector.ComputeEdgeDensity(textLike, 50);

        Assert.That(blankDensity, Is.LessThan(0.05));
        Assert.That(textDensity, Is.GreaterThanOrEqualTo(0.05));
        Assert.That(textDensity, Is.GreaterThan(blankDensity));
    }
}
