using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System.Drawing;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class BlackbodyColorTests
{
    [Test]
    public void TemperatureToColor_Candlelight_Is_Ember_Orange()
    {
        var color = BlackbodyColor.TemperatureToColor(1000);

        Assert.AreEqual(255, color.R);
        Assert.AreEqual(68, color.G);
        Assert.AreEqual(0, color.B);
    }

    [Test]
    public void TemperatureToColor_Daylight_Is_Near_White()
    {
        var color = BlackbodyColor.TemperatureToColor(6500);

        Assert.AreEqual(255, color.R);
        Assert.Greater(color.G, 240);
        Assert.Greater(color.B, 230);
    }

    [Test]
    public void TemperatureToColor_Overcast_Is_Pale_Blue()
    {
        var color = BlackbodyColor.TemperatureToColor(10000);

        Assert.AreEqual(255, color.B);
        Assert.Less(color.R, 220);
        Assert.Greater(color.G, 200);
    }

    [Test]
    public void TemperatureToColor_Clamps_Outside_Range()
    {
        Assert.AreEqual(
            BlackbodyColor.TemperatureToColor(1000).ToArgb(),
            BlackbodyColor.TemperatureToColor(500).ToArgb());
        Assert.AreEqual(
            BlackbodyColor.TemperatureToColor(40000).ToArgb(),
            BlackbodyColor.TemperatureToColor(50000).ToArgb());
    }

    [Test]
    public void TemperatureToColor_Warmer_Means_Redder()
    {
        var cool = BlackbodyColor.TemperatureToColor(2000);
        var hot = BlackbodyColor.TemperatureToColor(8000);

        Assert.Greater(cool.R - cool.B, hot.R - hot.B);
    }
}
