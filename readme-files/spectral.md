# "Spectral" generator

**Input source usage:** &check; Audio &cross; Video

Each bar of the output image is painted a solid color representing the **dominant audio frequency** of that slice of time. Bass renders red, mids pass through orange, yellow and green, and treble renders blue or violet. Silence renders as black.

## Pipeline overview

<pre>
source video file
  &rarr; extract mono audio track (ffmpeg, 8 kHz float samples)
  &rarr; split samples into one range per bar
  &rarr; Hann-windowed FFT per range, take the spectral peak
  &rarr; normalize peak frequencies per movie (log scale)
  &rarr; map each frequency to visible light, then to RGB (Bruton)
  &rarr; paint one solid column per bar
</pre>

## Split the audio into bar ranges

With $\text{barCount} = \max(1, \text{width} / \text{barWidth})$, bar $b$ owns samples $[b \cdot N / \text{barCount},\, (b+1) \cdot N / \text{barCount})$, where $N$ is the total sample count. This is the same proportional chunking the waveform code uses, so bars stay time-aligned across all audio features. Ranges shorter than 64 samples carry no meaningful spectral content and are rendered black.

## Dominant frequency per bar

Each range is [Hann-windowed](https://en.wikipedia.org/wiki/Hann_function) to tame edge artifacts:

$$w(n) = 0.5 \cdot \left(1 - \cos\left(\frac{2\pi n}{L - 1}\right)\right)$$

zero-padded to the next power of two, and run through a real FFT. The **dominant frequency** is the peak magnitude bin:

$$f = \frac{\text{peakBin} \cdot \text{sampleRate}}{\text{fftSize}}$$

This is monophonic, so one frequency wins per bar. Dense mixes (chords, full arrangements, etc.) jump between competing partials instead of blending them. The [Harmony generator](harmony.md) will give a more polyphonic view.

## Silence gating

Peak magnitudes are normalized by chunk length so bars are comparable, and the movie's loudest bar sets the reference. Any bar peaking below 1% of that reference is treated as silence and painted black.

## Per-movie normalization (log scale)

Significant peak frequencies are collected across the movie to find its own dullest ($\min$) and brightest ($\max$) tones, and each bar is placed on a perceptual log scale between them:

$$t = \frac{\log_{10}(f) - \log_{10}(\min)}{\log_{10}(\max) - \log_{10}(\min)}$$

so each movie always spans the full color range. The geometric mean of the movie's extremes lands exactly at $t = 0.5$. If the spread is narrower than ~1.25x (so audio with very little change), normalization would just stretch noise, so those movies fall back to the absolute 20Hz–20kHz hearing range instead.

## Audio frequency to visible light

$t = 0$ (dullest) maps to the red end of light and $t = 1$ (brightest) to the violet end, interpolating in log-frequency space between $c / 780\,\text{nm} \approx 384\,\text{THz}$ and $c / 380\,\text{nm} \approx 789\,\text{THz}$, then converting back to wavelength with $\lambda = c / F$. Resulting in a mapping from 
bass being red through treble being violet.

## Wavelength to RGB

[Dan Bruton's approximation](https://www.physics.sfasu.edu/astro/color/spectra.html) converts 380-780 nm to RGB piece-wise:

For wavelength $\lambda$ (nm), $380 \leq \lambda \leq 780$:

```math
(R,G,B)=
\begin{cases}
\left(\frac{440-\lambda}{60},\,0,\,1\right),
    &380\leq\lambda<440\
\left(0,\,\frac{\lambda-440}{50},\,1\right),
    &440\leq\lambda<490\
\left(0,\,1,\,\frac{510-\lambda}{20}\right),
    &490\leq\lambda<510\
\left(\frac{\lambda-510}{70},\,1,\,0\right),
    &510\leq\lambda<580\
\left(1,\,\frac{645-\lambda}{65},\,0\right),
    &580\leq\lambda<645\
(1,\,0,\,0),
    &645\leq\lambda\leq780
\end{cases}
```

The intensity factor is

```math
S(\lambda)=
\begin{cases}
0.3+0.7\frac{\lambda-380}{40},
    &380\leq\lambda<420\
1,
    &420\leq\lambda\leq700\
0.3+0.7\frac{780-\lambda}{80},
    &700<\lambda\leq780
\end{cases}
```

Apply $\gamma=0.8$ and scale to 8-bit RGB:

$$(R_8,G_8,B_8) = 255\left[(RS)^{0.8},\,(GS)^{0.8},\,(BS)^{0.8}\right]$$

Wavelengths outside 380–780 nm are mapped to black.

## Reading the output

- **Red/orange bars** - bass-heavy passages (kicks, bass lines, rumbles)
- **Yellow/green bars** - mids (most dialogue and melody lives here)
- **Blue/violet bars** - bright treble (cymbals, strings)
- **Black bars** - silence or near-silence

Audio is extracted at 8 kHz, so nothing above the 4 kHz [Nyquist limit](https://en.wikipedia.org/wiki/Nyquist_frequency) exists to map. The violet extreme is reachable, but only per-movie normalization can get you there. Raising the extraction rate would extend the top end at the cost of memory, but would make violet more obtainable.
