using Fallout4Downgrader;
using FO4Down.Core;
using FO4Down.Steam;
using FO4Down.Steam.DepotDownloader;
using SteamKit2.Authentication;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace FO4Down
{
    public class ApplicationContext
    {
        public FO4DowngraderStep Step { get; set; }
        public string Version { get; set; }
        public bool IsError { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsWarning { get; set; }
        public bool Continue { get; set; }
        public bool ReportToDeveloper { get; set; }
        public string Message { get; set; }
        public string LastErrorMessage { get; set; }
        public List<string> LoggedErrors { get; set; } = new List<string>();

        public Exception Exception { get; set; }

        public List<SteamLibFolder> LibraryFolders { get; set; }
        public Dictionary<string, SteamGame> InstalledGames { get; set; }
        public SteamGame Fallout4 { get; set; }
        public AppSettings Settings { get; internal set; }
        public Action<ApplicationContext> OnStepUpdate { get; internal set; }
        public StepRequest Request { get; set; }
        public float Fraction { get; internal set; }
        public string QRCode { get; internal set; }
        public IAuthenticator UserAuthenticator { get; set; }
        public int TotalDepotsToDownload { get; set; }
        public int DepotsDownloaded { get; set; }
        public bool DownloadCreationKit { get; internal set; }
        public bool IsAuthenticated { get; internal set; }

        private StringBuilder log = new StringBuilder();


        public Fallout4IniSettings Fallout4DefaultIni { get; set; }
        public Fallout4IniSettings Fallout4Ini { get; set; }
        public Dictionary<string, PatchInfo> Patch { get; set; } = new Dictionary<string, PatchInfo>();
        public bool CanPatch { get; set; }
        public bool IsF4SEInstalled { get; internal set; }
        public bool IsF4SEAddressLibraryInstalled { get; internal set; }
        public bool IsF4SEBASSInstalled { get; internal set; }
        public HttpClient HttpClient { get; internal set; }
        public List<Depot> Depots { get; internal set; }

        public PatchInfo Fallout4Patch
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

        public PatchInfo Fallout4LauncherPatch
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

        public PatchInfo SteamApi64Patch
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

        public CultureInfo GetTargetCultureInfo()
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

        private CultureInfo GetLanguageFromCode(string language)
        {
            try
            {
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

        public ApplicationContext()
        {
            UserAuthenticator = new UserAuthenticator(this);
        }


        public string GetWorkingDirectory()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public void Notify()
        {
            if (OnStepUpdate != null)
                this.OnStepUpdate(this);
        }

        public void Notify(string message, params object[] args)
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
                this.OnStepUpdate(this);
        }

        public void Progress(string message, float fraction)
        {
            Fraction = fraction;
            IsSuccess = false;
            if (!string.IsNullOrEmpty(message) && message != Message)
            {
                Message = message;
                log.AppendLine(message);
            }

            if (OnStepUpdate != null)
                this.OnStepUpdate(this);
        }

        public void Success(string message, params object[] args)
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
                this.OnStepUpdate(this);
        }
        public void Error(string message, params object[] args)
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
                this.OnStepUpdate(this);
        }

        public void Error(Exception exc)
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
                this.OnStepUpdate(this);
        }

        public void WarnAndReport(string message)
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
                this.OnStepUpdate(this);
        }

        public void Warn(string message)
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
                this.OnStepUpdate(this);
        }

        public void Report(Exception exc)
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
                this.OnStepUpdate(this);
        }

        //public void Next<T>(T value)
        //{
        //    var r = Request;
        //    Request = null;
        //    r.SetResult(value);
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string FormatMessage(string message, object[] args)
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

        public void Next(object value)
        {
            if (Request == null)
            {
                return;
            }

            var r = Request;
            Request = null;
            r.SetResult(value);
        }

        public Task<T> RequestAsync<T>(string name, params string[] args)
        {
            var req = new StepRequest<T>();
            req.Name = name;
            Request = req;
            Request.Arguments = args;
            Notify("Logging in to steam...");
            return req.AwaitResponseAsync();
        }

        public void SaveLog()
        {
            File.WriteAllText("log.txt", log.ToString());
        }

        internal string? RequestTwoFactorCode()
        {
            throw new NotImplementedException();
        }

        internal string? RequestEmailAuthCode()
        {
            throw new NotImplementedException();
        }

        private readonly object chunksMutex = new object();
        private Dictionary<string, List<ChunkDownloadProgress>> chunkDownloadProgress =
            new Dictionary<string, List<ChunkDownloadProgress>>();

        internal ChunkDownloadProgress ChunkDownloadProgress(string fileName, ulong offset, uint chunkSize)
        {
            lock (chunksMutex)
            {
                if (!chunkDownloadProgress.TryGetValue(fileName, out var list))
                {
                    chunkDownloadProgress[fileName] = (list = new List<ChunkDownloadProgress>());
                }
                var progress = new ChunkDownloadProgress(this)
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

        internal void ChunkDownloadProgressFinished(ChunkDownloadProgress cp)
        {
            // do we need to do something?
            cp.SetCompleted();
        }

        internal string GetAverageDownloadSpeed()
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

        internal void Merge(UserProvidedSettings userSettings)
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
