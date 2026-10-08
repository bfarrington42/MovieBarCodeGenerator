# "Audio harmony" generator

**Input source usage:** &check; Audio &cross; Video

Each bar is painted the hue of its dominant pitch class. Clear tonal passages render as vivid colors that track harmony and key. Diffuse percussion (white noise) leans toward white, and silence renders black.

## Pipeline overview

<pre>
source video file
  &rarr; extract mono audio track (ffmpeg, 8 kHz float samples)
  &rarr; split samples into one range per bar (same chunking as Spectral)
  &rarr; fixed 2048-sample Hann-windowed center excerpt per bar
  &rarr; 12-bin chroma vector (NWaves ChromaExtractor, C-based)
  &rarr; dominant bin to hue, concentration to saturation
  &rarr; paint one solid column per bar
</pre>

## Chroma extraction

Each bar contributes a fixed 2048-sample window, which is the center excerpt of its range (about a quarter second at 8 kHz), and is zero-padded when the range is shorter. Ranges under 64 samples render black because their is too little audio to mean anything harmonically.

The window runs through NWaves' chroma extractor with a [Hann window](https://en.wikipedia.org/wiki/Hann_function) into 12 pitch-class bins ordered chromatically from C:

```
[C, C#, D, D#, E, F, F#, G, G#, A, A#, B]
```

so a pure A440 reads as a spike at bin 9. Fixed windows keep the frequency resolution identical for every bar, unlike the variable-length FFTs of the peak-based modes.

## Chroma vector to color

Assuming $M$ is the strongest bin and $m$ is the weakest of the 12. Silence maps straight to black. Otherwise:

$$\text{hue} = 30 \cdot \arg\max(\text{bins})$$

$$\text{saturation} = \frac{M - m}{M}$$

$$\text{value} = 1$$

converted to RGB in HSV space. In words, the winning pitch class picks one of twelve hues around the wheel (C being red and stepping 30 degrees per semitone). How *peaked* the distribution is determines the vividness. A pure tone is
fully saturated, white noise renders pure white, and ties go to the lowest bin.

## Reading the output

- **Vivid bands** - hue indicates pitch, so key changes and chord movement read as hue shifts
- **White/gray washes** - diffuse, percussive, or noisy passages with no dominant pitch class
- **Black bars** - silence

A2 and A5 share a bin, so unlike the [Spectral mode](spectral.md) this barcode shows *what* is playing harmonically, not *how high* it is.
