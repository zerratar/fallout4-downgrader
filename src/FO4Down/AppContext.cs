using Fallout4Downgrader;
using FO4Down.Core;
using FO4Down.Steam;
using DepotDownloader;
using SteamKit2.Authentication;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace FO4Down
{
    public static class AppContext
    {
        private static StringBuilder log = new StringBuilder();

        public static FO4DowngraderStep Step { get; set; }
        public static string Version { get; set; }
        public static bool IsError { get; set; }
        public static bool IsSuccess { get; set; }
        public static bool IsWarning { get; set; }
        public static bool Continue { get; set; }
        public static bool ReportToDeveloper { get; set; }
        public static string Message { get; set; }
        public static string LastErrorMessage { get; set; }
        public static List<string> LoggedErrors { get; set; } = new List<string>();
        public static Exception Exception { get; set; }
        public static List<SteamLibFolder> LibraryFolders { get; set; }
        public static Dictionary<string, SteamGame> InstalledGames { get; set; }
        public static SteamGame Fallout4 { get; set; }
        public static AppSettings Settings { get; set; }
        public static Action OnStepUpdate { get; set; }
        public static StepRequest Request { get; set; }
        public static float Fraction { get; set; }
        public static string QRCode { get; set; }
        public static IAuthenticator UserAuthenticator { get; set; }
        public static int TotalDepotsToDownload { get; set; }
        public static int DepotsDownloaded { get; set; }
        public static bool DownloadCreationKit { get; set; }
        public static bool IsAuthenticated { get; set; }
        public static Fallout4IniSettings Fallout4DefaultIni { get; set; }
        public static Fallout4IniSettings Fallout4Ini { get; set; }
        public static Dictionary<string, PatchInfo> Patch { get; set; } = new Dictionary<string, PatchInfo>();
        public static bool CanPatch { get; set; }
        public static bool IsF4SEInstalled { get; set; }
        public static bool IsF4SEAddressLibraryInstalled { get; set; }
        public static bool IsF4SEBASSInstalled { get; set; }
        public static HttpClient HttpClient { get; set; }
        public static List<Depot> Depots { get; set; }

        public static PatchInfo Fallout4Patch
        {
            get
            {
                if (Patch.TryGetValue("Fallout4.exe", out var p)) return p;
                return null;
            }
            set
            {
                Patch["Fallout4.exe"] = value;
            }
        }

        public static PatchInfo Fallout4LauncherPatch
        {
            get
            {
                if (Patch.TryGetValue("Fallout4Launcher.exe", out var p)) return p;
                return null;
            }
            set
            {
                Patch["Fallout4Launcher.exe"] = value;
            }
        }

        public static PatchInfo SteamApi64Patch
        {
            get
            {
                if (Patch.TryGetValue("steam_api64.dll", out var p)) return p;
                return null;
            }
            set
            {
                Patch["steam_api64.dll"] = value;
            }
        }

        public static CultureInfo GetTargetCultureInfo()
        {
            var l = Settings.Language;
            if (!string.IsNullOrEmpty(l))
            {
                return GetLanguageFromCode(l);
            }

            var ini = Fallout4Ini ?? Fallout4DefaultIni;
            if (ini == null)
            {
                return Thread.CurrentThread.CurrentCulture;
            }

            return GetLanguageFromCode(ini["General"]["sLanguage"]);
        }

        private static CultureInfo GetLanguageFromCode(string language)
        {
            try
            {
                switch (language.ToLower())
                {
                    case "en":
                    case "english": return new CultureInfo("en-US");
                    case "de":
                    case "german": return new CultureInfo("de-DE");
                    case "fr":
                    case "french": return new CultureInfo("fr-FR");
                    case "es":
                    case "spanish": return new CultureInfo("es-ES");
                    case "pt":
                    case "portuguese": return new CultureInfo("pt-PT");
                    case "it":
                    case "italian": return new CultureInfo("it-IT");
                    case "ru":
                    case "russian": return new CultureInfo("ru-RU");
                    case "pl":
                    case "polish": return new CultureInfo("pl-PL");
                    case "jp":
                    case "japanese": return new CultureInfo("ja-JP");
                    case "ko":
                    case "korean": return new CultureInfo("ko-KR");
                    case "zh":
                    case "chinese": return new CultureInfo("zh-CN");
                    case "chinese traditional": return new CultureInfo("zh-TW");
                }
                return new CultureInfo(language);
            }
            catch (CultureNotFoundException)
            {

                var ci = CultureInfo.GetCultures(CultureTypes.AllCultures)
                                .FirstOrDefault(r => r.EnglishName.Equals(language, StringComparison.OrdinalIgnoreCase));
                if (ci != null) return ci;
                // Log the error or handle it appropriately if the language code is invalid
                Warn($"Warning: The provided language code '{language}' is not valid.");
                return CultureInfo.InvariantCulture; // Return invariant culture or a default culture
            }
        }

        static AppContext()
        {
            UserAuthenticator = new UserAuthenticator();
        }


        public static string GetWorkingDirectory()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static void Notify()
        {
            if (OnStepUpdate != null)
                OnStepUpdate();
        }

        public static void WriteLine(string message, params object[] args)
        {
            Notify(message, args);
        }

        public static void Notify(string message, params object[] args)
        {
            try
            {
                IsSuccess = false;
                string msg = FormatMessage(message, args);
                if (msg != Message)
                {
                    log.AppendLine(msg);
                }
                Message = msg;
            }
            catch (Exception exc)
            {
                Error(exc.ToString());
                return;
            }
            if (OnStepUpdate != null)
                OnStepUpdate();
        }

        public static void Progress(string message, float fraction)
        {
            Fraction = fraction;
            IsSuccess = false;
            if (!string.IsNullOrEmpty(message) && message != Message)
            {
                Message = message;
                log.AppendLine(message);
            }

            if (OnStepUpdate != null)
                OnStepUpdate();
        }

        public static void Success(string message, params object[] args)
        {
            var msg = FormatMessage(message, args);
            IsSuccess = true;
            IsError = false;
            Continue = true;
            ReportToDeveloper = false;

            if (msg != message)
            {
                log.AppendLine(msg);
            }

            Message = msg;

            if (OnStepUpdate != null)
                OnStepUpdate();
        }
        public static void Error(string message, params object[] args)
        {
            var msg = FormatMessage(message, args);
            IsSuccess = false;
            IsError = true;
            Continue = true;
            ReportToDeveloper = false;

            if (msg != message)
            {
                log.AppendLine(msg);
            }

            Message = msg;
            LastErrorMessage = msg;
            LoggedErrors.Add(msg);
            if (OnStepUpdate != null)
                OnStepUpdate();
        }

        public static void Error(Exception exc)
        {
            IsSuccess = false;
            IsError = true;
            Continue = false;
            Message = exc.Message;
            Exception = exc;
            LastErrorMessage = Message;
            LoggedErrors.Add(exc.ToString());
            log.AppendLine(exc.ToString());
            if (OnStepUpdate != null)
                OnStepUpdate();
        }

        public static void WarnAndReport(string message)
        {
            IsSuccess = false;
            IsError = false;
            IsWarning = true;
            Continue = false;
            ReportToDeveloper = true;
            Message = message;
            Exception = null;
            LastErrorMessage = message;
            log.AppendLine(message);
            if (OnStepUpdate != null)
                OnStepUpdate();
        }

        public static void Warn(string message)
        {
            IsSuccess = false;
            IsError = false;
            IsWarning = true;
            Continue = true;
            ReportToDeveloper = false;
            Message = message;
            Exception = null;
            log.AppendLine(message);
            if (OnStepUpdate != null)
                OnStepUpdate();
        }

        public static void Report(Exception exc)
        {
            IsSuccess = false;
            IsError = true;
            Continue = false;
            ReportToDeveloper = true;
            Message = exc.Message;
            Exception = exc;
            LastErrorMessage = Message;
            log.AppendLine(exc.ToString());
            if (OnStepUpdate != null)
                OnStepUpdate();
        }

        //public void Next<T>(T value)
        //{
        //    var r = Request;
        //    Request = null;
        //    r.SetResult(value);
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatMessage(string message, object[] args)
        {
            try
            {
                return args != null && args.Length > 0 && !string.IsNullOrEmpty(message) ? string.Format(message, args) : message;
            }
            catch (Exception exc)
            {
                log.AppendLine("Error formatting message (this can be ignored):\n" + exc.ToString());
            }
            return message;
        }

        public static void Next(object value)
        {
            if (Request == null)
            {
                return;
            }

            var r = Request;
            Request = null;
            r.SetResult(value);
        }

        public static Task<T> RequestAsync<T>(string name, params string[] args)
        {
            var req = new StepRequest<T>();
            req.Name = name;
            Request = req;
            Request.Arguments = args;
            Notify("Logging in to steam...");
            return req.AwaitResponseAsync();
        }

        public static void SaveLog()
        {
            File.WriteAllText("log.txt", log.ToString());
        }

        internal static string? RequestTwoFactorCode()
        {
            throw new NotImplementedException();
        }

        internal static string? RequestEmailAuthCode()
        {
            throw new NotImplementedException();
        }

        private static readonly object chunksMutex = new object();
        private static Dictionary<string, List<ChunkDownloadProgress>> chunkDownloadProgress =
            new Dictionary<string, List<ChunkDownloadProgress>>();

        internal static ChunkDownloadProgress ChunkDownloadProgress(string fileName, ulong offset, uint chunkSize)
        {
            lock (chunksMutex)
            {
                if (!chunkDownloadProgress.TryGetValue(fileName, out var list))
                {
                    chunkDownloadProgress[fileName] = (list = new List<ChunkDownloadProgress>());
                }
                var progress = new ChunkDownloadProgress()
                {
                    FileName = fileName,
                    Offset = offset,
                    ChunkSize = chunkSize,
                    Fraction = 0d,
                };
                list.Add(progress);
                return progress;
            }
        }

        internal static void ChunkDownloadProgressFinished(ChunkDownloadProgress cp)
        {
            // do we need to do something?
            cp.SetCompleted();
        }

        internal static string GetAverageDownloadSpeed()
        {
            var totalDownloadSpeed = 0d;
            var items = 0;
            lock (chunksMutex)
            {
                var lists = chunkDownloadProgress.Values.ToArray();
                foreach (var chunk in lists)
                {
                    if (chunk == null) continue;
                    foreach (var cd in chunk)
                    {
                        if (cd == null) continue;
                        if (!cd.Completed)
                        {
                            items++;
                            totalDownloadSpeed += cd.KiloBytesPerSecond;
                        }
                    }
                }

                var avg = totalDownloadSpeed / items;
                if (avg > 0)
                {
                    if (avg > 1000)
                    {
                        var mbs = avg / 1000;
                        return $"{mbs:00.00} Mb/s";
                    }

                    return $"{avg:00.00} Kb/s";
                }

                return "";
            }
        }

        internal static void Merge(UserProvidedSettings userSettings)
        {
            if (userSettings == null) return;
            Settings.KeepDepotFiles = userSettings.KeepDepotFilesWhenDone;
            Settings.DownloadAllLanguages = string.IsNullOrEmpty(userSettings.Language);
            Settings.Language = userSettings.Language;
            Settings.DownloadCreationKit = userSettings.DownloadCreationKit;
            Settings.DownloadHDTextures = userSettings.DownloadHDTextures;
            Settings.DownloadAllDLCs = userSettings.DownloadAllDLCs;
            Settings.DeleteCreationClubFiles = userSettings.DeleteCreationClubFiles;
            Settings.DeleteEnglishLanguageFiles = userSettings.DeleteEnglishLanguageFiles;
            Settings.Merged = true;
            ContentDownloader.Config.DownloadAllLanguages = Settings.DownloadAllLanguages;
        }
    }

    public class PatchInfo
    {
        public ulong Id { get; set; }
        public string Target { get; set; }
        public string[] Files { get; set; }
        public byte[] Hash { get; set; }
        public FileVersionInfo Version { get; set; }
        public bool IsPatched { get; set; }
    }
}
