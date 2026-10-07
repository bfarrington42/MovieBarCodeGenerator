using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System;
using System.Linq;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class AudioPeaksTests
{
    private static float[] SineSamples(float frequency, int count, int sampleRate = 8000)
    {
        var samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * frequency * i / sampleRate);
        }

        return samples;
    }

    [Test]
    public void Compute_Constant_Tone_Finds_Tone_Everywhere()
    {
        var envelope = AudioPeaks.Compute(SineSamples(440, 80000), sampleRate: 8000, width: 10, barWidth: 1);

        Assert.AreEqual(10, envelope.Frequencies.Length);
        Assert.IsTrue(envelope.Frequencies.All(f => Math.Abs(f - 440) < 5));
        Assert.AreEqual(envelope.Frequencies.Length, envelope.Levels.Length);
        Assert.Greater(envelope.MaxLevel, 0);
    }

    [Test]
    public void Compute_Two_Tone_Spans_Peaks()
    {
        var samples = SineSamples(200, 40000).Concat(SineSamples(2000, 40000)).ToArray();

        var envelope = AudioPeaks.Compute(samples, sampleRate: 8000, width: 10, barWidth: 1);

        Assert.That(envelope.MinFrequency, Is.EqualTo(200).Within(5));
        Assert.That(envelope.MaxFrequency, Is.EqualTo(2000).Within(5));
    }

    [Test]
    public void Compute_Silence_Yields_NaN_And_Absolute_Range()
    {
        var envelope = AudioPeaks.Compute(new float[80000], sampleRate: 8000, width: 10, barWidth: 1);

        Assert.IsTrue(envelope.Frequencies.All(double.IsNaN));
        Assert.AreEqual(0, envelope.MaxLevel);
        Assert.AreEqual(20.0, envelope.MinFrequency);
        Assert.AreEqual(20000.0, envelope.MaxFrequency);
    }

    [Test]
    public void Compute_Empty_Samples_Yields_NaN_And_Absolute_Range()
    {
        var envelope = AudioPeaks.Compute(Array.Empty<float>(), sampleRate: 8000, width: 100, barWidth: 4);

        Assert.AreEqual(25, envelope.Frequencies.Length);
        Assert.IsTrue(envelope.Frequencies.All(double.IsNaN));
        Assert.AreEqual(20.0, envelope.MinFrequency);
        Assert.AreEqual(20000.0, envelope.MaxFrequency);
    }

    [Test]
    public void Compute_Rejects_Invalid_Arguments()
    {
        var samples = SineSamples(440, 8000);

        Assert.Throws<ArgumentOutOfRangeException>(() => AudioPeaks.Compute(samples, sampleRate: 8000, width: 0, barWidth: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => AudioPeaks.Compute(samples, sampleRate: 8000, width: 10, barWidth: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => AudioPeaks.Compute(samples, sampleRate: 0, width: 10, barWidth: 1));
    }
}
