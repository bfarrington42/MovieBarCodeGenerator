using MovieBarCodeGenerator.Core.Utils;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace MovieBarCodeGenerator.Tests;

[TestFixture]
public class SpectralBrightnessTests
{
    private const int SampleRate = 8000;

    private static float[] Sine(float frequency, int count)
    {
        var samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * frequency * i / SampleRate);
        }

        return samples;
    }

    private static float[] RisingSweep(int count, float fromFrequency, float toFrequency)
    {
        var samples = new float[count];
        double duration = (double)count / SampleRate;
        for (int i = 0; i < count; i++)
        {
            double t = (double)i / SampleRate;
            double phase = 2 * Math.PI * (fromFrequency * t + (toFrequency - fromFrequency) * t * t / (2 * duration));
            samples[i] = (float)Math.Sin(phase);
        }

        return samples;
    }

    [Test]
    public void ComputeGains_Rising_Sweep_Increases_Gains()
    {
        float[] gains = SpectralBrightness.ComputeGains(RisingSweep(8000, 200, 2000), SampleRate, width: 20, barWidth: 1, intensity: 1.0);

        Assert.AreEqual(20, gains.Length);
        Assert.Less(gains[0], gains[gains.Length - 1], "Brighter sound should mean a brighter bar.");
        Assert.Greater(gains.Max() - gains.Min(), 0.2);
    }

    [Test]
    public void ComputeGains_Two_Tone_Halves_Bracket_Neutral()
    {
        var samples = Sine(200, 4000).Concat(Sine(2000, 4000)).ToArray();

        float[] gains = SpectralBrightness.ComputeGains(samples, SampleRate, width: 20, barWidth: 1, intensity: 1.0);

        Assert.AreEqual(20, gains.Length);
        for (int i = 0; i < 10; i++)
        {
            Assert.Less(gains[i], 1f, $"Dull bar {i} should be darkened.");
        }

        for (int i = 10; i < 20; i++)
        {
            Assert.Greater(gains[i], 1f, $"Bright bar {i} should be brightened.");
        }

        Assert.That(gains[0], Is.EqualTo(0.0).Within(0.1));
        Assert.That(gains[19], Is.EqualTo(2.0).Within(0.1));
    }

    [Test]
    public void ComputeGains_Outlier_Does_Not_Compress_Bulk()
    {
        // Gradient bulk with one bright blast bar: percentile normalization
        // anchors to the bulk, so mid-gradient bars still grade brightly
        // while the blast clamps to the extreme.
        const int bars = 100;
        const int samplesPerBar = 800;
        var samples = new float[bars * samplesPerBar];
        for (int bar = 0; bar < bars; bar++)
        {
            float frequency = bar < bars - 1 ? 200 + 600f * bar / (bars - 1) : 3500;
            for (int i = 0; i < samplesPerBar; i++)
            {
                int index = bar * samplesPerBar + i;
                samples[index] = (float)Math.Sin(2 * Math.PI * frequency * index / SampleRate);
            }
        }

        float[] gains = SpectralBrightness.ComputeGains(samples, SampleRate, width: bars, barWidth: 1, intensity: 1.0);

        Assert.AreEqual(bars, gains.Length);
        Assert.AreEqual(0f, gains[0]);
        Assert.Greater(gains[50], 1.2, "Mid-gradient bulk should grade brightly despite the outlier.");
        Assert.AreEqual(2f, gains[bars - 1]);
    }

    [Test]
    public void ComputeGains_Silence_Returns_Neutral()
    {
        float[] gains = SpectralBrightness.ComputeGains(new float[8000], SampleRate, width: 20, barWidth: 1, intensity: 1.0);

        Assert.AreEqual(20, gains.Length);
        Assert.IsTrue(gains.All(g => g == 1f));
    }

    [Test]
    public void ComputeGains_Constant_Tone_Returns_Neutral()
    {
        float[] gains = SpectralBrightness.ComputeGains(Sine(440, 8000), SampleRate, width: 20, barWidth: 1, intensity: 1.0);

        Assert.IsTrue(gains.All(g => g == 1f));
    }

    [Test]
    public void ComputeGains_Near_Uniform_Audio_Returns_Neutral()
    {
        // Two close tones: the spread is real but far below the grading
        // floor, so codec-jitter-like uniformity must not stretch it.
        var samples = new float[8000];
        var tone440 = Sine(440, 8000);
        var tone460 = Sine(460, 8000);
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = 0.9f * tone440[i] + 0.1f * tone460[i];
        }

        float[] gains = SpectralBrightness.ComputeGains(samples, SampleRate, width: 20, barWidth: 1, intensity: 1.0);

        Assert.IsTrue(gains.All(g => g == 1f));
    }

    [Test]
    public void ComputeGains_Moderate_Spread_Grades_Normally()
    {
        // 440Hz vs 600Hz clears the grading floor, unlike the near-uniform case.
        var samples = Sine(440, 4000).Concat(Sine(600, 4000)).ToArray();

        float[] gains = SpectralBrightness.ComputeGains(samples, SampleRate, width: 20, barWidth: 1, intensity: 1.0);

        for (int i = 0; i < 10; i++)
        {
            Assert.Less(gains[i], 1f, $"Dull bar {i} should be darkened.");
        }

        for (int i = 10; i < 20; i++)
        {
            Assert.Greater(gains[i], 1f, $"Bright bar {i} should be brightened.");
        }
    }

    [Test]
    public void ComputeGains_Zero_Intensity_Returns_Neutral()
    {
        float[] gains = SpectralBrightness.ComputeGains(RisingSweep(8000, 200, 2000), SampleRate, width: 20, barWidth: 1, intensity: 0);

        Assert.IsTrue(gains.All(g => g == 1f));
    }

    [Test]
    public void ComputeGains_Empty_Samples_Returns_Neutral_With_Expected_Length()
    {
        float[] gains = SpectralBrightness.ComputeGains(Array.Empty<float>(), SampleRate, width: 100, barWidth: 4, intensity: 1.0);

        Assert.AreEqual(25, gains.Length);
        Assert.IsTrue(gains.All(g => g == 1f));
    }

    [Test]
    public void ComputeGains_Gains_Stay_Within_Intensity_Bounds()
    {
        float[] full = SpectralBrightness.ComputeGains(RisingSweep(8000, 200, 2000), SampleRate, width: 20, barWidth: 1, intensity: 1.0);
        float[] half = SpectralBrightness.ComputeGains(RisingSweep(8000, 200, 2000), SampleRate, width: 20, barWidth: 1, intensity: 0.5);

        Assert.IsTrue(full.All(g => g >= 0f && g <= 2f));
        Assert.IsTrue(half.All(g => g >= 0.5f && g <= 1.5f));
    }

    [Test]
    public void ComputeGains_Rejects_Invalid_Arguments()
    {
        var samples = Sine(440, 8000);
        Assert.Throws<ArgumentOutOfRangeException>(() => SpectralBrightness.ComputeGains(samples, SampleRate, width: 0, barWidth: 1, intensity: 1.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpectralBrightness.ComputeGains(samples, SampleRate, width: 20, barWidth: 0, intensity: 1.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpectralBrightness.ComputeGains(samples, sampleRate: 0, width: 20, barWidth: 1, intensity: 1.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpectralBrightness.ComputeGains(samples, SampleRate, width: 20, barWidth: 1, intensity: -0.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpectralBrightness.ComputeGains(samples, SampleRate, width: 20, barWidth: 1, intensity: 1.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SpectralBrightness.ComputeGains(samples, SampleRate, width: 20, barWidth: 1, intensity: double.NaN));
    }

    [Test]
    public void ComputeGains_Honors_Cancellation()
    {
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        Assert.Throws<OperationCanceledException>(() => SpectralBrightness.ComputeGains(Sine(440, 8000), SampleRate, width: 20, barWidth: 1, intensity: 1.0, cancelled.Token));
    }
}
