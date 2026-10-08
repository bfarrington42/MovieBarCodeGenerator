# "Audio blackbody" generator

**Input source usage:** &check; Audio &cross; Video

Each bar is painted the [blackbody color](https://en.wikipedia.org/wiki/Planckian_locus) of its dominant audio frequency - ember orange for dull rumbles through neutral white to icy blue for the brightest treble.

## Pipeline overview

<pre>
source video file
  &rarr; extract mono audio track (ffmpeg, 8 kHz float samples)
  &rarr; split samples into one range per bar (same chunking as Spectral)
  &rarr; Hann-windowed FFT per range, take the spectral peak
  &rarr; normalize peak frequencies per movie (log scale)
  &rarr; map each position to a color temperature, then to RGB (Helland)
  &rarr; paint one solid column per bar
</pre>

Peak extraction, the 1% silence gate, the per-movie range with its 1.25x uniformity fallback, and silence rendered as black behavior are all shared with the [Spectral generator](spectral.md). Only the final color function differs. Where Spectral maps position $t$ onto light frequencies, this mode maps it onto temperatures:

$$T = 10^{\log_{10}(1500) + t \cdot (\log_{10}(10000) - \log_{10}(1500))}$$

so the dullest bar burns at 1500 K and the brightest at 10000 K.

## Tanner Helland's approximation

The code uses the following blackbody approximation presented by Tanner Helland [here](https://tannerhelland.com/2012/09/18/convert-temperature-rgb-algorithm-code.html), and is as he puts it _"a high-quality approximation, but it's not accurate enough for serious scientific use"_.

With $t = K / 100$:

$$R = 255 \qquad t \le 66$$

$$R = 329.698727446 \cdot (t-60)^{-0.1332047592} \qquad t > 66$$

$$G = 99.4708025861 \cdot \ln(t) - 161.1195681661 \qquad t \le 66$$

$$G = 288.1221695283 \cdot (t-60)^{-0.0755148492} \qquad t > 66$$

$$B = 255 \qquad t \ge 66$$

$$B = 0 \qquad t \le 19$$

$$B = 138.5177312231 \cdot \ln(t-10) - 305.0447927307 \qquad \text{otherwise}$$

Each channel is rounded and clamped to 0-255 with inputs outside 1000-40000 K clamped to the ends.

## Reading the output

- **Ember orange bars** - dull, rumbling passages
- **Warm-to-neutral white bars** - the midrange bulk of most mixes
- **Icy blue bars** - bright, airy treble
- **Black bars** - silence or near-silence
- **No greens, ever** - green sits off the Planckian locus, which is the whole visual difference from the Spectral generator. With this you get warm/cool grading instead of a rainbow. Expect largely whitish barcodes with colored ends.
