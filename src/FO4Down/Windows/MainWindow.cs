using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;
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

        private readonly ColorScheme SuccessLabelColorScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black),
        };

        private readonly ColorScheme DefaultLabelColorScheme;
        private readonly FO4Downgrader downgrader;

        private Label lblLogo;
        private Label lblContact;
        private Label lblStatus;
        private Label lblProgress;
        private Label lblQr;

        private ProgressBar progressBar;

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

                //#if DEBUG
                //new MenuBarItem("_DEBUG", new MenuItem[]
                //{
                //    new MenuItem("_Clear Settings", "", () => {
                //        var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                //        foreach(var f in isolatedStorage.GetFileNames())
                //        {
                //            isolatedStorage.DeleteFile(f);
                //        }
                //        foreach(var d in isolatedStorage.GetDirectoryNames()){
                //            isolatedStorage.DeleteDirectory(d);
                //        }
                //    })
                //})
                //#endif
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
            lblContact.Text = $"E-mail: zerratar@gmail.com | Discord: zerratar | Source: https://www.github.com/zerratar/fallout4-downgrader";
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
        private void OnStepUpdate()
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

                    if (AppContext.IsError)
                    {
                        lblStatus.ColorScheme = ErrorLabelColorScheme;
                        lblStatus.Text = "Error: " + AppContext.Message;
                    }
                    else if (AppContext.IsWarning)
                    {
                        lblStatus.ColorScheme = WarningLabelColorScheme;
                        lblStatus.Text = AppContext.Message;
                    }
                    else if (AppContext.IsSuccess)
                    {
                        lblStatus.ColorScheme = SuccessLabelColorScheme;
                        lblStatus.Text = AppContext.Message;
                    }
                    else
                    {
                        lblStatus.ColorScheme = DefaultLabelColorScheme;
                    }


                    if (AppContext.ReportToDeveloper)
                    {
                        File.WriteAllText("error.txt", BuildErrorReport());
                        if (AppContext.IsError)
                        {
                            MessageBox.ErrorQuery("Unexpected Error", "An unexpected error occurred: " + AppContext.Message + "\nA full report has been saved to error.txt\nPlease report this to zerratar", "OK");
                        }
                        else
                        {
                            MessageBox.ErrorQuery("Warning", AppContext.Message + "\nA full report has been saved to error.txt\nPlease report this to zerratar if the game is not working as expected", "OK");
                        }

                        try
                        {
                            var dir = AppContext.GetWorkingDirectory();
                            Shell32.OpenFolderAndSelectItem(dir, "error.txt");
                        }
                        catch { }

                        RequestStop();
                        return;
                    }

                    if (AppContext.IsError && !AppContext.Continue)
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(AppContext.Message))
                    {
                        lblStatus.Text = AppContext.Message;
                    }

                    switch (AppContext.Step)
                    {
                        case FO4DowngraderStep.LookingForFallout4Path:
                            if (AppContext.Request != null)
                            {
                                switch (AppContext.Request.Name)
                                {
                                    case "confirm":
                                        var result = MessageBox.Query("Confirm", AppContext.Request.Arguments[0], "Yes", "No") == 0;
                                        AppContext.Next(result);
                                        break;
                                }
                            }
                            break;
                        case FO4DowngraderStep.Patch:

                            if (AppContext.Request != null)
                            {
                                switch (AppContext.Request.Name)
                                {
                                    case "confirm":
                                        var pd = new PatchDialog();
                                        if (pd.ShowDialog())
                                        {
                                            AppContext.Next(true);
                                        }
                                        else
                                        {
                                            AppContext.Next(false);
                                        }
                                        break;
                                }
                            }

                            break;
                        case FO4DowngraderStep.ApplyLanguage:
                            if (AppContext.Request != null)
                            {
                                switch (AppContext.Request.Name)
                                {
                                    case "confirm":
                                        var result = MessageBox.Query("Confirm", AppContext.Request.Arguments[0], "Yes", "No") == 0;
                                        AppContext.Next(result);
                                        break;
                                }
                            }
                            break;
                        case FO4DowngraderStep.UserSettings:
                            if (AppContext.Request != null && AppContext.Request.Name == "settings")
                            {
                                var userSettings = new UserSettingsDialog();
                                userSettings.ShowDialog();
                                AppContext.Next(userSettings.Settings);
                            }
                            break;
                        case FO4DowngraderStep.LoginToSteam:
                            if (AppContext.Request != null)
                            {
                                switch (AppContext.Request.Name)
                                {
                                    case "auth_code":
                                        var auth = new SteamAuthCodeDialog();
                                        if (auth.ShowDialog())
                                        {
                                            AppContext.Next(auth.AuthCode);
                                        }
                                        break;
                                    case "credentials":
                                        var login = new SteamLoginDialog();
                                        if (login.ShowDialog())
                                        {
                                            if (login.QR)
                                            {
                                                AppContext.Settings.UseQrCode = true;
                                                AppContext.Next(((string)null, (string)null));
                                            }
                                            else
                                            {
                                                AppContext.Next((login.Username, login.Password));
                                            }
                                        }
                                        else
                                        {
                                            AppContext.Next(((string)null, (string)null));
                                        }
                                        break;
                                }
                                // context.Next(("username", "password"));
                            }
                            else if (!string.IsNullOrEmpty(AppContext.QRCode))
                            {
                                lblQr.Text = AppContext.QRCode;
                                lblQr.Visible = true;
                            }
                            break;
                        case FO4DowngraderStep.DownloadDepotFiles:
                            if (AppContext.Request != null)
                            {
                                switch (AppContext.Request.Name)
                                {
                                    case "confirm":
                                        var result = MessageBox.Query("Confirm", AppContext.Request.Arguments[0], "Yes", "No") == 0;
                                        AppContext.Next(result);
                                        break;
                                }
                            }
                            break;
                        case FO4DowngraderStep.DownloadCreationKitDepotFiles:
                        case FO4DowngraderStep.DownloadGameDepotFiles:
                            progressBar.Visible = AppContext.Fraction > 0.0;
                            lblProgress.Visible = progressBar.Visible;
                            lblProgress.Text = $"{(AppContext.Fraction * 100):0.00}%\n\n" + "Processing depot " + (AppContext.DepotsDownloaded + 1) + " out of " + AppContext.TotalDepotsToDownload + ".\nThis will take a while! Do not worry if nothing happens for a while.";// + context.GetAverageDownloadSpeed();
                            progressBar.Fraction = AppContext.Fraction;
                            break;
                        
                        case FO4DowngraderStep.DownloadPatchFiles:
                        case FO4DowngraderStep.CopyDepotFiles:
                        case FO4DowngraderStep.DeleteNextGenFiles:
                            progressBar.Visible = true;
                            lblProgress.Visible = true;
                            lblProgress.Text = $"{(AppContext.Fraction * 100):0.00}%";
                            progressBar.Fraction = AppContext.Fraction;
                            break;
                    }
                }
                catch (Exception exc)
                {
                    AppContext.Error(exc);
                }
                finally
                {
                    if (lblStatus != null)
                    {
                        // update size of lblStatus to ensure all text are shown
                        if (lblStatus.Text.Length > 0)
                        {
                            lblStatus.Height = lblStatus.Text.Split('\n').Length;
                        }
                    }
                    Interlocked.Exchange(ref runningStepUpdate, 0);
                }
            });
        }

        private string? BuildErrorReport()
        {
            var sb = new StringBuilder();
            var s = AppContext.Settings;
            sb.AppendLine("Version: " + GetVersion());
            sb.AppendLine();
            sb.AppendLine("[Settings]");
            sb.AppendLine("QR: " + s.UseQrCode);
            sb.AppendLine("Authenticated: " + AppContext.IsAuthenticated);
            sb.AppendLine("Language: " + (s.DownloadAllLanguages ? "All" : s.Language));
            sb.AppendLine("Downgrade Creation Kit: " + s.DownloadCreationKit);
            sb.AppendLine("Downgrade All DLCs: " + s.DownloadAllDLCs);
            sb.AppendLine("Delete Creation Club files: " + s.DeleteCreationClubFiles);
            sb.AppendLine("Keep Depot: " + s.KeepDepotFiles);
            sb.AppendLine();

            if (AppContext.LoggedErrors.Count > 0)
            {
                sb.AppendLine("[Previous Errors]");
                for (var i = 0; i < AppContext.LoggedErrors.Count; ++i)
                {
                    var err = AppContext.LoggedErrors[i];
                    sb.AppendLine("Error #" + (i + 1) + ": " + err);
                    sb.AppendLine();
                }
            }

            if (AppContext.Exception != null)
            {
                sb.AppendLine("[Crashing Error]");
            }
            else
            {
                sb.AppendLine(AppContext.LastErrorMessage);
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
