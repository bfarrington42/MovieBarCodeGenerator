using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System;
using System.Drawing;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class ElevationColorTests
{
    [Test]
    public void PositionToColor_Ends_Are_Purple_And_White()
    {
        Assert.AreEqual(Color.FromArgb(75, 0, 130).ToArgb(), ElevationColor.PositionToColor(0).ToArgb());
        Assert.AreEqual(Color.White.ToArgb(), ElevationColor.PositionToColor(1).ToArgb());
    }

    [Test]
    public void PositionToColor_Knots_Return_Exact_Stops()
    {
        Assert.AreEqual(Color.FromArgb(60, 170, 70).ToArgb(), ElevationColor.PositionToColor(0.45).ToArgb());
    }

    [Test]
    public void PositionToColor_Midpoint_Is_Greenish()
    {
        var color = ElevationColor.PositionToColor(0.5);

        // Green dominates; R and B sit on rounding boundaries, so tolerate 1 LSB.
        Assert.AreEqual(180, color.G);
        Assert.That(color.R, Is.InRange(101, 103));
        Assert.That(color.B, Is.InRange(71, 73));
        Assert.Greater(color.G, color.R);
        Assert.Greater(color.G, color.B);
    }

    [Test]
    public void PositionToColor_Clamps_Outside_Range()
    {
        Assert.AreEqual(
            ElevationColor.PositionToColor(0).ToArgb(),
            ElevationColor.PositionToColor(-0.5).ToArgb());
        Assert.AreEqual(
            ElevationColor.PositionToColor(1).ToArgb(),
            ElevationColor.PositionToColor(1.5).ToArgb());
    }

    [Test]
    public void FrequencyToColor_CustomRange_Maps_Ends_And_Midpoint()
    {
        Assert.AreEqual(
            Color.FromArgb(75, 0, 130).ToArgb(),
            ElevationColor.FrequencyToColor(100, minFrequencyHz: 100, maxFrequencyHz: 1000).ToArgb());
        Assert.AreEqual(
            Color.White.ToArgb(),
            ElevationColor.FrequencyToColor(1000, minFrequencyHz: 100, maxFrequencyHz: 1000).ToArgb());

        double geometricMean = 100 * Math.Sqrt(1000.0 / 100);
        var viaFrequency = ElevationColor.FrequencyToColor(geometricMean, minFrequencyHz: 100, maxFrequencyHz: 1000);
        var direct = ElevationColor.PositionToColor(0.5);

        // Same midpoint up to float noise.
        Assert.That(Math.Abs(viaFrequency.R - direct.R), Is.LessThanOrEqualTo(1));
        Assert.That(Math.Abs(viaFrequency.G - direct.G), Is.LessThanOrEqualTo(1));
        Assert.That(Math.Abs(viaFrequency.B - direct.B), Is.LessThanOrEqualTo(1));
    }

    [Test]
    public void FrequencyToColor_CustomRange_Rejects_Invalid_Range()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ElevationColor.FrequencyToColor(100, minFrequencyHz: 0, maxFrequencyHz: 1000));
        Assert.Throws<ArgumentOutOfRangeException>(() => ElevationColor.FrequencyToColor(100, minFrequencyHz: 100, maxFrequencyHz: 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => ElevationColor.FrequencyToColor(100, minFrequencyHz: 500, maxFrequencyHz: 100));
    }
}
