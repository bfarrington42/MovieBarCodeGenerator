using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace MovieBarCodeGenerator.GUI;

partial class AboutBox : Krypton.Toolkit.KryptonForm
{
    public AboutBox()
    {
        InitializeComponent();
        Text = "About";

        var assemblyInfo = Assembly.GetExecutingAssembly().GetName();

        appName.Text = $"Movie Barcode Generator {assemblyInfo.Version}";

        gplLicenseText.Text = $@"This program is open source, and released under the GPL license.
© Billy Farrington.";

        githubRepoLink.Text = "https://github.com/bfarrington42/MovieBarCodeGenerator";
        githubRepoLink.LinkClicked += (s, e) => Process.Start(githubRepoLink.Text);

        otherSoftwareAttributions.Text = @"The following projects are used within and are subject to their
own licensing/restrictions:";

        magicScalerLicense.Text = @"PhotoSauce MagicScaler (MIT)";

        magicScalerLink.Text = "https://github.com/saucecontrol/PhotoSauce";
        magicScalerLink.LinkClicked += (s, e) => Process.Start(magicScalerLink.Text);

        kryptonLicense.Text = @"Krypton Standard Toolkit (BSD 3-Clause)";

        kryptonLink.Text = "https://github.com/Krypton-Suite/Standard-Toolkit";
        kryptonLink.LinkClicked += (s, e) => Process.Start(kryptonLink.Text);

        nwavesLicense.Text = @"NWaves (MIT)";

        nwavesLink.Text = "https://github.com/ar1st0crat/NWaves";
        nwavesLink.LinkClicked += (s, e) => Process.Start(nwavesLink.Text);

        ffmpegLicense.Text = @"FFmpeg (LGPL-2.1)";

        ffmpegLink.Text = "https://ffmpeg.org/";
        ffmpegLink.LinkClicked += (s, e) => Process.Start(ffmpegLink.Text);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        WindowCorners.ApplyRoundedCorners(this);
    }

    private void AboutBox_Load(object sender, EventArgs e)
    {

    }
}
