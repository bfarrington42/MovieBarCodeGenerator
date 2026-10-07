//Copyright 2026 Billy Farrington

//This file is part of MovieBarCodeGenerator.

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Mono.Options;
using MovieBarCodeGenerator.Core;
using MovieBarCodeGenerator.Core.Generators;
using PhotoSauce.MagicScaler;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.CLI;

public class CLIBatchProcessor
{
    private readonly BarCodeParametersValidator _barCodeParametersValidator = new();
    private readonly FfmpegWrapper _ffmpegWrapper = new(FfmpegWrapper.DefaultExecutableName);
    private readonly ImageStreamProcessor _imageProcessor = new();

    public async Task ProcessAsync(string[] args)
    {
        var arguments = new RawArguments();
        var allRawInputs = new List<string>();

        var options = new OptionSet();

        options.Add("?|help",
            "Show this help message.",
            x => ShowHelp(options));

        options.Add("in=|input=",
            @"Accepted inputs:
- a file path
- a directory path (only video files are picked up, see --extensions)
- a file pattern (simple '?' and '*' wildcards are accepted)
- a directory path followed by a file pattern
- an url
This parameter can be set multiple times.",
            x => allRawInputs.Add(x));

        options.Add("out=|output=",
            "Output file or directory. When a directory, each input file is saved there " +
            "as <inputname>.png (plus the generator suffix). Default: same folder as the input file.",
            x => arguments.RawOutput = x);

        options.Add("x|overwrite",
            "If set, existing files will be overwritten instead of being ignored.",
            x => arguments.Overwrite = true);

        options.Add("r|recursive",
            "If set, input is browsed recursively.",
            x => arguments.Recursive = true);

        options.Add("extensions=",
            $"Comma or semicolon separated video extensions used when an input is a plain directory " +
            $"(e.g. --extensions=mp4,mkv). Accepts \"mp4\", \".mp4\" or \"*.mp4\". " +
            $"Explicit file paths and wildcard patterns are always respected as given. " +
            $"Default: {string.Join(",", SupportedVideoExtensions.Default)}.",
            x => arguments.RawExtensions = x);

        options.Add("w=|width=",
            $"Width of the output image. Default: {RawArguments.DefaultWidth}",
            x => arguments.RawWidth = x);

        options.Add("h=|H=|height=",
            $"Height of the output image. Default: {RawArguments.DefaultHeight}",
            x => arguments.RawHeight = x);

        options.Add("b=|barwidth=|barWidth=",
            $"Width of each bar in the output image. Default: {RawArguments.DefaultBarWidth}",
            x => arguments.RawBarWidth = x);

        options.Add("postfix=",
            "Custom postfix appended to every output file name, after the generator suffix " +
            "and before the extension. E.g. --postfix=-banner gives filename-banner.png, " +
            "filename_legacy-banner.png, etc. Do not include a file extension. " +
            "Note: the generator suffix (_legacy, _scanline, ...) is omitted " +
            "when only one barcode mode is selected.",
            x => arguments.RawPostfix = x);

        options.Add("normal",
            "Generate a normal barcode.\nDefaults to True.\n(Use --normal- to set it to False)",
            x => arguments.GenerateNormal = x != null);

        options.Add("smoothed",
            "Average each bar vertically for a smoother barcode (applies to all selected modes).\nDefaults to False.",
            x => arguments.GenerateSmoothed = x != null);

        options.Add("legacy",
            "Generate a legacy barcode.\nDefaults to False.",
            x => arguments.GenerateLegacy = x != null);

        options.Add("scanline",
            "Generate a scanline barcode (middle row of each frame stretched into a bar).\nDefaults to False.",
            x => arguments.GenerateScanline = x != null);

        options.Add("vertical-sweep",
            "Generate a vertical-sweep barcode (a vertical column of each frame stretched into a bar, sweeping left to right across frames).\nDefaults to False.",
            x => arguments.GenerateVerticalSweep = x != null);

        options.Add("cropped",
            "Crop letterbox bars off each frame before generating (applies to all selected modes). Skipped when no bars are found.\nDefaults to False.",
            x => arguments.GenerateCropped = x != null);

        options.Add("dominant",
            "Generate a dominant-color barcode (each bar painted the most common color of its frame).\nDefaults to False.",
            x => arguments.GenerateDominant = x != null);

        options.Add("subject",
            "Generate a subject-color barcode (each bar painted the dominant color of the largest object in its frame).\nDefaults to False.",
            x => arguments.GenerateSubject = x != null);

        options.Add("spectrum",
            "Generate an audio spectrum barcode (each bar painted the visible color of its dominant audio frequency, normalized per movie, audio-only).\nDefaults to False.",
            x => arguments.GenerateSpectrum = x != null);

        options.Add("blackbody",
            "Generate an audio blackbody barcode (each bar painted the blackbody color of its dominant audio frequency, normalized per movie, audio-only).\nDefaults to False.",
            x => arguments.GenerateBlackbody = x != null);

        options.Add("harmony",
            "Generate an audio harmony barcode (each bar painted the hue of its dominant pitch class, audio-only).\nDefaults to False.",
            x => arguments.GenerateHarmony = x != null);

        options.Add("elevation",
            "Generate an audio elevation barcode (each bar painted the elevation color of its dominant audio frequency, normalized per movie, audio-only).\nDefaults to False.",
            x => arguments.GenerateElevation = x != null);

        options.Add("waveform-overlay",
            "Blend the audio waveform into every selected barcode in HSV (needs both video and audio).\nDefaults to False.",
            x => arguments.GenerateWaveformOverlay = x != null);

        options.Add("waveform-strength=",
            $"Waveform overlay strength, 0 to 1. Only used with --waveform-overlay.\nDefault: {ImageStreamProcessor.DefaultOverlayStrength}",
            x => arguments.RawWaveformStrength = x);

        options.Add("waveform-color=",
            "Waveform overlay color as hex: 6 digits (RRGGBB, e.g. FF0000 for red) or 8 digits (AARRGGBB), with or without a leading '#'.\nOnly used with --waveform-overlay. Defaults to white.",
            x => arguments.RawWaveformColor = x);

        options.Add("spectral-brightness",
            "Grade every selected barcode by the audio spectral centroid: brighter sound, brighter bar (needs both video and audio).\nDefaults to False.",
            x => arguments.GenerateSpectralBrightness = x != null);

        options.Add("brightness-intensity=",
            $"Spectral brightness intensity, 0.1 to 1. Only used with --spectral-brightness.\nDefault: {ImageStreamProcessor.DefaultBrightnessIntensity}",
            x => arguments.RawBrightnessIntensity = x);

        options.Add("exclude-credits",
            "Detect end credits by scanning the last 15 minutes for dark, text-heavy frames and stop the barcode where they begin.\nDefaults to False.",
            x => arguments.ExcludeCredits = x != null);

        try
        {
            options.Parse(args);
        }
        catch (OptionException ex)
        {
            Console.WriteLine($"Error: {ex.Message}\n");
            ShowHelp(options);
            return;
        }

        var fileSystemService = new FileSystemService();
        var parsedExtensions = SupportedVideoExtensions.ParseOrNull(arguments.RawExtensions);
        var directoryExtensionsFilter = (parsedExtensions == null || parsedExtensions.Count == 0)
            ? new HashSet<string>(SupportedVideoExtensions.Default, StringComparer.OrdinalIgnoreCase)
            : parsedExtensions;
        var expandedInputFileList = CLIUtils.GetExpandedAndValidatedFilePaths(fileSystemService, allRawInputs, arguments.Recursive, directoryExtensionsFilter)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (expandedInputFileList.Any())
        {
            foreach (var file in expandedInputFileList)
            {
                arguments.RawInput = file; // FIXME: copy instead of changing in place...
                try
                {
                    await DealWithOneInputFileAsync(arguments);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file '{file}': {ex}\nSkipping file...");
                }
            }
        }
        else
        {
            Console.WriteLine("No input.");
        }

        Console.WriteLine($"Exiting...");
    }

    /// <summary>
    /// Parses --waveform-strength (double) between 0.1 and 1.
    /// Missing or no value assumes default of 0.6
    /// Note, we're not allowing 0 because that would be pointless
    /// </summary>
    internal static bool TryParseWaveformStrength(string raw, out double strength)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            strength = ImageStreamProcessor.DefaultOverlayStrength;
            return true;
        }

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out strength)
            && strength >= 0.1 && strength <= 1)
        {
            return true;
        }

        strength = default;
        return false;
    }

    /// <summary>
    /// Parses --waveform-color: Hex color code (may include alpha), with or without a leading '#'.
    /// Missing or no value assumes solid white e.g., #FFFFFF
    /// </summary>
    internal static bool TryParseWaveformColor(string raw, out Color color)
    {
        color = Color.White;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        // uint: 8-digit white (FFFFFFFF) overflows int
        string hex = raw.Trim().TrimStart('#');
        if ((hex.Length == 6 || hex.Length == 8)
            && uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint parsed))
        {
            uint argb = hex.Length == 6 ? 0xFF000000u | parsed : parsed;
            color = Color.FromArgb(unchecked((int)argb));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parses --brightness-intensity (double) between 0.1 and 1.
    /// Missing or no value assumes default of 0.5
    /// Note, we're not allowing 0 because that would be pointless
    /// </summary>
    internal static bool TryParseBrightnessIntensity(string raw, out double intensity)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            intensity = ImageStreamProcessor.DefaultBrightnessIntensity;
            return true;
        }

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out intensity)
            && intensity >= 0.1 && intensity <= 1)
        {
            return true;
        }

        intensity = default;
        return false;
    }

    private async Task DealWithOneInputFileAsync(RawArguments arguments)
    {
        Console.WriteLine($"Processing file '{arguments.RawInput}':");

        var generators = new List<IBarGenerator>();

        if (arguments.GenerateNormal)
            generators.Add(new MagicScalerBarGenerator("Normal"));

        if (arguments.GenerateLegacy)
            generators.Add(GdiBarGenerator.CreateLegacy(average: false));

        if (arguments.GenerateScanline)
            generators.Add(new ScanlineBarGenerator("Scanline"));

        if (arguments.GenerateVerticalSweep)
            generators.Add(new VerticalSweepBarGenerator("Vertical sweep"));

        if (arguments.GenerateDominant)
            generators.Add(new DominantColorBarGenerator("Dominant color"));

        if (arguments.GenerateSubject)
            generators.Add(new SubjectColorBarGenerator("Subject color"));

        if (arguments.GenerateSpectrum)
            generators.Add(new SpectralColorBarGenerator("Audio spectrum"));

        if (arguments.GenerateBlackbody)
            generators.Add(new BlackbodyColorBarGenerator("Audio blackbody"));

        if (arguments.GenerateHarmony)
            generators.Add(new ChromaColorBarGenerator("Audio harmony"));

        if (arguments.GenerateElevation)
            generators.Add(new ElevationColorBarGenerator("Audio elevation"));

        if (!generators.Any())
        {
            Console.WriteLine("No generator.");
            return;
        }

        if (generators.All(g => g is IAudioBarGenerator) && arguments.ExcludeCredits)
        {
            Console.WriteLine("WARNING: --exclude-credits is ignored for audio-only runs (no video to trim).");
        }

        double overlayStrength = ImageStreamProcessor.DefaultOverlayStrength;
        Color overlayColor = Color.White;
        if (arguments.GenerateWaveformOverlay)
        {
            if (!TryParseWaveformStrength(arguments.RawWaveformStrength, out overlayStrength))
            {
                Console.Error.WriteLine($"Invalid --waveform-strength value '{arguments.RawWaveformStrength}': expected a number between 0.1 and 1.");
                return;
            }

            if (!TryParseWaveformColor(arguments.RawWaveformColor, out overlayColor))
            {
                Console.Error.WriteLine($"Invalid --waveform-color value '{arguments.RawWaveformColor}': expected 6 or 8 hex digits e.g., FF0000 or FFFFFF88, optionally prefixed with '#'.");
                return;
            }
        }
        else if (arguments.RawWaveformStrength != null || arguments.RawWaveformColor != null)
        {
            Console.WriteLine("NOTE: --waveform-strength and --waveform-color are ignored without --waveform-overlay.");
        }

        double brightnessIntensity = ImageStreamProcessor.DefaultBrightnessIntensity;
        if (arguments.GenerateSpectralBrightness)
        {
            if (!TryParseBrightnessIntensity(arguments.RawBrightnessIntensity, out brightnessIntensity))
            {
                Console.Error.WriteLine($"Invalid --brightness-intensity value '{arguments.RawBrightnessIntensity}': expected a number between 0.1 and 1.");
                return;
            }
        }
        else if (arguments.RawBrightnessIntensity != null)
        {
            Console.WriteLine("NOTE: --brightness-intensity is ignored without --spectral-brightness.");
        }

        IReadOnlyCollection<string> existingOutputs = Array.Empty<string>();
        BarCodeParameters parameters;
        try
        {
            parameters = _barCodeParametersValidator.GetValidatedParameters(
                rawInputPath: arguments.RawInput,
                rawBaseOutputPath: arguments.RawOutput,
                rawBarWidth: arguments.RawBarWidth,
                rawImageWidth: arguments.RawWidth,
                rawImageHeight: arguments.RawHeight,
                // Choosing whether to overwrite or not is done after validating parameters, not here
                shouldOverwriteOutputPaths: x => { existingOutputs = x; return true; },
                generators,
                fileNamePostfix: arguments.RawPostfix);
        }
        catch (ParameterValidationException ex)
        {
            Console.Error.WriteLine($"Invalid parameters: {ex.Message}");
            return;
        }

        if (existingOutputs.Any() && arguments.Overwrite == false)
        {
            // Check once before generating the image, and once just before saving.
            Console.WriteLine($"WARNING: skipped file {parameters.InputPath} because the output already exists. ({string.Join(", ", existingOutputs.Select(x => $"'{x}'"))})");
            return;
        }

        IReadOnlyDictionary<IBarGenerator, Bitmap> result = null;
        await Task.Run(() =>
        {
            result = _imageProcessor.CreateBarCodes(
                parameters,
                _ffmpegWrapper,
                CancellationToken.None,
                null,
                x => Console.WriteLine(x),
                excludeCredits: arguments.ExcludeCredits,
                overlayWaveform: arguments.GenerateWaveformOverlay,
                overlayStrength: overlayStrength,
                overlayWaveformColor: overlayColor,
                spectralBrightness: arguments.GenerateSpectralBrightness,
                brightnessIntensity: brightnessIntensity,
                smoothed: arguments.GenerateSmoothed,
                cropLetterbox: arguments.GenerateCropped);
            return result;
        }); // Image Magic throws if we are on an STA thread, so we have to execute everything on the thread pool and wait...

        foreach (var barcode in result)
        {
            try
            {
                var outputPath = parameters.GeneratorOutputPaths[barcode.Key];
                if (File.Exists(outputPath) && arguments.Overwrite == false)
                {
                    // Check once before generating the image, and once just before saving.
                    Console.WriteLine($"WARNING: skipped saving output file '{outputPath}' because it already exists.");
                }
                else
                {
                    var outputDir = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    barcode.Value.Save(outputPath);
                    Console.WriteLine($"File '{outputPath}' saved successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to save the image: {ex}");
            }
        }
    }

    private static void ShowHelp(OptionSet options)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();

        Console.WriteLine($@"Movie BarCode Generator {executingAssembly.GetName().Version}

Generate bar codes from movies. (concatenated movie frames in one image)

You can provide one input file, or a full directory,
along with an output file or directory.
");

        options.WriteOptionDescriptions(Console.Out);
    }
}

class RawArguments
{
    public const string DefaultWidth = "1000";
    public const string DefaultHeight = "256";
    public const string DefaultBarWidth = "1";
    public string RawInput { get; set; } = null;
    public string RawOutput { get; set; } = null;
    public bool Overwrite { get; set; } = false;
    public bool Recursive { get; set; } = false;
    public string RawWidth { get; set; } = DefaultWidth;
    public string RawHeight { get; set; } = DefaultHeight;
    public string RawBarWidth { get; set; } = DefaultBarWidth;
    public string RawExtensions { get; set; } = null;
    public string RawPostfix { get; set; } = null;

    public bool GenerateNormal { get; set; } = true;
    public bool GenerateSmoothed { get; set; }
    public bool GenerateLegacy { get; set; }
    public bool GenerateScanline { get; set; }
    public bool GenerateVerticalSweep { get; set; }
    public bool GenerateCropped { get; set; }
    public bool GenerateDominant { get; set; }
    public bool GenerateSubject { get; set; }
    public bool GenerateSpectrum { get; set; }
    public bool GenerateBlackbody { get; set; }
    public bool GenerateHarmony { get; set; }
    public bool GenerateElevation { get; set; }
    public bool GenerateWaveformOverlay { get; set; }
    public string RawWaveformStrength { get; set; } = null;
    public string RawWaveformColor { get; set; } = null;
    public bool GenerateSpectralBrightness { get; set; }
    public string RawBrightnessIntensity { get; set; } = null;
    public bool ExcludeCredits { get; set; }
}
