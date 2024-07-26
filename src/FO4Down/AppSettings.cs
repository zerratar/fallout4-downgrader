using FO4Down.Windows;
using SteamKit2.Internal;

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
            var folon = p.Contains("-fallout4london") || p.Contains("-folon");
            var useQrCode = p.Contains("-qr");
            var installPlugins = p.Contains("-install-plugins");
            var installHelperEnabled = p.Contains("-install-helper");
            var keepDepotFiles = p.Contains("-keep-depot");
            var patchFiles = p.Contains("-patch-files");
            var forcePatch = p.Contains("-force-patch");
            var downloadDepots = p.Contains("-download-depots");
            var downloadCreationKit = p.Contains("-creation-kit") || p.Contains("-ck");
            var creationKitOnly = p.Contains("-creation-kit-only");
            var allLanguages = p.Contains("-all-languages");
            var language = p.Get<string>("-language");
            var deleteCc = p.Contains("-delete-cc");

            // if folon, overwrite all other settings to match the requirements
            if (folon)
            {
                installPlugins = false;
                patchFiles = false;
                forcePatch = false;
                downloadCreationKit = false;
                creationKitOnly = false;
                downloadDepots = true;
                allLanguages = false;
                deleteCc = false;
            }

            return new AppSettings
            {
                DeleteCreationClubFiles = deleteCc,
                UseQrCode = useQrCode,
                InstallPlugins = installPlugins,
                InstallHelperEnabled = installHelperEnabled,
                KeepDepotFiles = keepDepotFiles,
                PatchFiles = patchFiles,
                ForcePatch = forcePatch,
                DownloadDepots = downloadDepots,
                DownloadCreationKit = downloadCreationKit,
                DowngradeCreationKitOnly = creationKitOnly,
                Username = p.Get<string>("-username") ?? p.Get<string>("-user") ?? p.Get<string>("-u"),
                Password = p.Get<string>("-password") ?? p.Get<string>("-pass") ?? p.Get<string>("-p"),
                DownloadAllLanguages = allLanguages,
                Language = language
            };
        }
    }
}
