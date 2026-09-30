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
            this.groupBox1 = new Krypton.Toolkit.KryptonGroupBox();
            this.label11 = new Krypton.Toolkit.KryptonLabel();
            this.extensionsTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.label10 = new Krypton.Toolkit.KryptonLabel();
            this.postfixTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.label2 = new Krypton.Toolkit.KryptonLabel();
            this.outputPathTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.browseOutputPathButton = new Krypton.Toolkit.KryptonButton();
            this.aboutButton = new Krypton.Toolkit.KryptonButton();
            this.themeButton = new Krypton.Toolkit.KryptonCheckButton();
            this.groupBox2 = new Krypton.Toolkit.KryptonGroupBox();
            this.generatorInfoBody = new Krypton.Toolkit.KryptonTextBox();
            this.label9 = new Krypton.Toolkit.KryptonLabel();
            this.excludeCreditsCheckBox = new Krypton.Toolkit.KryptonCheckBox();
            this.barGeneratorList = new Krypton.Toolkit.KryptonCheckedListBox();
            this.label7 = new Krypton.Toolkit.KryptonLabel();
            this.label6 = new Krypton.Toolkit.KryptonLabel();
            this.barWidthTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.label5 = new Krypton.Toolkit.KryptonLabel();
            this.barCountTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.useInputHeightForOutputCheckBox = new Krypton.Toolkit.KryptonCheckBox();
            this.label4 = new Krypton.Toolkit.KryptonLabel();
            this.imageHeightTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.label3 = new Krypton.Toolkit.KryptonLabel();
            this.imageWidthTextBox = new Krypton.Toolkit.KryptonTextBox();
            this.progressBar1 = new Krypton.Toolkit.KryptonProgressBar();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.logToggleButton = new Krypton.Toolkit.KryptonCheckButton();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox1.Panel)).BeginInit();
            this.groupBox1.Panel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox2.Panel)).BeginInit();
            this.groupBox2.Panel.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            // 
            // groupBox1.Panel
            // 
            this.groupBox1.Panel.Controls.Add(this.label11);
            this.groupBox1.Panel.Controls.Add(this.extensionsTextBox);
            this.groupBox1.Panel.Controls.Add(this.label10);
            this.groupBox1.Panel.Controls.Add(this.postfixTextBox);
            this.groupBox1.Panel.Controls.Add(this.label2);
            this.groupBox1.Panel.Controls.Add(this.outputPathTextBox);
            this.groupBox1.Panel.Controls.Add(this.browseOutputPathButton);
            this.groupBox1.Panel.Controls.Add(this.label1);
            this.groupBox1.Panel.Controls.Add(this.inputPathTextBox);
            this.groupBox1.Panel.Controls.Add(this.browseInputPathButton);
            this.groupBox1.Size = new System.Drawing.Size(619, 231);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.Values.Heading = "Files";
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
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(12, 249);
            // 
            // groupBox2.Panel
            // 
            this.groupBox2.Panel.Controls.Add(this.generatorInfoBody);
            this.groupBox2.Panel.Controls.Add(this.label9);
            this.groupBox2.Panel.Controls.Add(this.excludeCreditsCheckBox);
            this.groupBox2.Panel.Controls.Add(this.barGeneratorList);
            this.groupBox2.Panel.Controls.Add(this.label7);
            this.groupBox2.Panel.Controls.Add(this.label6);
            this.groupBox2.Panel.Controls.Add(this.barWidthTextBox);
            this.groupBox2.Panel.Controls.Add(this.label5);
            this.groupBox2.Panel.Controls.Add(this.barCountTextBox);
            this.groupBox2.Panel.Controls.Add(this.useInputHeightForOutputCheckBox);
            this.groupBox2.Panel.Controls.Add(this.label4);
            this.groupBox2.Panel.Controls.Add(this.imageHeightTextBox);
            this.groupBox2.Panel.Controls.Add(this.label3);
            this.groupBox2.Panel.Controls.Add(this.imageWidthTextBox);
            this.groupBox2.Size = new System.Drawing.Size(619, 365);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.Values.Heading = "Barcode parameters";
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
            this.generatorInfoBody.Size = new System.Drawing.Size(324, 204);
            this.generatorInfoBody.StateCommon.Border.Draw = Krypton.Toolkit.InheritBool.False;
            this.generatorInfoBody.TabIndex = 15;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(1, 109);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(233, 20);
            this.label9.TabIndex = 20;
            this.label9.Values.Text = "Generate the following barcode versions:";
            // 
            // excludeCreditsCheckBox
            // 
            this.excludeCreditsCheckBox.Location = new System.Drawing.Point(6, 85);
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
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(190, 64);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(210, 20);
            this.label7.TabIndex = 17;
            this.label7.Values.Text = "Note: bar width will take precedence";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(258, 15);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(119, 20);
            this.label6.TabIndex = 16;
            this.label6.Values.Text = "Bar width (in pixels):";
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
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(187, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 20);
            this.label5.TabIndex = 14;
            this.label5.Values.Text = "Bar count:";
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
            // useInputHeightForOutputCheckBox
            // 
            this.useInputHeightForOutputCheckBox.Location = new System.Drawing.Point(6, 64);
            this.useInputHeightForOutputCheckBox.Name = "useInputHeightForOutputCheckBox";
            this.useInputHeightForOutputCheckBox.Size = new System.Drawing.Size(193, 20);
            this.useInputHeightForOutputCheckBox.TabIndex = 8;
            this.useInputHeightForOutputCheckBox.Values.Text = "Same height as the input video";
            this.useInputHeightForOutputCheckBox.CheckedChanged += new System.EventHandler(this.useInputHeightForOutputCheckBox_CheckedChanged);
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
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(3, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(126, 20);
            this.label3.TabIndex = 1;
            this.label3.Values.Text = "Image size (in pixels):";
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
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(72, 618);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(478, 25);
            this.progressBar1.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(184)))), ((int)(((byte)(234)))), ((int)(((byte)(255)))));
            this.progressBar1.StateCommon.Back.Color2 = System.Drawing.Color.Black;
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 6;
            this.progressBar1.TextBackdropColor = System.Drawing.Color.Empty;
            this.progressBar1.TextShadowColor = System.Drawing.Color.Empty;
            this.progressBar1.Values.Text = "";
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
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.generateButton);
            this.MinimumSize = new System.Drawing.Size(425, 466);
            this.Name = "MainForm";
            this.Text = "Movie BarCode Generator";
            ((System.ComponentModel.ISupportInitialize)(this.groupBox1.Panel)).EndInit();
            this.groupBox1.Panel.ResumeLayout(false);
            this.groupBox1.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupBox2.Panel)).EndInit();
            this.groupBox2.Panel.ResumeLayout(false);
            this.groupBox2.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Krypton.Toolkit.KryptonManager kryptonManager1;
        private Krypton.Toolkit.KryptonTextBox inputPathTextBox;
        private Krypton.Toolkit.KryptonButton browseInputPathButton;
        private Krypton.Toolkit.KryptonButton generateButton;
        private Krypton.Toolkit.KryptonLabel label1;
        private Krypton.Toolkit.KryptonGroupBox groupBox1;
        private Krypton.Toolkit.KryptonLabel label2;
        private Krypton.Toolkit.KryptonTextBox outputPathTextBox;
        private Krypton.Toolkit.KryptonButton browseOutputPathButton;
        private Krypton.Toolkit.KryptonLabel label10;
        private Krypton.Toolkit.KryptonTextBox postfixTextBox;
        private Krypton.Toolkit.KryptonLabel label11;
        private Krypton.Toolkit.KryptonTextBox extensionsTextBox;
        private Krypton.Toolkit.KryptonGroupBox groupBox2;
        private Krypton.Toolkit.KryptonLabel label6;
        private Krypton.Toolkit.KryptonTextBox barWidthTextBox;
        private Krypton.Toolkit.KryptonLabel label5;
        private Krypton.Toolkit.KryptonTextBox barCountTextBox;
        private Krypton.Toolkit.KryptonCheckBox useInputHeightForOutputCheckBox;
        private Krypton.Toolkit.KryptonCheckBox excludeCreditsCheckBox;
        private Krypton.Toolkit.KryptonLabel label4;
        private Krypton.Toolkit.KryptonTextBox imageHeightTextBox;
        private Krypton.Toolkit.KryptonLabel label3;
        private Krypton.Toolkit.KryptonTextBox imageWidthTextBox;
        private Krypton.Toolkit.KryptonProgressBar progressBar1;
        private Krypton.Toolkit.KryptonButton aboutButton;
        private Krypton.Toolkit.KryptonCheckButton themeButton;
        private Krypton.Toolkit.KryptonLabel label7;
        private System.Windows.Forms.ToolTip toolTip1;
        private Krypton.Toolkit.KryptonCheckButton logToggleButton;
        private Krypton.Toolkit.KryptonCheckedListBox barGeneratorList;
        private Krypton.Toolkit.KryptonLabel label9;
        private Krypton.Toolkit.KryptonTextBox generatorInfoBody;
    }
}
