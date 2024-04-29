using Fallout4Downgrader;
using Terminal.Gui;

namespace FO4Down.Windows
{
    internal class UserSettingsDialog : SimpleDialog
    {
        private DowngradeContext ctx;
        private ComboBox cbLanguage;
        private CheckBox cbDownloadAllDlcs;
        private CheckBox cbHDTextures;
        private CheckBox cbKeepDepots;
        private CheckBox cbCreationKit;
        private CheckBox cbCreationClub;
        private Button btnOK;
        private string[] languages = ["All", "English", "Polish", "Italian", "Japanese", "Portuguese", "German", "Spanish", "Traditional Chinese", "French"];

        public UserProvidedSettings Settings { get; set; }

        public UserSettingsDialog(DowngradeContext ctx)
            : base()
        {
            this.ctx = ctx;
            this.Settings = new UserProvidedSettings();

            Title = "Settings";
            Width = Dim.Percent(65);
            Height = 19;

            var note = Lbl("Note: Automatron and Wasteland Workshop\nare always included.\n");
            cbDownloadAllDlcs = Check("Download Contraptions, Far Harbor, Vault-Tec and Nuka World?", note, (_, value) => Settings.DownloadAllDLCs = value.GetValueOrDefault());
            cbHDTextures = Check("Download HD Textures", cbDownloadAllDlcs, (_, value) => Settings.DownloadHDTextures = value.GetValueOrDefault());
            cbCreationKit = Check("Downgrade Creation Kit", cbHDTextures, (_, value) => Settings.DownloadCreationKit = value.GetValueOrDefault());
            cbCreationClub = Check("Delete Creation Club files", cbCreationKit, (_, value) => Settings.DeleteCreationClubFiles = value.GetValueOrDefault());
            cbKeepDepots = Check("Keep Depots when done", cbCreationClub, (_, value) => Settings.KeepDepotFilesWhenDone = value.GetValueOrDefault());

            cbLanguage = Combo("Select Language (Press arrow and use mouse scroll)", cbKeepDepots, OnLanguageChanged, languages);

            var defaultLanguage = Thread.CurrentThread.CurrentCulture.EnglishName;
            var defeaultLanguageIndex = GetLanguageIndex(defaultLanguage);
            if (defeaultLanguageIndex == -1)
            {
                defeaultLanguageIndex = 1; // english
            }

            if (ctx.Settings.DownloadAllLanguages)
            {
                cbLanguage.SelectedItem = 0;
            }
            else
            {
                SetSelectedLanguage(ctx.Settings.Language, defeaultLanguageIndex);
            }


            btnOK = Btn("Start downgrade", null, BtnOKClicked);
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
        }

        private void BtnOKClicked()
        {
            result = true;
            RequestStop();
        }
    }
}
