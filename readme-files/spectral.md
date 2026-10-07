# Audio Spectrum Barcode ("Spectral" generator)

An **audio-only** barcode mode: it needs no video frames at all. Each bar of
the output image is painted a solid color representing the **dominant audio
frequency** of that slice of time — bass renders red, mids pass through
orange, yellow and green, and treble renders blue to violet. Silent stretches
render black.

## Pipeline overview

```
source video file
  → extract mono audio track (ffmpeg, 8 kHz float samples)
  → split samples into one range per bar
  → Hann-windowed FFT per range, take the spectral peak
  → normalize peak frequencies per movie (log scale)
  → map each frequency to visible light, then to RGB (Bruton)
  → paint one solid column per bar
```

## Step 1 — Split the audio into bar ranges

With $\text{barCount} = \max(1, \text{width} / \text{barWidth})$, bar $b$
owns samples $[b \cdot N / \text{barCount},\, (b+1) \cdot N / \text{barCount})$,
where $N$ is the total sample count — the same proportional chunking the
waveform code uses, so bars stay time-aligned across all audio features.
Ranges shorter than 64 samples carry no meaningful spectral content and
render black.

## Step 2 — Dominant frequency per bar

Each range is Hann-windowed to tame edge artifacts:

$$w(n) = 0.5 \cdot \left(1 - \cos\left(\frac{2\pi n}{L - 1}\right)\right)$$

zero-padded to the next power of two, and run through a real FFT. The
**dominant frequency** is the peak magnitude bin:

$$f = \frac{\text{peakBin} \cdot \text{sampleRate}}{\text{fftSize}}$$

This is a monophonic read: one frequency wins per bar. Dense mixes
(chords, full arrangements) jump between competing partials instead of
blending them — the Harmony generator exists for that polyphonic view.

## Step 3 — Silence gating

Peak magnitudes are normalized by chunk length so bars are comparable, and
the movie's loudest bar sets the reference. Any bar peaking below 1% of
that reference is treated as silence (or noise floor) and painted black
rather than given a junk color. Fully silent input therefore renders an
all-black image.

## Step 4 — Per-movie normalization (log scale)

Significant peak frequencies are collected across the movie to find its
own dullest ($\min$) and brightest ($\max$) tones, and each bar is placed
on a perceptual log scale between them:

$$t = \frac{\log_{10}(f) - \log_{10}(\min)}{\log_{10}(\max) - \log_{10}(\min)}$$

so the movie always spans the full color range. The geometric mean of the
movie's extremes lands exactly at $t = 0.5$. If the spread is narrower than
~1.25x (a test tone, an ambient drone), normalization would just stretch
measurement noise, so those movies fall back to the absolute 20Hz–20kHz
hearing range instead.

## Step 5 — Audio frequency to visible light

$t = 0$ (dullest) maps to the red end of light and $t = 1$ (brightest) to
the violet end, interpolating in log-frequency space between
$c / 780\,\text{nm} \approx 384\,\text{THz}$ and
$c / 380\,\text{nm} \approx 789\,\text{THz}$, then converting back to
wavelength with $\lambda = c / F$ ($c = 299792458\,\text{m/s}$). Bass → red
and treble → violet falls straight out of preserving the frequency
ordering.

## Step 6 — Wavelength to RGB (Bruton)

Dan Bruton's approximation converts 380–780 nm to RGB piece-wise, one
band per line (all values 0–1 before gamma):

$$R = -(\lambda-440)/60,\quad G = 0,\quad B = 1 \qquad 380 \le \lambda < 440$$

$$R = 0,\quad G = (\lambda-440)/50,\quad B = 1 \qquad 440 \le \lambda < 490$$

$$R = 0,\quad G = 1,\quad B = -(\lambda-510)/20 \qquad 490 \le \lambda < 510$$

$$R = (\lambda-510)/70,\quad G = 1,\quad B = 0 \qquad 510 \le \lambda < 580$$

$$R = 1,\quad G = -(\lambda-645)/65,\quad B = 0 \qquad 580 \le \lambda < 645$$

$$R = 1,\quad G = 0,\quad B = 0 \qquad 645 \le \lambda \le 780$$

Human vision fades at the edges, so a falloff factor applies below 420 nm
($0.3 + 0.7 \cdot (\lambda - 380)/40$) and above 700 nm
($0.3 + 0.7 \cdot (780 - \lambda)/80$), and each channel is
gamma-corrected with an exponent of 0.8:

$$\text{channel} = 255 \cdot (\text{channel} \cdot \text{factor})^{0.8}$$

Wavelengths outside 380–780 nm (invisible light) map to black.

## Reading the output

- **Red/orange bars** — bass-heavy passages (kicks, bass lines, rumbles).
- **Yellow/green bars** — midrange energy (most dialogue and melody lives here).
- **Blue/violet bars** — bright treble (cymbals, strings, air).
- **Black bars** — silence or near-silence.

One practical note: audio is extracted at 8 kHz, so nothing above the
4 kHz Nyquist limit exists to map — the violet extreme is reachable, but
only per-movie normalization gets you there, never absolute high treble.
Raising the extraction rate would extend the top end at the cost of memory.
