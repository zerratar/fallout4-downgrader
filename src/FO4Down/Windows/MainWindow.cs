using Newtonsoft.Json.Linq;
using SteamKit2.WebUI.Internal;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
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

        private readonly ColorScheme WarningLabelColorScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Yellow, Color.Black),
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

            Title = "Fallout 4 Downgrader";

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
        private int runningStepUpdate;
        private void OnStepUpdate(ApplicationContext context)
        {
            Application.Invoke(() =>
            {
                // check if this is already running
                if (Interlocked.CompareExchange(ref runningStepUpdate, 1, 0) == 1)
                    return;

                try
                {

                    lblQr.Visible = false;
                    progressBar.Visible = false;
                    lblProgress.Visible = false;

                    if (context.IsError)
                    {
                        lblStatus.ColorScheme = ErrorLabelColorScheme;
                        lblStatus.Text = "Error: " + context.Message;
                    }

                    if (context.IsWarning)
                    {
                        lblStatus.ColorScheme = WarningLabelColorScheme;
                        lblStatus.Text = context.Message;
                    }

                    if (context.ReportToDeveloper)
                    {
                        File.WriteAllText("error.txt", BuildErrorReport(context));
                        if (context.IsError)
                        {
                            MessageBox.ErrorQuery("Unexpected Error", "An unexpected error occurred: " + context.Message + "\nA full report has been saved to error.txt\nPlease report this to zerratar", "OK");
                        }
                        else
                        {
                            MessageBox.ErrorQuery("Warning", context.Message + "\nA full report has been saved to error.txt\nPlease report this to zerratar if the game is not working as expected", "OK");
                        }

                        try
                        {
                            var dir = context.GetWorkingDirectory();
                            Shell32.OpenFolderAndSelectItem(dir, "error.txt");
                        }
                        catch { }

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
                        case FO4DowngraderStep.Patch:

                            if (context.Request != null)
                            {
                                switch (context.Request.Name)
                                {
                                    case "confirm":
                                        var pd = new PatchDialog(context);
                                        if (pd.ShowDialog())
                                        {
                                            context.Next(true);
                                        }
                                        else
                                        {
                                            context.Next(false);
                                        }
                                        break;
                                }
                            }

                            break;
                        case FO4DowngraderStep.ApplyLanguage:
                            if (context.Request != null)
                            {
                                switch (context.Request.Name)
                                {
                                    case "confirm":
                                        var result = MessageBox.Query("Confirm", context.Request.Arguments[0], "Yes", "No") == 0;
                                        context.Next(result);
                                        break;
                                }
                            }
                            break;
                        case FO4DowngraderStep.UserSettings:
                            var userSettings = new UserSettingsDialog(context);
                            userSettings.ShowDialog();
                            context.Next(userSettings.Settings);
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
                            if (context.Request != null)
                            {
                                switch (context.Request.Name)
                                {
                                    case "confirm":
                                        var result = MessageBox.Query("Confirm", context.Request.Arguments[0], "Yes", "No") == 0;
                                        context.Next(result);
                                        break;
                                }
                            }
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
                }
                catch (Exception exc)
                {
                    context.Error(exc);
                }
                finally
                {
                    Interlocked.Exchange(ref runningStepUpdate, 0);
                }
            });
        }

        private string? BuildErrorReport(ApplicationContext context)
        {
            var sb = new StringBuilder();
            var s = context.Settings;
            sb.AppendLine("Version: " + GetVersion());
            sb.AppendLine();
            sb.AppendLine("[Settings]");
            sb.AppendLine("QR: " + s.UseQrCode);
            sb.AppendLine("Authenticated: " + context.IsAuthenticated);
            sb.AppendLine("Language: " + (s.DownloadAllLanguages ? "All" : s.Language));
            sb.AppendLine("Downgrade Creation Kit: " + s.DownloadCreationKit);
            sb.AppendLine("Downgrade All DLCs: " + s.DownloadAllDLCs);
            sb.AppendLine("Delete Creation Club files: " + s.DeleteCreationClubFiles);
            sb.AppendLine("Keep Depot: " + s.KeepDepotFiles);
            sb.AppendLine();

            if (context.LoggedErrors.Count > 0)
            {
                sb.AppendLine("[Previous Errors]");
                for (var i = 0; i < context.LoggedErrors.Count; ++i)
                {
                    var err = context.LoggedErrors[i];
                    sb.AppendLine("Error #" + (i + 1) + ": " + err);
                    sb.AppendLine();
                }
            }

            if (context.Exception != null)
            {
                sb.AppendLine("[Crashing Error]");
            }
            else
            {
                sb.AppendLine(context.LastErrorMessage);
            }

            return sb.ToString();

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
