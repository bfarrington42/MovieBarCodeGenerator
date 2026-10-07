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

using MovieBarCodeGenerator.Core;
using MovieBarCodeGenerator.Core.Generators;
using MovieBarCodeGenerator.Properties;
using PhotoSauce.MagicScaler;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.GUI;

public partial class MainForm : Krypton.Toolkit.KryptonForm
{
    private const string GenerateButtonText = "Generate!";
    private const string CancelButtonText = "Cancel";

    private readonly OpenFileDialog _openFileDialog;
    private readonly SaveFileDialog _saveFileDialog;
    private readonly FfmpegWrapper _ffmpegWrapper;
    private readonly ImageStreamProcessor _imageProcessor;
    private readonly BarCodeParametersValidator _barCodeParametersValidator;

    private CancellationTokenSource _cancellationTokenSource;

    private readonly Color _progressTrackDarkColor;
    private readonly LogForm _logForm;
    private bool _logUserMoved;
    private bool _dockingLog;
    private readonly Image _bulbLit;
    private readonly Image _bulbUnlit;
    private readonly Image _aboutImage;

    private readonly List<BarGeneratorViewModel> _barGenerators;

    public MainForm()
    {
        InitializeComponent();

        // The designer strips this line because the property's DefaultValue
        // attribute claims it is false, but pages are draggable at runtime
        // without it. The designer will keep deleting it, so it lives here
        // instead.
        settingsWorkspaceCell.AllowPageDrag = false;

        _logForm = new LogForm();
        _ = _logForm.Handle;
        _logForm.UserClosed += (s, e) => logToggleButton.Checked = false;
        _logForm.LocationChanged += (s, e) =>
        {
            if (!_dockingLog)
            {
                _logUserMoved = true;
            }
        };
        LocationChanged += (s, e) => FollowLogWindow();
        ResizeEnd += (s, e) => FollowLogWindow();
        generatorInfoBody.Resize += (s, e) => TextBoxAutoScroll.Update(generatorInfoBody);

        var executingAssembly = Assembly.GetExecutingAssembly();
        Icon = Icon.ExtractAssociatedIcon(executingAssembly.Location);
        Text += $" - {executingAssembly.GetName().Version}";

        _barGenerators = new List<BarGeneratorViewModel>
            {
                new BarGeneratorViewModel(
                    new MagicScalerBarGenerator("Normal", average: false),
                    "The default mode to generate barcodes. It scales images using a resampling algorithm that takes care of gamma correction and produces correct color averages.",
                    initialCheckState: true),
                new BarGeneratorViewModel(
                    GdiBarGenerator.CreateLegacy(average: false),
                    "The mode used in previous versions. It's relatively fast, but the algorithm used to scale images is of poor quality. This mode is not recommended, and only here for backward-compatibility.",
                    initialCheckState: false),
                new BarGeneratorViewModel(
                    new ScanlineBarGenerator("Scanline"),
                    "Samples the middle row of each frame and stretches it into a bar. Sharper than the average-based modes and immune to letterbox bars, but noisier.",
                    initialCheckState: false),
                new BarGeneratorViewModel(
                    new VerticalSweepBarGenerator("Vertical sweep"),
                    "Samples a vertical column of each frame and stretches it into a bar. The column sweeps left to right across frames, wrapping around.",
                    initialCheckState: false),
                new BarGeneratorViewModel(
                    new DominantColorBarGenerator("Dominant color"),
                    "Paints each bar the most common color of its frame. Poster-like barcodes instead of the smeared average.",
                    initialCheckState: false),
                new BarGeneratorViewModel(
                    new SubjectColorBarGenerator("Subject color"),
                    "Paints each bar the dominant color of the largest object in its frame. Like dominant color, but isolated to the main subject.",
                    initialCheckState: false),
                new BarGeneratorViewModel(
                    new SpectralColorBarGenerator("Audio spectrum"),
                    "Paints each bar the visible color of its dominant audio frequency, normalized per movie from dullest red to brightest violet. Audio-only. End-credits exclusion is ignored for audio-only runs.",
                    initialCheckState: false),
                new BarGeneratorViewModel(
                    new BlackbodyColorBarGenerator("Audio blackbody"),
                    "Paints each bar the blackbody color of its dominant audio frequency, normalized per movie from ember orange to pale blue. Audio-only. End-credits exclusion is ignored for audio-only runs.",
                    initialCheckState: false),
                new BarGeneratorViewModel(
                    new ChromaColorBarGenerator("Audio harmony"),
                    "Paints each bar the hue of its dominant pitch class, vivid for clear harmony washing toward white for diffuse sound. Audio-only. End-credits exclusion is ignored for audio-only runs.",
                    initialCheckState: false),
                new BarGeneratorViewModel(
                    new ElevationColorBarGenerator("Audio elevation"),
                    "Paints each bar the elevation color of its dominant audio frequency, normalized per movie from deep purple basins to white peaks. Audio-only. End-credits exclusion is ignored for audio-only runs.",
                    initialCheckState: false),
            };

        barGeneratorList.DisplayMember = nameof(BarGeneratorViewModel.DisplayName);
        barGeneratorList.Items.Clear();
        foreach (var item in _barGenerators)
        {
            int index = barGeneratorList.Items.Add(item);
            barGeneratorList.SetItemChecked(index, item.Checked);
        }

        barGeneratorList.SelectedItem = _barGenerators.First(x => x.Checked); // So that the right panel displays something.
        barGeneratorList.SelectedItem = null; // Unselect so a click on the line will not uncheck the item.
        UpdateWaveformControlsAvailability();
        UpdateBrightnessControlsAvailability();
        UpdateSettingsTabsAvailability();

        AppendLog(Text);

        _openFileDialog = new OpenFileDialog()
        {
            CheckFileExists = true,
            CheckPathExists = true,
        };

        _saveFileDialog = new SaveFileDialog()
        {
            DefaultExt = ".png",
            Filter = "Bitmap|*.bmp|Jpeg|*.jpg|Png|*.png|Gif|*.gif|All files|*.*",
            FilterIndex = 3, // 1 based
            OverwritePrompt = true,
        };

        _ffmpegWrapper = new FfmpegWrapper(FfmpegWrapper.DefaultExecutableName);
        _imageProcessor = new ImageStreamProcessor();
        _barCodeParametersValidator = new BarCodeParametersValidator();

        generateButton.Text = GenerateButtonText;

        kryptonManager1.BaseFont = new Font("Segoe UI", 9f);
        _progressTrackDarkColor = progressBar.StateCommon.Back.Color2;
        _bulbLit = LoadEmbeddedImage("light-mode.png");
        _bulbUnlit = LoadEmbeddedImage("dark-mode.png");
        _aboutImage = LoadEmbeddedImage("about.png");
        aboutButton.StateCommon.Back.Color1 = Color.Transparent;
        aboutButton.StateCommon.Back.ColorStyle = Krypton.Toolkit.PaletteColorStyle.Solid;
        aboutButton.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
        aboutButton.Values.Image = _aboutImage;
        themeButton.StateCommon.Back.Color1 = Color.Transparent;
        themeButton.StateCommon.Back.ColorStyle = Krypton.Toolkit.PaletteColorStyle.Solid;
        themeButton.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.None;
        themeButton.Values.ImageStates.ImageNormal = _bulbLit;
        themeButton.Values.ImageStates.ImageTracking = _bulbLit;
        themeButton.Values.ImageStates.ImagePressed = _bulbLit;
        themeButton.Values.ImageStates.ImageDisabled = _bulbUnlit;
        themeButton.Values.ImageStates.ImageCheckedNormal = _bulbUnlit;
        themeButton.Values.ImageStates.ImageCheckedTracking = _bulbUnlit;
        themeButton.Values.ImageStates.ImageCheckedPressed = _bulbUnlit;
        themeButton.Checked = Settings.Default.Theme != "Light";
        ApplyTheme(themeButton.Checked);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        WindowCorners.ApplyRoundedCorners(this);
    }

    private void themeButton_CheckedChanged(object sender, EventArgs e)
    {
        ApplyTheme(themeButton.Checked);
    }

    private void logToggleButton_CheckedChanged(object sender, EventArgs e)
    {
        if (_logForm == null)
        {
            return;
        }

        if (logToggleButton.Checked)
        {
            DockLogWindow();
            if (!_logForm.Visible)
            {
                // The log opens without taking focus, keeping foreground on the main
                // form avoids it randomly sinking behind other windows on first show.
                _logForm.Show(this);
            }
        }
        else
        {
            _logForm.Hide();
        }
    }

    private void DockLogWindow()
    {
        PositionLogWindow();
        _logUserMoved = false;
    }

    private void FollowLogWindow()
    {
        if (_logForm.Visible && !_logUserMoved)
        {
            PositionLogWindow();
        }
    }

    private void PositionLogWindow()
    {
        _dockingLog = true;
        try
        {
            _logForm.Bounds = new Rectangle(Bounds.Right + 8, Bounds.Top, 800, Bounds.Height);
        }
        finally
        {
            _dockingLog = false;
        }
    }

    private void ApplyTheme(bool dark)
    {
        kryptonManager1.GlobalPaletteMode = dark
            ? Krypton.Toolkit.PaletteMode.Office2010Black
            : Krypton.Toolkit.PaletteMode.Office2010Silver;
        ApplyListHighlightColors(dark);
        ApplyListBaseColors(dark);
        ApplyInfoBoxColors(dark);
        ApplyWorkspaceBorderColors(dark);
        _logForm.ApplyThemeColors(dark);
        progressBar.StateCommon.Back.Color2 = dark ? _progressTrackDarkColor : Color.White;
        Settings.Default.Theme = dark ? "Dark" : "Light";
        Settings.Default.Save();
    }

    /// <summary>
    /// Matches the checklist background and item text to the surrounding
    /// panel colors sampled from the Office 2010 themes, so the list blends
    /// in like the description box does.
    /// </summary>
    private void ApplyListBaseColors(bool dark)
    {
        if (dark)
        {
            barGeneratorList.StateCommon.Back.Color1 = Color.FromArgb(113, 113, 113);
            barGeneratorList.StateCommon.Back.Color2 = Color.FromArgb(113, 113, 113);
            barGeneratorList.StateCommon.Back.ColorStyle = Krypton.Toolkit.PaletteColorStyle.Solid;
            barGeneratorList.StateCommon.Item.Content.ShortText.Color1 = Color.FromArgb(255, 255, 255);
        }
        else
        {
            barGeneratorList.StateCommon.Back.Color1 = Color.FromArgb(227, 230, 232);
            barGeneratorList.StateCommon.Back.Color2 = Color.FromArgb(227, 230, 232);
            barGeneratorList.StateCommon.Back.ColorStyle = Krypton.Toolkit.PaletteColorStyle.Solid;
            barGeneratorList.StateCommon.Item.Content.ShortText.Color1 = Color.FromArgb(59, 59, 59);
        }
    }

    /// <summary>
    /// Matches the generator description box to the surrounding panel colors
    /// sampled from the Office 2010 themes.
    /// </summary>
    private void ApplyInfoBoxColors(bool dark)
    {
        if (dark)
        {
            generatorInfoBody.StateCommon.Back.Color1 = Color.FromArgb(113, 113, 113);
            generatorInfoBody.StateCommon.Content.Color1 = Color.FromArgb(255, 255, 255);
        }
        else
        {
            generatorInfoBody.StateCommon.Back.Color1 = Color.FromArgb(227, 230, 232);
            generatorInfoBody.StateCommon.Content.Color1 = Color.FromArgb(59, 59, 59);
        }
    }

    /// <summary>
    /// Matches the settings workspace frame to the generator checklist
    /// frame, since the navigator palette does not resolve the same color on its own.
    /// </summary>
    private void ApplyWorkspaceBorderColors(bool dark)
    {
        Color border = dark
            ? Color.FromArgb(132, 132, 132)
            : Color.FromArgb(212, 214, 217);

        // The page frame is drawn from the HeaderGroup palette, not the
        // cell border slots.
        var frame = settingsWorkspaceCell.StateNormal.HeaderGroup.Border;
        frame.Draw = Krypton.Toolkit.InheritBool.True;
        frame.Color1 = border;
        frame.ColorStyle = Krypton.Toolkit.PaletteColorStyle.Solid;
        frame.Rounding = 3F;
    }

    /// <summary>
    /// Loads an image embedded from the images folder. The image is copied
    /// into a standalone bitmap so the resource stream lifetime can't break
    /// later paints (GDI+ keeps Images tied to their source stream).
    /// </summary>
    private static Image LoadEmbeddedImage(string fileName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"MovieBarCodeGenerator.images.{fileName}");
        using var source = Image.FromStream(stream);
        return new Bitmap(source);
    }

    private void ApplyListHighlightColors(bool dark)
    {
        // The Office2010Silver palette leaves tracked/pressed checklist rows
        // with an unreadable foreground, so pin those states to the standard
        // Windows selection colors in light mode. Color.Empty reverts to the
        // palette, which is already fine in dark mode.
        Color back = dark ? Color.Empty : Color.FromArgb(0, 120, 215);
        Color text = dark ? Color.Empty : Color.White;
        var states = new[]
        {
            barGeneratorList.StateTracking,
            barGeneratorList.StatePressed,
            barGeneratorList.StateCheckedTracking,
            barGeneratorList.StateCheckedPressed,
        };
        foreach (var state in states)
        {
            state.Item.Back.Color1 = back;
            state.Item.Content.ShortText.Color1 = text;
        }
    }

    private async void generateButton_Click(object sender, EventArgs e)
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            generateButton.Text = GenerateButtonText;
            progressBar.Value = progressBar.Minimum;
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
            return;
        }

        // Validate parameters

        bool overwriteGranted = false;
        bool PromptOverwriteExistingOutputFile(IReadOnlyCollection<string> paths)
        {
            var promptResult = MessageBox.Show(this,
                 $"The following files already exist: '{string.Join(", ", paths.Select(x => $"'{x}'"))}'. Do you want to overwrite them?",
                 "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            overwriteGranted = promptResult == DialogResult.Yes;
            return overwriteGranted;
        }

        var generators =
            _barGenerators
            .Where(x => x.Checked)
            .Select(x => x.Generator)
            .ToArray();

        if (generators.Any() == false)
        {
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
            MessageBox.Show(this, "At least one barcode version must be selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Batch mode: input is a folder -> process every video file recursively
        // into the output folder (named after each input file).
        if (Directory.Exists(inputPathTextBox.Text.Trim('"')))
        {
            await RunBatchAsync(generators, PromptOverwriteExistingOutputFile);
            return;
        }

        BarCodeParameters parameters;
        try
        {
            parameters = _barCodeParametersValidator.GetValidatedParameters(
                rawInputPath: inputPathTextBox.Text,
                rawBaseOutputPath: outputPathTextBox.Text,
                rawBarWidth: barWidthTextBox.Text,
                rawImageWidth: imageWidthTextBox.Text,
                rawImageHeight: imageHeightTextBox.Text,
                shouldOverwriteOutputPaths: PromptOverwriteExistingOutputFile,
                barGenerators: generators,
                fileNamePostfix: postfixTextBox.Text);
        }
        catch (OperationCanceledException)
        {
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
            return;
        }
        catch (Exception ex)
        {
            AppendLog("Error validating input parameters. " + ex.ToString());
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }

        AppendLog($@"Barcode generation starting...
Input: '{parameters.InputPath}'
Output: {string.Join(", ", parameters.GeneratorOutputPaths.Select(x => $"'{x.Value}'"))}
Output width: {parameters.Width}
Output height: {parameters.Height}
Bar width: {parameters.BarWidth}");

        // Register progression callback and ready cancellation source

        var progress = new PercentageProgressHandler(percentage =>
        {
            var progressBarValue = Math.Min(100, (int)Math.Round(percentage * 100, MidpointRounding.AwayFromZero));
            Invoke(new Action(() =>
            {
                if (_cancellationTokenSource != null)
                {
                    progressBar.Value = progressBarValue;
                    TaskbarProgress.SetValue(Handle, progressBarValue, 100);
                }
            }));
        });

        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationLocalRef = _cancellationTokenSource;

        // Actually create the barcode

        IReadOnlyDictionary<IBarGenerator, Bitmap> result = null;
        try
        {
            generateButton.Text = CancelButtonText;
            generateButton.Enabled = false;

            // Prevent the user from cancelling for 1sec (it might not be obvious the generation has started)
            var dontCare = Task.Delay(1000).ContinueWith(t =>
            {
                try
                {
                    Invoke(new Action(() => generateButton.Enabled = true));
                }
                catch { }
            });

            await Task.Run(() =>
            {
                result = _imageProcessor.CreateBarCodes(
                    parameters,
                    _ffmpegWrapper,
                    _cancellationTokenSource.Token,
                    progress,
                    AppendLog,
                    excludeCredits: excludeCreditsCheckBox.Checked,
                    overlayWaveform: overlayWaveformCheckBox.Checked,
                    overlayStrength: waveformStrengthTrackBar.Value / 100.0,
                    overlayWaveformColor: waveformColorButton.SelectedColor,
                    spectralBrightness: spectralBrightnessCheckBox.Checked,
                    brightnessIntensity: brightnessTrackBar.Value / 100.0,
                    smoothed: smoothedCheckBox.Checked,
                    cropLetterbox: croppedCheckBox.Checked);
            }, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            AppendLog("Operation cancelled.");
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
            return;
        }
        catch (Exception ex)
        {
            AppendLog("Error: " + ex.ToString());
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
            MessageBox.Show(this, "Sorry, something went wrong. See the log for more information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        finally
        {
            generateButton.Text = GenerateButtonText;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        if (cancellationLocalRef.IsCancellationRequested)
        {
            AppendLog("Operation cancelled.");
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
            return;
        }

        // Save the barcode

        AppendLog("Saving the images...");

        try
        {
            foreach (var barcode in result)
            {
                var outputPath = parameters.GeneratorOutputPaths[barcode.Key];
                var outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                if (File.Exists(outputPath) && !overwriteGranted)
                {
                    // The file appeared after validation (or validation was skipped).
                    AppendLog($"WARNING: skipped saving output file '{outputPath}' because it already exists.");
                    continue;
                }
                barcode.Value.Save(outputPath);
            }
        }
        catch (Exception ex)
        {
            var message = $"Unable to save the images: {ex}";
            AppendLog(message);
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        AppendLog("Barcode generated successfully!");
        progressBar.Value = progressBar.Minimum;
        TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
    }

    private async Task RunBatchAsync(IBarGenerator[] generators, Func<IReadOnlyCollection<string>, bool> shouldOverwriteOutputPaths)
    {
        var rawInputDir = inputPathTextBox.Text.Trim('"');
        var rawOutput = outputPathTextBox.Text.Trim('"');

        if (!string.IsNullOrWhiteSpace(rawOutput) && File.Exists(rawOutput))
        {
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
            MessageBox.Show(this, "When the input is a folder, the output must be a folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }

        var parsedExtensions = SupportedVideoExtensions.ParseOrNull(extensionsTextBox.Text);
        var allowedExtensions = (parsedExtensions == null || parsedExtensions.Count == 0)
            ? new HashSet<string>(SupportedVideoExtensions.Default, StringComparer.OrdinalIgnoreCase)
            : parsedExtensions;
        List<string> files;
        try
        {
            files = Directory.EnumerateFiles(rawInputDir, "*", SearchOption.AllDirectories)
                .Where(f => allowedExtensions.Contains(Path.GetExtension(f)))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception ex)
        {
            AppendLog("Error browsing input directory. " + ex.ToString());
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }

        if (files.Any() == false)
        {
            AppendLog($"No video files found in '{rawInputDir}'.");
            MessageBox.Show(this, "No video files found in the input folder.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var rawBarWidth = barWidthTextBox.Text;
        var rawImageWidth = imageWidthTextBox.Text;
        var rawImageHeight = imageHeightTextBox.Text;

        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationLocalRef = _cancellationTokenSource;

        generateButton.Text = CancelButtonText;
        generateButton.Enabled = false;
        var dontCare = Task.Delay(1000).ContinueWith(t =>
        {
            try
            {
                Invoke(new Action(() => generateButton.Enabled = true));
            }
            catch { }
        });

        AppendLog($"Batch starting... {files.Count} video file(s) in '{rawInputDir}'.");

        try
        {
            int index = 0;
            foreach (var file in files)
            {
                if (cancellationLocalRef.IsCancellationRequested)
                {
                    AppendLog("Operation cancelled.");
                    break;
                }

                index++;
                AppendLog($"Processing file {index}/{files.Count}: '{file}'...");

                BarCodeParameters parameters;
                bool fileOverwriteGranted = false;
                try
                {
                    parameters = _barCodeParametersValidator.GetValidatedParameters(
                        rawInputPath: file,
                        rawBaseOutputPath: rawOutput,
                        rawBarWidth: rawBarWidth,
                        rawImageWidth: rawImageWidth,
                        rawImageHeight: rawImageHeight,
                        shouldOverwriteOutputPaths: x => { fileOverwriteGranted = shouldOverwriteOutputPaths(x); return fileOverwriteGranted; },
                        barGenerators: generators,
                        fileNamePostfix: postfixTextBox.Text);
                }
                catch (OperationCanceledException)
                {
                    AppendLog("Skipped (output already exists).");
                    continue;
                }
                catch (Exception ex)
                {
                    AppendLog("Error validating input parameters. " + ex.ToString());
                    continue;
                }

                var progress = new PercentageProgressHandler(percentage =>
                {
                    var progressBarValue = Math.Min(100, (int)Math.Round(percentage * 100, MidpointRounding.AwayFromZero));
                    Invoke(new Action(() =>
                    {
                        if (_cancellationTokenSource != null)
                        {
                            progressBar.Value = progressBarValue;
                            TaskbarProgress.SetValue(Handle, progressBarValue, 100);
                        }
                    }));
                });

                IReadOnlyDictionary<IBarGenerator, Bitmap> result = null;
                try
                {
                    progressBar.Value = progressBar.Minimum;
                    await Task.Run(() =>
                    {
                        result = _imageProcessor.CreateBarCodes(
                            parameters,
                            _ffmpegWrapper,
                            _cancellationTokenSource.Token,
                            progress,
                            AppendLog,
                            excludeCredits: excludeCreditsCheckBox.Checked,
                            overlayWaveform: overlayWaveformCheckBox.Checked,
                            overlayStrength: waveformStrengthTrackBar.Value / 100.0,
                            overlayWaveformColor: waveformColorButton.SelectedColor,
                            spectralBrightness: spectralBrightnessCheckBox.Checked,
                            brightnessIntensity: brightnessTrackBar.Value / 100.0,
                            smoothed: smoothedCheckBox.Checked,
                            cropLetterbox: croppedCheckBox.Checked);
                    }, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    AppendLog("Operation cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    AppendLog("Error: " + ex.ToString());
                    continue;
                }

                if (cancellationLocalRef.IsCancellationRequested)
                {
                    AppendLog("Operation cancelled.");
                    break;
                }

                try
                {
                    foreach (var barcode in result)
                    {
                        var outputPath = parameters.GeneratorOutputPaths[barcode.Key];
                        var outputDir = Path.GetDirectoryName(outputPath);
                        if (!string.IsNullOrEmpty(outputDir))
                        {
                            Directory.CreateDirectory(outputDir);
                        }
                        if (File.Exists(outputPath) && !fileOverwriteGranted)
                        {
                            // The file appeared after validation.
                            AppendLog($"WARNING: skipped saving output file '{outputPath}' because it already exists.");
                            continue;
                        }
                        barcode.Value.Save(outputPath);
                        AppendLog($"File '{outputPath}' saved successfully!");
                    }
                }
                catch (Exception ex)
                {
                    var message = $"Unable to save the images: {ex}";
                    AppendLog(message);
                    MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            AppendLog("Batch finished!");
        }
        finally
        {
            generateButton.Text = GenerateButtonText;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            progressBar.Value = progressBar.Minimum;
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
        }
    }

    private void browseInputPathButton_Click(object sender, EventArgs e)
    {
        if (_openFileDialog.ShowDialog(owner: this) == DialogResult.OK)
        {
            inputPathTextBox.Text = _openFileDialog.FileName;
        }
    }

    private void browseOutputPathButton_Click(object sender, EventArgs e)
    {
        if (_saveFileDialog.ShowDialog(owner: this) == DialogResult.OK)
        {
            outputPathTextBox.Text = _saveFileDialog.FileName;
        }
    }

    private void imageWidthTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (int.TryParse(imageWidthTextBox.Text, out var imageWidth)
         && int.TryParse(barWidthTextBox.Text, out var barWidth))
        {
            try
            {
                var newBarCount = (int)Math.Round((double)imageWidth / barWidth);
                barCountTextBox.Text = newBarCount.ToString();
            }
            catch { }
        }
    }

    private void barCountTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (int.TryParse(barCountTextBox.Text, out var barCount)
            && int.TryParse(imageWidthTextBox.Text, out var imageWidth))
        {
            try
            {
                var newBarWidth = (int)Math.Round((double)imageWidth / barCount);
                barWidthTextBox.Text = newBarWidth.ToString();
            }
            catch { }
        }
    }

    private void barWidthTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (int.TryParse(barWidthTextBox.Text, out var barWidth)
            && int.TryParse(imageWidthTextBox.Text, out var imageWidth))
        {
            try
            {
                var newBarCount = (int)Math.Round((double)imageWidth / barWidth);
                barCountTextBox.Text = newBarCount.ToString();
            }
            catch { }
        }
    }

    private void aboutButton_Click(object sender, EventArgs e) => new AboutBox().ShowDialog();

    private void AppendLog(string value)
    {
        _logForm.AppendLog(value);
    }

    private void TextBox_DragDrop(object sender, DragEventArgs e)
    {
        if (sender is TextBox target)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                var path = paths.FirstOrDefault();
                if (path != null)
                {
                    target.Text = path;
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                target.Text = (string)e.Data.GetData(DataFormats.Text);
            }
        }
    }

    private void TextBox_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Text))
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void barGeneratorList_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        if (barGeneratorList.Items[e.Index] is BarGeneratorViewModel generator)
        {
            generator.Checked = e.NewValue == CheckState.Checked;
        }

        UpdateExcludeCreditsAvailability();
        UpdateWaveformControlsAvailability();
        UpdateBrightnessControlsAvailability();
        UpdateSettingsTabsAvailability();
    }

    private void UpdateExcludeCreditsAvailability()
    {
        // Credits exclusion trims both audio and video to the same prefix,
        // so it stays available whenever any video-based (incl. hybrid)
        // generator is selected. Only pure audio-only runs disable it.
        var checkedGenerators = _barGenerators.Where(x => x.Checked).Select(x => x.Generator).ToArray();
        bool anyChecked = checkedGenerators.Length > 0;
        bool pureAudioOnly = anyChecked && checkedGenerators.All(g => g is IAudioBarGenerator);
        excludeCreditsCheckBox.Enabled = !pureAudioOnly;
        if (pureAudioOnly)
        {
            toolTip1.SetToolTip(excludeCreditsCheckBox, "End-credits exclusion is ignored for audio-only runs (no video to trim).");
        }
        else
        {
            toolTip1.SetToolTip(excludeCreditsCheckBox, "Detects dark end credits with a quick brightness scan and ends the barcode where they begin.");
        }
    }

    private void barGeneratorList_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (barGeneratorList.SelectedItem is BarGeneratorViewModel generator)
        {
            generatorInfoBody.Text = $"{generator.Details}";
            TextBoxAutoScroll.Update(generatorInfoBody);
            settingsWorkspaceCell.SelectedPage = pageGeneral;
        }
    }

    /// <summary>
    /// The track bar works in whole hundredths, 10 to 100, to match the overlay strength range of 0.1 to 1.
    /// </summary>
    private void waveformStrengthTrackBar_ValueChanged(object sender, EventArgs e)
    {
        double strength = waveformStrengthTrackBar.Value / 100.0;
        waveformStrengthValueLabel.Values.Text = strength.ToString("0.00");
    }

    private void overlayWaveformCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        UpdateWaveformControlsAvailability();
        SelectSettingsPage();
    }

    /// <summary>
    /// The track bar works in whole hundredths, 10 to 100, to match the brightness intensity range of 0.1 to 1.
    /// </summary>
    private void brightnessTrackBar_ValueChanged(object sender, EventArgs e)
    {
        double intensity = brightnessTrackBar.Value / 100.0;
        brightnessValueLabel.Values.Text = intensity.ToString("0.00");
    }

    private void spectralBrightnessCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        UpdateBrightnessControlsAvailability();
        SelectSettingsPage();
    }

    /// <summary>
    /// Shows the settings page for the most recently enabled option, falling
    /// back to General. Tabs stay clickable for free navigation.
    /// </summary>
    private void SelectSettingsPage()
    {
        if (overlayWaveformCheckBox.Checked && pageWaveform.Enabled)
        {
            settingsWorkspaceCell.SelectedPage = pageWaveform;
        }
        else if (spectralBrightnessCheckBox.Checked && pageBrightness.Enabled)
        {
            settingsWorkspaceCell.SelectedPage = pageBrightness;
        }
        else
        {
            settingsWorkspaceCell.SelectedPage = pageGeneral;
        }
    }

    /// <summary>
    /// The Waveform and Brightness pages only apply to video-based modes, so
    /// their tabs stay disabled for audio-only runs. Falls back to General
    /// before disabling.
    /// </summary>
    private void UpdateSettingsTabsAvailability()
    {
        bool videoSelected = _barGenerators.Any(x => x.Checked && x.Generator is not IAudioBarGenerator);
        if (!videoSelected)
        {
            settingsWorkspaceCell.SelectedPage = pageGeneral;
        }

        pageWaveform.Enabled = videoSelected;
        pageBrightness.Enabled = videoSelected;
    }

    /// <summary>
    /// The waveform controls only apply when the overlay option is checked.
    /// </summary>
    private void UpdateWaveformControlsAvailability()
    {
        bool overlaySelected = overlayWaveformCheckBox.Checked;
        waveformStrengthLabel.Enabled = overlaySelected;
        waveformStrengthValueLabel.Enabled = overlaySelected;
        waveformStrengthTrackBar.Enabled = overlaySelected;
        waveformColorLabel.Enabled = overlaySelected;
        waveformColorButton.Enabled = overlaySelected;
    }

    /// <summary>
    /// The brightness controls only apply when the spectral brightness option is checked.
    /// </summary>
    private void UpdateBrightnessControlsAvailability()
    {
        bool brightnessSelected = spectralBrightnessCheckBox.Checked;
        brightnessLabel.Enabled = brightnessSelected;
        brightnessValueLabel.Enabled = brightnessSelected;
        brightnessTrackBar.Enabled = brightnessSelected;
    }

    private void toolTip1_Popup(object sender, PopupEventArgs e)
    {

    }
}

public class PercentageProgressHandler : IProgress<double>
{
    private readonly Action<double> _handler;

    public PercentageProgressHandler(Action<double> handler)
    {
        _handler = handler;
    }
    public void Report(double value) => _handler(value);
}

public class BarGeneratorViewModel
{
    public IBarGenerator Generator { get; }
    public string Details { get; }

    public bool Checked { get; set; }
    public string DisplayName => Generator.DisplayName;

    public override string ToString() => DisplayName;

    public BarGeneratorViewModel(IBarGenerator generator, string details, bool initialCheckState)
    {
        Generator = generator;
        Details = details;
        Checked = initialCheckState;
    }
}
