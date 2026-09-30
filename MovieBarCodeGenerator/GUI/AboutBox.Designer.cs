namespace MovieBarCodeGenerator.GUI
{
    partial class AboutBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.titleLabel = new Krypton.Toolkit.KryptonLabel();
            this.textLabel = new Krypton.Toolkit.KryptonLabel();
            this.linkLabel = new Krypton.Toolkit.KryptonLinkLabel();
            this.SuspendLayout();
            // 
            // kryptonManager1
            // 
            this.kryptonManager1.GlobalPaletteMode = Krypton.Toolkit.PaletteMode.Global;
            this.kryptonManager1.ToolkitStrings.MessageBoxStrings.LessDetails = "L&ess Details...";
            this.kryptonManager1.ToolkitStrings.MessageBoxStrings.MoreDetails = "&More Details...";
            // 
            // titleLabel
            // 
            this.titleLabel.LabelStyle = Krypton.Toolkit.LabelStyle.TitleControl;
            this.titleLabel.Location = new System.Drawing.Point(14, 19);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(65, 29);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Values.Text = "label1";
            // 
            // textLabel
            // 
            this.textLabel.Location = new System.Drawing.Point(14, 51);
            this.textLabel.Name = "textLabel";
            this.textLabel.Size = new System.Drawing.Size(43, 20);
            this.textLabel.TabIndex = 1;
            this.textLabel.Values.Text = "label1";
            // 
            // linkLabel
            // 
            this.linkLabel.Location = new System.Drawing.Point(14, 95);
            this.linkLabel.Name = "linkLabel";
            this.linkLabel.Size = new System.Drawing.Size(65, 20);
            this.linkLabel.TabIndex = 2;
            this.linkLabel.Values.Text = "linkLabel1";
            // 
            // AboutBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 159);
            this.Controls.Add(this.linkLabel);
            this.Controls.Add(this.textLabel);
            this.Controls.Add(this.titleLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AboutBox";
            this.Load += new System.EventHandler(this.AboutBox_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Krypton.Toolkit.KryptonManager kryptonManager1;
        private Krypton.Toolkit.KryptonLabel titleLabel;
        private Krypton.Toolkit.KryptonLabel textLabel;
        private Krypton.Toolkit.KryptonLinkLabel linkLabel;
    }
}
