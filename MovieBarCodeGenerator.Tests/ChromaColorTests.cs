using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System;
using System.Drawing;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class ChromaColorTests
{
    private static float[] SingleBin(int bin, float value = 10f)
    {
        var chroma = new float[ChromaColor.PitchClassCount];
        chroma[bin] = value;
        return chroma;
    }

    [Test]
    public void ChromaToColor_Pure_A_Maps_To_Violet()
    {
        // Bin 9 (A) = 270 degrees at full saturation.
        var color = ChromaColor.ChromaToColor(SingleBin(9));

        Assert.AreEqual(Color.FromArgb(128, 0, 255).ToArgb(), color.ToArgb());
    }

    [Test]
    public void ChromaToColor_Flat_Vector_Maps_To_White()
    {
        var chroma = new float[ChromaColor.PitchClassCount];
        for (int i = 0; i < chroma.Length; i++)
        {
            chroma[i] = 5f;
        }

        Assert.AreEqual(Color.White.ToArgb(), ChromaColor.ChromaToColor(chroma).ToArgb());
    }

    [Test]
    public void ChromaToColor_Silence_Maps_To_Black()
    {
        Assert.AreEqual(Color.Black.ToArgb(), ChromaColor.ChromaToColor(new float[ChromaColor.PitchClassCount]).ToArgb());
    }

    [Test]
    public void ChromaToColor_C_Maps_To_Red_Family()
    {
        var chroma = SingleBin(0);

        var color = ChromaColor.ChromaToColor(chroma);

        Assert.AreEqual(255, color.R);
        Assert.AreEqual(color.G, color.B);
        Assert.Less(color.G, 100);
    }

    [Test]
    public void ChromaToColor_Ties_Break_To_First_Bin()
    {
        var chroma = new float[ChromaColor.PitchClassCount];
        chroma[0] = 5f;
        chroma[7] = 5f;

        Assert.AreEqual(Color.FromArgb(255, 0, 0).ToArgb(), ChromaColor.ChromaToColor(chroma).ToArgb());
    }

    [Test]
    public void ChromaToColor_Rejects_Invalid_Vectors()
    {
        Assert.Throws<ArgumentNullException>(() => ChromaColor.ChromaToColor(null));
        Assert.Throws<ArgumentException>(() => ChromaColor.ChromaToColor(new float[11]));
    }
}
