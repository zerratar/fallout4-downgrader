namespace Fallout4Downgrader
{
    public class UserProvidedSettings
    {
        public string Language { get; set; }
        public bool DownloadCreationKit { get; set; }
        public bool DeleteCreationClubFiles { get; set; }
        public bool KeepDepotFilesWhenDone { get; set; }
        public bool DownloadHDTextures { get; set; }
        public bool DownloadAllDLCs { get; set; }
        public bool DeleteEnglishLanguageFiles { get; internal set; }
    }

    public class AppSettings
    {
        public bool UseQrCode { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Language { get; set; }
        public bool DownloadAllLanguages { get; set; }
        public bool KeepDepotFiles { get; set; }
        public bool DownloadCreationKit { get; set; }
        public bool DeleteCreationClubFiles { get; set; }
        public bool DownloadHDTextures { get; set; }
        public bool DownloadAllDLCs { get; set; }
        public bool Merged { get; internal set; }
        public bool DeleteEnglishLanguageFiles { get; internal set; }
        public bool InstallPlugins { get; set; }
        public bool InstallHelperEnabled { get; internal set; }
        public bool PatchFiles { get; private set; }
        public bool DownloadDepots { get; private set; }
        public bool ForcePatch { get; internal set; }
        public bool DowngradeCreationKitOnly { get; private set; }

        public static AppSettings FromParams(Params p)
        {
            return new AppSettings
            {
                UseQrCode = p.Contains("-qr"),
                InstallPlugins = p.Contains("-install-plugins"),
                InstallHelperEnabled = p.Contains("-install-helper"),
                KeepDepotFiles = p.Contains("-keep-depot"),
                PatchFiles = p.Contains("-patch-files"),
                ForcePatch = p.Contains("-force-patch"),
                DownloadDepots = p.Contains("-download-depots"),
                DownloadCreationKit = p.Contains("-creation-kit") || p.Contains("-ck"),
                DowngradeCreationKitOnly = p.Contains("-creation-kit-only"),
                Username = p.Get<string>("-username") ?? p.Get<string>("-user"),
                Password = p.Get<string>("-password") ?? p.Get<string>("-pass"),
                DownloadAllLanguages = p.Contains("-all-languages"),
                Language = p.Get<string>("-language")
            };
        }
    }
}
