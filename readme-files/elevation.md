# "Audio elevation" generator

**Input source usage:** &check; Audio &cross; Video

Each bar is painted the elevation-tint color of its dominant audio frequency. Deep purple basins for dull rumbles rising through blue water, green lowlands, yellow foothills and brown highlands to white peaks for the brightest treble. It's a topographical map of the source material, in a way.

## Pipeline overview

<pre>
source video file
  &rarr; extract mono audio track (ffmpeg, 8 kHz float samples)
  &rarr; split samples into one range per bar (same chunking as Spectral)
  &rarr; Hann-windowed FFT per range, take the spectral peak
  &rarr; normalize peak frequencies per movie (log scale)
  &rarr; map each position onto the elevation ramp
  &rarr; paint one solid column per bar
</pre>

Peak extraction, the 1% silence gate, the per-movie range with its 1.25x uniformity fallback, and silence rendered as black behavior are all shared with the [Spectral generator](spectral.md). Only the color ramp differs here.

## The elevation ramp

Six control stops, linearly interpolated in RGB between neighbors. With $t$ the normalized position (0 = movie's dullest bar, 1 = brightest):

| $t$    | Color                       |
|--------|-----------------------------|
| 0.00   | deep purple (75, 0, 130)    |
| 0.25   | blue (30, 100, 170)         |
| 0.45   | green (60, 170, 70)         |
| 0.65   | yellow (230, 210, 80)       |
| 0.85   | brown (180, 110, 50)        |
| 1.00   | white (255, 255, 255)       |

Within segment $i$, each channel blends as $from + (to - from) \cdot \text{local}$, rounded to bytes. Knot positions return their stop colors exactly, and positions outside $[0, 1]$ clamp to the ends.

Because both the audio mapping and the ramp placement are symmetric, the movie's geometric-mean frequency lands exactly mid-ramp. The audio midpoint is centered on the color midpoint.

## Reading the output

- **Purple/blue bars** — dull, bass-heavy passages (the "basins").
- **Green/yellow bars** — the midrange bulk, where most mixes live.
- **Brown/white bars** — bright, airy treble (the "peaks").
- **Black bars** — silence or near-silence.

Unlike [Blackbody](blackbody.md), the midrange here stays colorful (green/yellow) rather than washing white.
