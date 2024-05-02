using Fallout4Downgrader;
using System.Globalization;
using Terminal.Gui;

namespace FO4Down.Windows
{
    internal class UserSettingsDialog : SimpleDialog
    {
        private DowngradeContext ctx;
        private ComboBox cbLanguage;
        private CheckBox cbDownloadAllDlcs;
        private CheckBox cbHDTextures;
        private CheckBox cbEnglishFiles;
        private CheckBox cbKeepDepots;
        private CheckBox cbCreationKit;
        private CheckBox cbCreationClub;
        private Button btnOK;
        private string[] languages = [/*"All", */ "English", "French", "German", "Italian", "Spanish", "Polish", "Russian", "Portuguese", "Traditional Chinese", "Japanese"];

        private TabView tabView;
        private bool delEnglishFilesWasSelected;
        private bool lastCbEnglishFilesEnabled;

        public UserProvidedSettings Settings { get; set; }

        public UserSettingsDialog(DowngradeContext ctx)
            : base()
        {
            this.ctx = ctx;
            this.Settings = new UserProvidedSettings();

            Title = "Settings";
            Width = Dim.Percent(65);

            Label noteLabel = null;
            if (ctx.IsAuthenticated)
            {
                noteLabel = Lbl("Before you get started,\nplease select the settings you want to use", null, this);
                Height = 20;
            }
            else
            {
                Height = 16;
            }

            var pos = noteLabel != null ? Pos.Bottom(noteLabel) + 1 : 0;

            tabView = new TabView();
            tabView.Width = Dim.Fill();
            tabView.Height = Dim.Fill();
            tabView.Y = pos;
            Add(tabView);

            var basic = new Tab();
            basic.View = new View
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            basic.Width = Dim.Fill();
            basic.Height = Dim.Fill();
            basic.DisplayText = "Basic";
            {
                cbLanguage = Combo("Select Language (Press arrow and use mouse scroll)", cbKeepDepots, basic.View, OnLanguageChanged, languages);

                var defaultLanguage = ctx.GetTargetCultureInfo();
                var defeaultLanguageIndex = GetLanguageIndex(defaultLanguage);
                if (defeaultLanguageIndex == -1)
                {
                    defeaultLanguageIndex = Array.IndexOf(languages, "English"); // english
                }

                if (ctx.Settings.DownloadAllLanguages)
                {
                    cbLanguage.SelectedItem = 0;
                }
                else
                {
                    SetSelectedLanguage(ctx.Settings.Language, defeaultLanguageIndex);
                }

                //Check
            }
            tabView.AddTab(basic, true);

            var advanced = new Tab();
            advanced.View = new View
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            advanced.Width = Dim.Fill();
            advanced.Height = Dim.Fill();
            advanced.DisplayText = "Advanced";
            {
                //var note = Lbl("Note: Automatron and Wasteland Workshop\nare always downgraded.\n", parent: advanced.View);
                var note = Lbl("You don't need to select anything here\nOnly use this if you're having trouble with downgrade.\n", parent: advanced.View);
                //cbDownloadAllDlcs = Check("Downgrade Contraptions, Far Harbor, Vault-Tec and Nuka World?", note, advanced.View, (_, value) => Settings.DownloadAllDLCs = value.GetValueOrDefault());
                //cbHDTextures = Check("Downgrade HD Textures", cbDownloadAllDlcs, advanced.View, (_, value) => Settings.DownloadHDTextures = value.GetValueOrDefault());

                cbCreationClub = Check("Delete Creation Club files", note, advanced.View, (_, value) => Settings.DeleteCreationClubFiles = value.GetValueOrDefault());
                cbCreationClub.Checked = Settings.DeleteCreationClubFiles;

                cbCreationKit = Check("Downgrade Creation Kit", cbCreationClub, advanced.View, (_, value) => Settings.DownloadCreationKit = value.GetValueOrDefault());
                cbCreationKit.Checked = ctx.DownloadCreationKit;

                cbEnglishFiles = Check("Delete English Language Files", cbCreationKit, advanced.View,
                    (_, value) => Settings.DeleteEnglishLanguageFiles = value.GetValueOrDefault());
                cbEnglishFiles.Enabled = cbLanguage.SelectedItem > 1;

                cbKeepDepots = Check("Keep Depots when done (Only if you intend to use them afterwards)", cbEnglishFiles, advanced.View, (_, value) => Settings.KeepDepotFilesWhenDone = value.GetValueOrDefault());
                cbKeepDepots.Checked = Settings.KeepDepotFilesWhenDone;

            }
            tabView.AddTab(advanced, false);

            btnOK = Btn(
                ctx.IsAuthenticated
                ? "Start downgrade"
                : "OK", null, BtnOKClicked);
        }

        private int GetLanguageIndex(string lang)
        {
            if (string.IsNullOrEmpty(lang))
            {
                return -1;
            }

            if (lang.Contains('('))
            {
                lang = lang.Remove(lang.IndexOf('(')).Trim();
            }

            var target = languages.FirstOrDefault(x => x.Equals(lang, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                return Array.IndexOf(languages, target);
            }

            return -1;
        }

        private int GetLanguageIndex(CultureInfo cultureInfo)
        {
            return GetLanguageIndex(cultureInfo.EnglishName);
        }

        private void SetSelectedLanguage(string lang, int fallback)
        {
            var itemIndex = GetLanguageIndex(lang);
            if (itemIndex != -1)
            {
                cbLanguage.SelectedItem = itemIndex;
            }
            else
            {
                cbLanguage.SelectedItem = fallback;
            }
        }

        private void OnLanguageChanged(ComboBox box, int itemIndex, string itemValue)
        {
            if (itemIndex == 0)
            {
                Settings.Language = null; // null is all languages
            }
            else
            {
                Settings.Language = itemValue.ToLower();
            }

            if (cbEnglishFiles != null)
            {
                var isChecked = cbEnglishFiles.Checked.GetValueOrDefault();
                if (isChecked)
                    delEnglishFilesWasSelected = true;

                var shouldCheck = isChecked;

                if (!lastCbEnglishFilesEnabled && delEnglishFilesWasSelected && itemIndex > 1)
                {
                    shouldCheck = true;
                }

                lastCbEnglishFilesEnabled = cbEnglishFiles.Enabled = itemIndex > 1;

                if (!cbEnglishFiles.Enabled)
                {
                    shouldCheck = false;
                }

                cbEnglishFiles.Checked = shouldCheck;
            }
        }

        private void BtnOKClicked()
        {
            result = true;
            RequestStop();
        }
    }
}
