using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace FO4Down.Windows
{
    public class MainWindow : Window
    {
        private readonly Key ctrlQ = new Key(KeyCode.Q).WithCtrl;
        private readonly Key ctrlC = new Key(KeyCode.C).WithCtrl;
        private readonly Key altF4 = new Key(KeyCode.F4).WithAlt;

        private readonly ColorScheme ErrorLabelColorScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Red, Color.Black),
        };

        private readonly ColorScheme DefaultLabelColorScheme;
        private readonly FO4Downgrader downgrader;

        private Label lblLogo;
        private Label lblContact;
        private Label lblStatus;
        private Label lblProgress;

        private ProgressBar progressBar;

        private string contactText;
        private Label lblQr;

        public MainWindow()
        {
            this.downgrader = new FO4Downgrader();

            Title = "Fallout 4 Downgrader (Ctrl+C to quit)";

            X = 0;
            Y = 1;
            Width = Dim.Fill();
            Height = Dim.Fill();

            var menu = new MenuBar();

            menu.Menus = [
                new MenuBarItem("_File", new MenuItem[]
                {
                    new MenuItem("_Quit", "", () => Application.RequestStop())
                }),

                new MenuBarItem("_Help", new MenuItem[]
                {
                    new MenuItem("_About", "", () => MessageBox.Query(64, 10, "About",
                        "FO4Down - Fallout 4 Downgrader\n\n" +
                        "Version "+GetVersion()+"\n" +
                        "Created by zerratar@gmail.com\n" +
                        "https://www.github.com/zerratar/fallout4-downgrader",
                        "OK"))
                }),

                #if DEBUG

                new MenuBarItem("_DEBUG", new MenuItem[]
                {
                    new MenuItem("_Clear Settings", "", () => {
                        var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                        foreach(var f in isolatedStorage.GetFileNames())
                        {
                            isolatedStorage.DeleteFile(f);
                        }

                        foreach(var d in isolatedStorage.GetDirectoryNames()){
                            isolatedStorage.DeleteDirectory(d);
                        }
                    })
                })

                #endif
            ];

            Add(menu);

            lblContact = new Label();

            DefaultLabelColorScheme = lblContact.ColorScheme;

            lblContact.X = 0;
            lblContact.Y = 5;
            lblContact.Height = Dim.Fill();
            lblContact.Width = Dim.Fill();
            lblContact.VerticalTextAlignment = VerticalTextAlignment.Bottom;
            lblContact.TextAlignment = TextAlignment.Centered;
            contactText = lblContact.Text = $"E-mail: zerratar@gmail.com | Discord: zerratar | Source: https://www.github.com/zerratar/fallout4-downgrader";
            Add(lblContact);

            lblLogo = new Label();
            lblLogo.X = 0;
            lblLogo.Y = 3;
            lblLogo.Height = 6;
            lblLogo.Width = Dim.Fill();
            lblLogo.TextAlignment = TextAlignment.Centered;
            lblLogo.Text = $"""
                    ______      ____            __     __ __
                   / ____/___ _/ / /___  __  __/ /_   / // /
                  / /_  / __ `/ / / __ \/ / / / __/  / // /_
                 / __/ / /_/ / / / /_/ / /_/ / /_   /__  __/
                /_/    \__,_/_/_/\____/\__,_/\__/     /_/   
                Downgrader v{GetVersion()}
                """;
            Add(lblLogo);


            lblStatus = new Label();
            lblStatus.X = 0;
            lblStatus.Y = Pos.Bottom(lblLogo) + 1;
            lblStatus.Height = 2;
            lblStatus.Width = Dim.Fill();
            lblStatus.TextAlignment = TextAlignment.Centered;
            lblStatus.Text = "Awaiting Nuclear Detonation... Please wait";

            Add(lblStatus);

            progressBar = new ProgressBar();
            progressBar.Visible = false;
            progressBar.X = Pos.Center() + 1;
            progressBar.Y = Pos.Bottom(lblStatus) + 2;
            progressBar.Width = Dim.Fill() - 6;
            progressBar.Height = 2;
            progressBar.ProgressBarFormat = ProgressBarFormat.Simple;
            progressBar.Fraction = 0.5f;
            Add(progressBar);

            lblProgress = new Label();
            lblProgress.Visible = false;
            lblProgress.X = 0;
            lblProgress.Y = Pos.Bottom(progressBar) + 1;
            lblProgress.Height = 5;
            lblProgress.Width = Dim.Fill();
            lblProgress.TextAlignment = TextAlignment.Centered;
            lblProgress.Text = "0%";
            Add(lblProgress);

            lblQr = new Label();
            lblQr.Visible = false;
            lblQr.X = 0;
            lblQr.Y = 1;
            lblQr.Height = Dim.Fill(2);
            lblQr.Width = Dim.Fill(2);
            lblQr.TextAlignment = TextAlignment.Centered;
            lblQr.VerticalTextAlignment = VerticalTextAlignment.Middle;
            Add(lblQr);


        }

        public override void OnLoaded()
        {
            base.OnLoaded();

            Task.Factory.StartNew(async () => await downgrader.RunAsync(OnStepUpdate));
        }

        private void OnStepUpdate(DowngradeContext context)
        {
            Application.Invoke(() =>
            {
                lblQr.Visible = false;
                progressBar.Visible = false;
                lblProgress.Visible = false;

                if (context.IsError)
                {
                    lblStatus.ColorScheme = ErrorLabelColorScheme;
                    lblStatus.Text = "Error: " + context.Message;
                }

                if (context.ReportToDeveloper)
                {
                    File.WriteAllText("error.txt", context.Exception.ToString());
                    MessageBox.ErrorQuery("Unexpected Error", "An unexpected error occurred: " + context.Message + "\nA full report has been saved to error.txt\nPlease report this to zerratar", "OK");
                    RequestStop();
                    return;
                }

                if (context.IsError && !context.Continue)
                {
                    return;
                }

                lblStatus.ColorScheme = DefaultLabelColorScheme;
                if (!string.IsNullOrEmpty(context.Message))
                {
                    lblStatus.Text = context.Message;
                }

                switch (context.Step)
                {
                    case FO4DowngraderStep.UserSettings:
                        var userSettings = new UserSettingsDialog(context);
                        if (userSettings.ShowDialog())
                        {
                            context.Next(userSettings.Settings);
                            return;
                        }
                        context.Next(null);
                        break;
                    case FO4DowngraderStep.LookingForFallout4Path:
                        lblStatus.Text = "Fallout 4 install path found\n" + context.Fallout4.Path;
                        break;
                    case FO4DowngraderStep.LoginToSteam:
                        if (context.Request != null)
                        {
                            switch (context.Request.Name)
                            {
                                case "auth_code":
                                    var auth = new SteamAuthCodeDialog(context);
                                    if (auth.ShowDialog())
                                    {
                                        context.Next(auth.AuthCode);
                                    }
                                    break;
                                case "credentials":
                                    var login = new SteamLoginDialog(context);
                                    if (login.ShowDialog())
                                    {
                                        if (login.QR)
                                        {
                                            context.Settings.UseQrCode = true;
                                            context.Next(((string)null, (string)null));
                                        }
                                        else
                                        {
                                            context.Next((login.Username, login.Password));
                                        }
                                    }
                                    else
                                    {
                                        context.Next(((string)null, (string)null));
                                    }
                                    break;
                            }
                            // context.Next(("username", "password"));
                        }
                        else if (!string.IsNullOrEmpty(context.QRCode))
                        {
                            lblQr.Text = context.QRCode;
                            lblQr.Visible = true;
                        }
                        break;
                    case FO4DowngraderStep.DownloadDepotFiles:
                        break;
                    case FO4DowngraderStep.DownloadCreationKitDepotFiles:
                    case FO4DowngraderStep.DownloadGameDepotFiles:

                        progressBar.Visible = context.Fraction > 0.0;
                        lblProgress.Visible = progressBar.Visible;
                        lblProgress.Text = $"{(context.Fraction * 100):00.00}%\n\n" + "Processing depot " + (context.DepotsDownloaded + 1) + " out of " + context.TotalDepotsToDownload + ".\nThis will take a while! Do not worry if nothing happens for a while.";// + context.GetAverageDownloadSpeed();
                        progressBar.Fraction = context.Fraction;
                        break;
                    case FO4DowngraderStep.CopyDepotFiles:
                    case FO4DowngraderStep.DeleteNextGenFiles:
                        progressBar.Visible = true;
                        lblProgress.Visible = true;
                        lblProgress.Text = $"{(context.Fraction * 100):00.00}%";
                        progressBar.Fraction = context.Fraction;
                        break;
                }
            });
        }

        public override bool OnKeyDown(Key keyEvent)
        {
            var kc = keyEvent.KeyCode;
            if (kc == ctrlQ || kc == ctrlC || kc == altF4)
            {
                Application.RequestStop();
                return true;
            }

            return base.OnKeyDown(keyEvent);
        }

        private string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
