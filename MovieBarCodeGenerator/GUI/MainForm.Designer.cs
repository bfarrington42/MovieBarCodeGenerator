namespace MovieBarCodeGenerator.GUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bulbLit?.Dispose();
                _bulbUnlit?.Dispose();
                _aboutImage?.Dispose();
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.kryptonManager1 = new Krypton.Toolkit.KryptonManager(this.components);
            this.inputPathTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.browseInputPathButton = new Krypton.Toolkit.KryptonButton();
            this.generateButton = new Krypton.Toolkit.KryptonButton();
            this.label1 = new Krypton.Toolkit.KryptonLabel();
            this.filesSection = new Krypton.Toolkit.KryptonGroupBox();
            this.label11 = new Krypton.Toolkit.KryptonLabel();
            this.extensionsTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.label10 = new Krypton.Toolkit.KryptonLabel();
            this.postfixTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.label2 = new Krypton.Toolkit.KryptonLabel();
            this.outputPathTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.browseOutputPathButton = new Krypton.Toolkit.KryptonButton();
            this.aboutButton = new Krypton.Toolkit.KryptonButton();
            this.themeButton = new Krypton.Toolkit.KryptonCheckButton();
            this.optionsSection = new Krypton.Toolkit.KryptonGroupBox();
            this.generatorInfoBody = new Krypton.Toolkit.KryptonTextBox();
            this.overlayWaveformCheckBox = new Krypton.Toolkit.KryptonCheckBox();
            this.waveformStrengthLabel = new Krypton.Toolkit.KryptonLabel();
            this.waveformStrengthValueLabel = new Krypton.Toolkit.KryptonLabel();
            this.waveformStrengthTrackBar = new Krypton.Toolkit.KryptonTrackBar();
            this.waveformColorLabel = new Krypton.Toolkit.KryptonLabel();
            this.waveformColorButton = new Krypton.Toolkit.KryptonColorButton();
            this.modeSelectionLabel = new Krypton.Toolkit.KryptonLabel();
            this.excludeCreditsCheckBox = new Krypton.Toolkit.KryptonCheckBox();
            this.barGeneratorList = new Krypton.Toolkit.KryptonCheckedListBox();
            this.precendenceNote = new Krypton.Toolkit.KryptonLabel();
            this.barWidthLabel = new Krypton.Toolkit.KryptonLabel();
            this.barWidthTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.barCountLabel = new Krypton.Toolkit.KryptonLabel();
            this.barCountTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.label4 = new Krypton.Toolkit.KryptonLabel();
            this.imageHeightTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.outputImageSizeLabel = new Krypton.Toolkit.KryptonLabel();
            this.imageWidthTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.progressBar = new Krypton.Toolkit.KryptonProgressBar();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.logToggleButton = new Krypton.Toolkit.KryptonCheckButton();
            ((System.ComponentModel.ISupportInitialize)(this.filesSection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.filesSection.Panel)).BeginInit();
            this.filesSection.Panel.SuspendLayout();
            this.filesSection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.optionsSection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.optionsSection.Panel)).BeginInit();
            this.optionsSection.Panel.SuspendLayout();
            this.optionsSection.SuspendLayout();
            this.SuspendLayout();
            // 
            // kryptonManager1
            // 
            this.kryptonManager1.GlobalPaletteMode = Krypton.Toolkit.PaletteMode.Office2010Black;
            this.kryptonManager1.ToolkitStrings.MessageBoxStrings.LessDetails = "L&ess Details...";
            this.kryptonManager1.ToolkitStrings.MessageBoxStrings.MoreDetails = "&More Details...";
            // 
            // inputPathTextBox
            // 
            this.inputPathTextBox.AllowDrop = true;
            this.inputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputPathTextBox.Location = new System.Drawing.Point(6, 28);
            this.inputPathTextBox.Name = "inputPathTextBox";
            this.inputPathTextBox.Size = new System.Drawing.Size(504, 23);
            this.inputPathTextBox.TabIndex = 1;
            this.toolTip1.SetToolTip(this.inputPathTextBox, "A video file, or a folder to batch-process every video in it and its subfolders. " +
        "You can also drag and drop a file or folder here.");
            this.inputPathTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox_DragDrop);
            this.inputPathTextBox.DragOver += new System.Windows.Forms.DragEventHandler(this.TextBox_DragOver);
            // 
            // browseInputPathButton
            // 
            this.browseInputPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseInputPathButton.Location = new System.Drawing.Point(525, 27);
            this.browseInputPathButton.Name = "browseInputPathButton";
            this.browseInputPathButton.Size = new System.Drawing.Size(75, 25);
            this.browseInputPathButton.TabIndex = 2;
            this.browseInputPathButton.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.browseInputPathButton.Values.Text = "Browse...";
            this.browseInputPathButton.Click += new System.EventHandler(this.browseInputPathButton_Click);
            // 
            // generateButton
            // 
            this.generateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.generateButton.Location = new System.Drawing.Point(556, 618);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(75, 25);
            this.generateButton.StateCommon.Content.Padding = new System.Windows.Forms.Padding(3);
            this.generateButton.TabIndex = 13;
            this.generateButton.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.generateButton.Values.Text = "Generate!";
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(1, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 20);
            this.label1.TabIndex = 3;
            this.label1.Values.Text = "Input video path:";
            // 
            // filesSection
            // 
            this.filesSection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filesSection.Location = new System.Drawing.Point(12, 12);
            // 
            // filesSection.Panel
            // 
            this.filesSection.Panel.Controls.Add(this.label11);
            this.filesSection.Panel.Controls.Add(this.extensionsTextBox);
            this.filesSection.Panel.Controls.Add(this.label10);
            this.filesSection.Panel.Controls.Add(this.postfixTextBox);
            this.filesSection.Panel.Controls.Add(this.label2);
            this.filesSection.Panel.Controls.Add(this.outputPathTextBox);
            this.filesSection.Panel.Controls.Add(this.browseOutputPathButton);
            this.filesSection.Panel.Controls.Add(this.label1);
            this.filesSection.Panel.Controls.Add(this.inputPathTextBox);
            this.filesSection.Panel.Controls.Add(this.browseInputPathButton);
            this.filesSection.Size = new System.Drawing.Size(619, 231);
            this.filesSection.TabIndex = 0;
            this.filesSection.Values.Heading = "Files";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(2, 148);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(288, 20);
            this.label11.TabIndex = 8;
            this.label11.Values.Text = "Video extensions, e.g. mp4,mkv (empty = defaults):";
            // 
            // extensionsTextBox
            // 
            this.extensionsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionsTextBox.Location = new System.Drawing.Point(6, 169);
            this.extensionsTextBox.Name = "extensionsTextBox";
            this.extensionsTextBox.Size = new System.Drawing.Size(504, 23);
            this.extensionsTextBox.TabIndex = 6;
            this.toolTip1.SetToolTip(this.extensionsTextBox, "Comma or semicolon separated extensions. Accepts \"mp4\", \".mp4\" or \"*.mp4\". Leave " +
        "empty to use all supported video types.");
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(1, 100);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(145, 20);
            this.label10.TabIndex = 7;
            this.label10.Values.Text = "Custom filename postfix:";
            // 
            // postfixTextBox
            // 
            this.postfixTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.postfixTextBox.Location = new System.Drawing.Point(6, 122);
            this.postfixTextBox.Name = "postfixTextBox";
            this.postfixTextBox.Size = new System.Drawing.Size(504, 23);
            this.postfixTextBox.TabIndex = 5;
            this.toolTip1.SetToolTip(this.postfixTextBox, "Appended to every output file name, after the mode suffix and before the extensio" +
        "n.\r\ne.g. \"-banner\" gives movie-banner.png.\r\nThe mode suffix is omitted when only" +
        " one barcode mode is selected.");
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(1, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 20);
            this.label2.TabIndex = 6;
            this.label2.Values.Text = "Output image path:";
            // 
            // outputPathTextBox
            // 
            this.outputPathTextBox.AllowDrop = true;
            this.outputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputPathTextBox.Location = new System.Drawing.Point(6, 75);
            this.outputPathTextBox.Name = "outputPathTextBox";
            this.outputPathTextBox.Size = new System.Drawing.Size(504, 23);
            this.outputPathTextBox.TabIndex = 3;
            this.toolTip1.SetToolTip(this.outputPathTextBox, "An image file, or a folder to save one image per input video, named after each vi" +
        "deo. Leave empty to save next to the input.");
            this.outputPathTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox_DragDrop);
            this.outputPathTextBox.DragOver += new System.Windows.Forms.DragEventHandler(this.TextBox_DragOver);
            // 
            // browseOutputPathButton
            // 
            this.browseOutputPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseOutputPathButton.Location = new System.Drawing.Point(525, 75);
            this.browseOutputPathButton.Name = "browseOutputPathButton";
            this.browseOutputPathButton.Size = new System.Drawing.Size(75, 25);
            this.browseOutputPathButton.TabIndex = 4;
            this.browseOutputPathButton.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.browseOutputPathButton.Values.Text = "Browse...";
            this.browseOutputPathButton.Click += new System.EventHandler(this.browseOutputPathButton_Click);
            // 
            // aboutButton
            // 
            this.aboutButton.ButtonStyle = Krypton.Toolkit.ButtonStyle.LowProfile;
            this.aboutButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.aboutButton.Location = new System.Drawing.Point(612, 1);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.Size = new System.Drawing.Size(16, 18);
            this.aboutButton.TabIndex = 12;
            this.aboutButton.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.aboutButton.Values.Text = "";
            this.aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
            // 
            // themeButton
            // 
            this.themeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.themeButton.Checked = true;
            this.themeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.themeButton.Location = new System.Drawing.Point(590, 1);
            this.themeButton.Name = "themeButton";
            this.themeButton.Size = new System.Drawing.Size(18, 16);
            this.themeButton.TabIndex = 16;
            this.toolTip1.SetToolTip(this.themeButton, "Toggle between the dark and light themes.");
            this.themeButton.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.themeButton.Values.Text = "";
            this.themeButton.CheckedChanged += new System.EventHandler(this.themeButton_CheckedChanged);
            // 
            // optionsSection
            // 
            this.optionsSection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optionsSection.Location = new System.Drawing.Point(12, 249);
            // 
            // optionsSection.Panel
            // 
            this.optionsSection.Panel.Controls.Add(this.generatorInfoBody);
            this.optionsSection.Panel.Controls.Add(this.overlayWaveformCheckBox);
            this.optionsSection.Panel.Controls.Add(this.waveformStrengthLabel);
            this.optionsSection.Panel.Controls.Add(this.waveformStrengthValueLabel);
            this.optionsSection.Panel.Controls.Add(this.waveformStrengthTrackBar);
            this.optionsSection.Panel.Controls.Add(this.waveformColorLabel);
            this.optionsSection.Panel.Controls.Add(this.waveformColorButton);
            this.optionsSection.Panel.Controls.Add(this.modeSelectionLabel);
            this.optionsSection.Panel.Controls.Add(this.excludeCreditsCheckBox);
            this.optionsSection.Panel.Controls.Add(this.barGeneratorList);
            this.optionsSection.Panel.Controls.Add(this.precendenceNote);
            this.optionsSection.Panel.Controls.Add(this.barWidthLabel);
            this.optionsSection.Panel.Controls.Add(this.barWidthTextBox);
            this.optionsSection.Panel.Controls.Add(this.barCountLabel);
            this.optionsSection.Panel.Controls.Add(this.barCountTextBox);
            this.optionsSection.Panel.Controls.Add(this.label4);
            this.optionsSection.Panel.Controls.Add(this.imageHeightTextBox);
            this.optionsSection.Panel.Controls.Add(this.outputImageSizeLabel);
            this.optionsSection.Panel.Controls.Add(this.imageWidthTextBox);
            this.optionsSection.Size = new System.Drawing.Size(619, 365);
            this.optionsSection.TabIndex = 5;
            this.optionsSection.Values.Heading = "Barcode parameters";
            // 
            // generatorInfoBody
            // 
            this.generatorInfoBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.generatorInfoBody.Location = new System.Drawing.Point(286, 130);
            this.generatorInfoBody.Multiline = true;
            this.generatorInfoBody.Name = "generatorInfoBody";
            this.generatorInfoBody.ReadOnly = true;
            this.generatorInfoBody.Size = new System.Drawing.Size(324, 113);
            this.generatorInfoBody.StateCommon.Border.Draw = Krypton.Toolkit.InheritBool.False;
            this.generatorInfoBody.TabIndex = 15;
            // 
            // overlayWaveformCheckBox
            // 
            this.overlayWaveformCheckBox.Location = new System.Drawing.Point(444, 41);
            this.overlayWaveformCheckBox.Name = "overlayWaveformCheckBox";
            this.overlayWaveformCheckBox.Size = new System.Drawing.Size(156, 20);
            this.overlayWaveformCheckBox.TabIndex = 26;
            this.toolTip1.SetToolTip(this.overlayWaveformCheckBox, "Blends the audio waveform into every selected barcode in HSV (needs both video an" +
        "d audio). End-credits exclusion trims both streams.");
            this.overlayWaveformCheckBox.Values.Text = "Overlay waveform (HSV)";
            this.overlayWaveformCheckBox.CheckedChanged += new System.EventHandler(this.overlayWaveformCheckBox_CheckedChanged);
            // 
            // waveformStrengthLabel
            // 
            this.waveformStrengthLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.waveformStrengthLabel.Enabled = false;
            this.waveformStrengthLabel.Location = new System.Drawing.Point(286, 246);
            this.waveformStrengthLabel.Name = "waveformStrengthLabel";
            this.waveformStrengthLabel.Size = new System.Drawing.Size(145, 20);
            this.waveformStrengthLabel.TabIndex = 21;
            this.waveformStrengthLabel.Values.Text = "Waveform HSV strength:";
            // 
            // waveformStrengthValueLabel
            // 
            this.waveformStrengthValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.waveformStrengthValueLabel.Enabled = false;
            this.waveformStrengthValueLabel.Location = new System.Drawing.Point(437, 246);
            this.waveformStrengthValueLabel.Name = "waveformStrengthValueLabel";
            this.waveformStrengthValueLabel.Size = new System.Drawing.Size(33, 20);
            this.waveformStrengthValueLabel.TabIndex = 22;
            this.waveformStrengthValueLabel.Values.Text = "0.60";
            // 
            // waveformStrengthTrackBar
            // 
            this.waveformStrengthTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveformStrengthTrackBar.AutoSize = false;
            this.waveformStrengthTrackBar.Enabled = false;
            this.waveformStrengthTrackBar.Location = new System.Drawing.Point(286, 266);
            this.waveformStrengthTrackBar.Maximum = 100;
            this.waveformStrengthTrackBar.Minimum = 10;
            this.waveformStrengthTrackBar.Name = "waveformStrengthTrackBar";
            this.waveformStrengthTrackBar.Size = new System.Drawing.Size(324, 24);
            this.waveformStrengthTrackBar.TabIndex = 23;
            this.waveformStrengthTrackBar.TickFrequency = 10;
            this.waveformStrengthTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.toolTip1.SetToolTip(this.waveformStrengthTrackBar, "Waveform overlay strength, 0.1 = subtle, 1 = full waveform value.");
            this.waveformStrengthTrackBar.Value = 60;
            this.waveformStrengthTrackBar.ValueChanged += new System.EventHandler(this.waveformStrengthTrackBar_ValueChanged);
            // 
            // waveformColorLabel
            // 
            this.waveformColorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.waveformColorLabel.Enabled = false;
            this.waveformColorLabel.Location = new System.Drawing.Point(286, 301);
            this.waveformColorLabel.Name = "waveformColorLabel";
            this.waveformColorLabel.Size = new System.Drawing.Size(100, 20);
            this.waveformColorLabel.TabIndex = 24;
            this.waveformColorLabel.Values.Text = "Waveform color:";
            // 
            // waveformColorButton
            // 
            this.waveformColorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.waveformColorButton.DropDownOrientation = Krypton.Toolkit.VisualOrientation.Top;
            this.waveformColorButton.Enabled = false;
            this.waveformColorButton.Location = new System.Drawing.Point(392, 296);
            this.waveformColorButton.Name = "waveformColorButton";
            this.waveformColorButton.SelectedColor = System.Drawing.Color.White;
            this.waveformColorButton.Size = new System.Drawing.Size(100, 25);
            this.waveformColorButton.TabIndex = 25;
            this.toolTip1.SetToolTip(this.waveformColorButton, "Waveform layer color. Its brightness sets the blended value: white or saturated c" +
        "olors brighten fully, dark colors darken.");
            this.waveformColorButton.Values.Text = "";
            this.waveformColorButton.VisibleNoColor = false;
            // 
            // modeSelectionLabel
            // 
            this.modeSelectionLabel.Location = new System.Drawing.Point(1, 109);
            this.modeSelectionLabel.Name = "modeSelectionLabel";
            this.modeSelectionLabel.Size = new System.Drawing.Size(233, 20);
            this.modeSelectionLabel.TabIndex = 20;
            this.modeSelectionLabel.Values.Text = "Generate the following barcode versions:";
            // 
            // excludeCreditsCheckBox
            // 
            this.excludeCreditsCheckBox.Location = new System.Drawing.Point(444, 15);
            this.excludeCreditsCheckBox.Name = "excludeCreditsCheckBox";
            this.excludeCreditsCheckBox.Size = new System.Drawing.Size(129, 20);
            this.excludeCreditsCheckBox.TabIndex = 12;
            this.toolTip1.SetToolTip(this.excludeCreditsCheckBox, "Detects dark end credits with a quick brightness scan and ends the barcode where " +
        "they begin.");
            this.excludeCreditsCheckBox.Values.Text = "Exclude end credits";
            // 
            // barGeneratorList
            // 
            this.barGeneratorList.Location = new System.Drawing.Point(6, 130);
            this.barGeneratorList.Name = "barGeneratorList";
            this.barGeneratorList.Size = new System.Drawing.Size(274, 204);
            this.barGeneratorList.StateCommon.Border.Rounding = 3F;
            this.barGeneratorList.TabIndex = 18;
            this.barGeneratorList.SelectedIndexChanged += new System.EventHandler(this.barGeneratorList_SelectedIndexChanged);
            this.barGeneratorList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.barGeneratorList_ItemCheck);
            // 
            // precendenceNote
            // 
            this.precendenceNote.Location = new System.Drawing.Point(190, 64);
            this.precendenceNote.Name = "precendenceNote";
            this.precendenceNote.Size = new System.Drawing.Size(210, 20);
            this.precendenceNote.TabIndex = 17;
            this.precendenceNote.Values.Text = "Note: bar width will take precedence";
            // 
            // barWidthLabel
            // 
            this.barWidthLabel.Location = new System.Drawing.Point(258, 15);
            this.barWidthLabel.Name = "barWidthLabel";
            this.barWidthLabel.Size = new System.Drawing.Size(119, 20);
            this.barWidthLabel.TabIndex = 16;
            this.barWidthLabel.Values.Text = "Bar width (in pixels):";
            // 
            // barWidthTextBox
            // 
            this.barWidthTextBox.Location = new System.Drawing.Point(261, 37);
            this.barWidthTextBox.Name = "barWidthTextBox";
            this.barWidthTextBox.Size = new System.Drawing.Size(55, 23);
            this.barWidthTextBox.TabIndex = 10;
            this.barWidthTextBox.Text = "1";
            this.barWidthTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.barWidthTextBox_KeyUp);
            // 
            // barCountLabel
            // 
            this.barCountLabel.Location = new System.Drawing.Point(187, 15);
            this.barCountLabel.Name = "barCountLabel";
            this.barCountLabel.Size = new System.Drawing.Size(65, 20);
            this.barCountLabel.TabIndex = 14;
            this.barCountLabel.Values.Text = "Bar count:";
            // 
            // barCountTextBox
            // 
            this.barCountTextBox.Location = new System.Drawing.Point(190, 37);
            this.barCountTextBox.Name = "barCountTextBox";
            this.barCountTextBox.Size = new System.Drawing.Size(55, 23);
            this.barCountTextBox.TabIndex = 9;
            this.barCountTextBox.Text = "1000";
            this.barCountTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.barCountTextBox_KeyUp);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(64, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 20);
            this.label4.TabIndex = 11;
            this.label4.Values.Text = "x";
            // 
            // imageHeightTextBox
            // 
            this.imageHeightTextBox.Location = new System.Drawing.Point(84, 37);
            this.imageHeightTextBox.Name = "imageHeightTextBox";
            this.imageHeightTextBox.Size = new System.Drawing.Size(55, 23);
            this.imageHeightTextBox.TabIndex = 7;
            this.imageHeightTextBox.Text = "256";
            // 
            // outputImageSizeLabel
            // 
            this.outputImageSizeLabel.Location = new System.Drawing.Point(3, 15);
            this.outputImageSizeLabel.Name = "outputImageSizeLabel";
            this.outputImageSizeLabel.Size = new System.Drawing.Size(126, 20);
            this.outputImageSizeLabel.TabIndex = 1;
            this.outputImageSizeLabel.Values.Text = "Image size (in pixels):";
            // 
            // imageWidthTextBox
            // 
            this.imageWidthTextBox.Location = new System.Drawing.Point(6, 37);
            this.imageWidthTextBox.Name = "imageWidthTextBox";
            this.imageWidthTextBox.Size = new System.Drawing.Size(55, 23);
            this.imageWidthTextBox.TabIndex = 6;
            this.imageWidthTextBox.Text = "1000";
            this.imageWidthTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.imageWidthTextBox_KeyUp);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(72, 618);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(478, 25);
            this.progressBar.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(234)))), ((int)(((byte)(255)))));
            this.progressBar.StateCommon.Back.Color2 = System.Drawing.Color.Black;
            this.progressBar.Step = 1;
            this.progressBar.TabIndex = 6;
            this.progressBar.TextBackdropColor = System.Drawing.Color.Empty;
            this.progressBar.TextShadowColor = System.Drawing.Color.Empty;
            this.progressBar.Values.Text = "";
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 100;
            this.toolTip1.ReshowDelay = 100;
            this.toolTip1.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip1_Popup);
            // 
            // logToggleButton
            // 
            this.logToggleButton.Location = new System.Drawing.Point(12, 618);
            this.logToggleButton.Name = "logToggleButton";
            this.logToggleButton.Size = new System.Drawing.Size(54, 25);
            this.logToggleButton.TabIndex = 17;
            this.toolTip1.SetToolTip(this.logToggleButton, "Show or hide the log window.");
            this.logToggleButton.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            this.logToggleButton.Values.Text = "Log";
            this.logToggleButton.CheckedChanged += new System.EventHandler(this.logToggleButton_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(643, 655);
            this.Controls.Add(this.logToggleButton);
            this.Controls.Add(this.themeButton);
            this.Controls.Add(this.aboutButton);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.optionsSection);
            this.Controls.Add(this.filesSection);
            this.Controls.Add(this.generateButton);
            this.MinimumSize = new System.Drawing.Size(425, 466);
            this.Name = "MainForm";
            this.Text = "Movie BarCode Generator";
            ((System.ComponentModel.ISupportInitialize)(this.filesSection.Panel)).EndInit();
            this.filesSection.Panel.ResumeLayout(false);
            this.filesSection.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.filesSection)).EndInit();
            this.filesSection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.optionsSection.Panel)).EndInit();
            this.optionsSection.Panel.ResumeLayout(false);
            this.optionsSection.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.optionsSection)).EndInit();
            this.optionsSection.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Krypton.Toolkit.KryptonManager kryptonManager1;
        private Krypton.Toolkit.KryptonTextBox inputPathTextBox;
        private Krypton.Toolkit.KryptonButton browseInputPathButton;
        private Krypton.Toolkit.KryptonButton generateButton;
        private Krypton.Toolkit.KryptonLabel label1;
        private Krypton.Toolkit.KryptonGroupBox filesSection;
        private Krypton.Toolkit.KryptonLabel label2;
        private Krypton.Toolkit.KryptonTextBox outputPathTextBox;
        private Krypton.Toolkit.KryptonButton browseOutputPathButton;
        private Krypton.Toolkit.KryptonLabel label10;
        private Krypton.Toolkit.KryptonTextBox postfixTextBox;
        private Krypton.Toolkit.KryptonLabel label11;
        private Krypton.Toolkit.KryptonTextBox extensionsTextBox;
        private Krypton.Toolkit.KryptonGroupBox optionsSection;
        private Krypton.Toolkit.KryptonLabel barWidthLabel;
        private Krypton.Toolkit.KryptonTextBox barWidthTextBox;
        private Krypton.Toolkit.KryptonLabel barCountLabel;
        private Krypton.Toolkit.KryptonTextBox barCountTextBox;
        private Krypton.Toolkit.KryptonCheckBox excludeCreditsCheckBox;
        private Krypton.Toolkit.KryptonLabel label4;
        private Krypton.Toolkit.KryptonTextBox imageHeightTextBox;
        private Krypton.Toolkit.KryptonLabel outputImageSizeLabel;
        private Krypton.Toolkit.KryptonTextBox imageWidthTextBox;
        private Krypton.Toolkit.KryptonProgressBar progressBar;
        private Krypton.Toolkit.KryptonButton aboutButton;
        private Krypton.Toolkit.KryptonCheckButton themeButton;
        private Krypton.Toolkit.KryptonLabel precendenceNote;
        private System.Windows.Forms.ToolTip toolTip1;
        private Krypton.Toolkit.KryptonCheckButton logToggleButton;
        private Krypton.Toolkit.KryptonCheckedListBox barGeneratorList;
        private Krypton.Toolkit.KryptonLabel modeSelectionLabel;
        private Krypton.Toolkit.KryptonTextBox generatorInfoBody;
        private Krypton.Toolkit.KryptonCheckBox overlayWaveformCheckBox;
        private Krypton.Toolkit.KryptonLabel waveformStrengthLabel;
        private Krypton.Toolkit.KryptonLabel waveformStrengthValueLabel;
        private Krypton.Toolkit.KryptonTrackBar waveformStrengthTrackBar;
        private Krypton.Toolkit.KryptonLabel waveformColorLabel;
        private Krypton.Toolkit.KryptonColorButton waveformColorButton;
    }
}
