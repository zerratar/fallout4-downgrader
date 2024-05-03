using System.Diagnostics;
using Terminal.Gui;
using static QRCoder.PayloadGenerator;
using static SteamKit2.Internal.CMsgClientUserNotifications;

namespace FO4Down.Windows
{
    internal class PatchDialog : SimpleDialog
    {
        private ApplicationContext ctx;
        private Label lblAutomaticInstall;
        private Timer timer;
        private Button btnPatch;
        private Button btnDowngrade;
        private CheckBox cbf4seAddressLibrary;
        private CheckBox cbf4se;
        private CheckBox cbBAAS;
        private Label lblInstallBAAS;
        private Label lblInstallAddress;
        private Label lblInstallF4SE;
        private Label reqLabel;

        public PatchDialog(ApplicationContext ctx)
            : base()
        {
            this.ctx = ctx;

            Title = "Select type of Downgrade";
            Width = Dim.Percent(70);
            Height = 20;

            var notification = Lbl("It is possible to Patch Fallout 4 instead of downgrading it.\n\n" +
                "This will allow you to play the old version of the game but with the latest data files," +
                "but it may not be compatible with all mods.\n\n" +
                "If you decide to do a normal downgrade, this will download all depots\n(~32gb of files) " +
                "and requires your steam username/password to proceed.\n", null, this);

            if (!ctx.IsF4SEAddressLibraryInstalled || !ctx.IsF4SEInstalled || !ctx.IsF4SEBASSInstalled)
            {

                reqLabel = Lbl("Requirements for patching", notification);
                var requirements = new View
                {
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                    BorderStyle = LineStyle.Single,
                    X = 1,
                    Y = Pos.Bottom(reqLabel),
                };
                var startTimer = false;
                cbf4se = Check("F4SE", parent: requirements);
                cbf4se.Enabled = false;
                cbf4se.Checked = ctx.IsF4SEInstalled;
                if (!cbf4se.Checked.GetValueOrDefault())
                {
                    startTimer = true;
                    lblInstallF4SE = Lbl("<Install>", parent: requirements);
                    lblInstallF4SE.MouseClick += (s, e) =>
                    {
                        OpenURL("https://www.nexusmods.com/Core/Libs/Common/Widgets/DownloadPopUp?id=253312&game_id=1151");
                    };
                    lblInstallF4SE.TextAlignment = TextAlignment.Centered;
                    lblInstallF4SE.Width = 16;
                    lblInstallF4SE.Y = cbf4se.Y;
                    lblInstallF4SE.X = Pos.Right(requirements) - 20;
                }

                cbf4seAddressLibrary = Check("F4SE Plugin: Address Library", other: cbf4se, parent: requirements);
                cbf4seAddressLibrary.Enabled = false;
                cbf4seAddressLibrary.Checked = ctx.IsF4SEAddressLibraryInstalled;
                if (!cbf4seAddressLibrary.Checked.GetValueOrDefault())
                {
                    startTimer = true;
                    lblInstallAddress = Lbl("<Install>", parent: requirements);
                    lblInstallAddress.MouseClick += (s, e) =>
                    {
                        OpenURL("https://www.nexusmods.com/Core/Libs/Common/Widgets/DownloadPopUp?id=191018&game_id=1151");
                    };

                    lblInstallAddress.TextAlignment = TextAlignment.Centered;
                    lblInstallAddress.Width = 16;
                    lblInstallAddress.Y = cbf4seAddressLibrary.Y;
                    lblInstallAddress.X = Pos.Right(requirements) - 20;
                }

                cbBAAS = Check("F4SE Plugin: Backported Archive2 Support System", other: cbf4seAddressLibrary, parent: requirements);
                cbBAAS.Enabled = false;
                cbBAAS.Checked = ctx.IsF4SEBASSInstalled;
                if (!cbBAAS.Checked.GetValueOrDefault())
                {
                    startTimer = true;
                    lblInstallBAAS = Lbl("<Install>", parent: requirements);
                    lblInstallBAAS.MouseClick += (s, e) =>
                    {
                        OpenURL("https://www.nexusmods.com/fallout4/mods/81859?tab=files&file_id=313321");
                    };
                    lblInstallBAAS.TextAlignment = TextAlignment.Centered;
                    lblInstallBAAS.Width = 16;
                    lblInstallBAAS.Y = cbBAAS.Y;
                    lblInstallBAAS.X = Pos.Right(requirements) - 20;
                }

                if (startTimer)
                {
                    lblAutomaticInstall = Lbl("<Install All Automatically>", parent: requirements);
                    lblAutomaticInstall.Y = Pos.Bottom(cbBAAS) + 1;
                    lblAutomaticInstall.MouseClick += async (s, e) => await InstallAllPlugins();
                    timer = new Timer(CheckForInstallStates, null, 5000, 1000);
                }

                Add(requirements);
            }
            else
            {
                reqLabel = Lbl("All requirements for patching has been met!", notification);
                reqLabel.ColorScheme = new ColorScheme
                {
                    Normal = new Terminal.Gui.Attribute(ColorName.Green, this.ColorScheme.Normal.Background)
                };
            }

            btnPatch = Btn("Yes patch it!", notification, BtnPatchClicked);
            btnDowngrade = Btn("Normal Downgrade", notification, BtnDowngradeClicked);
        }

        private async Task InstallAllPlugins()
        {
            var result = MessageBox.Query("Install into Fallout 4 folder?", "No support for MO2 yet.\nAll files will be installed directly under /Fallout 4/ folder.\nDo you wish to continue?", "Yes", "No, I will do this manually.");
            if (result == 0)
            {
                if (!ctx.IsF4SEInstalled)
                {
                    await FO4Downgrader.InstallF4SEAsync(ctx);
                }

                if (!ctx.IsF4SEAddressLibraryInstalled)
                {
                    await FO4Downgrader.InstallAddressLibraryPluginAsync(ctx);
                }

                if (!ctx.IsF4SEBASSInstalled)
                {
                    await FO4Downgrader.InstallBASSAsync(ctx);
                }
            }
        }

        private void CheckForInstallStates(object? state)
        {
            FO4Downgrader.CheckIfF4SEIsInstalled(ctx);
            FO4Downgrader.CheckIfF4SEBAASIsInstalled(ctx);
            FO4Downgrader.CheckIfF4SEAddressLibraryIsInstalled(ctx);

            Application.Invoke(() =>
            {
                cbf4se.Checked = ctx.IsF4SEInstalled;
                if (ctx.IsF4SEInstalled && lblInstallF4SE != null)
                {
                    lblInstallF4SE.Visible = false;
                }

                cbf4seAddressLibrary.Checked = ctx.IsF4SEAddressLibraryInstalled;
                if (ctx.IsF4SEAddressLibraryInstalled && lblInstallAddress != null)
                {
                    lblInstallAddress.Visible = false;
                }

                cbBAAS.Checked = ctx.IsF4SEBASSInstalled;
                if (ctx.IsF4SEBASSInstalled && lblInstallBAAS != null)
                {
                    lblInstallBAAS.Visible = false;
                }

                if (ctx.IsF4SEBASSInstalled && ctx.IsF4SEInstalled && ctx.IsF4SEAddressLibraryInstalled)
                {
                    timer.Dispose();
                    timer = null;

                    reqLabel.Text = "All requirements for patching has been met!";
                    reqLabel.ColorScheme = new ColorScheme
                    {
                        Normal = new Terminal.Gui.Attribute(ColorName.Green, this.ColorScheme.Normal.Background)
                    };
                }
            });
        }

        private void OpenURL(string url)
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        private void BtnDowngradeClicked()
        {
            if (timer != null)
                timer.Dispose();
            result = false;
            RequestStop();
        }

        private void BtnPatchClicked()
        {
            if (!ctx.IsF4SEAddressLibraryInstalled || !ctx.IsF4SEInstalled || !ctx.IsF4SEBASSInstalled)
            {
                if (MessageBox.Query("Requirements not met", "You still have not installed the required plugins, the patched Fallout 4 wont work without it\nIf you do not wish to do it now, you can do it after the patch is completed.\nDo you still want to apply the patch?", "Yes! I will do it after", "Cancel") != 0)
                {
                    return;
                }
            }
            if (timer != null)
                timer.Dispose();
            result = true;
            RequestStop();
        }
    }
}
