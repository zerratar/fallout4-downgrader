using Fallout4Downgrader;
using FO4Down.Core;
using FO4Down.Steam;
using FO4Down.Steam.DepotDownloader;
using SteamKit2.Authentication;
using System.Diagnostics;
using System.Text;

namespace FO4Down
{
    public class FO4Downgrader
    {
        public async Task RunAsync(Action<DowngradeContext> onStepUpdate)
        {
            var ctx = new DowngradeContext();
            ctx.OnStepUpdate = onStepUpdate;
            ctx.Settings = LoadSettings(ctx);

            try
            {

                Steam3Session.OnDisplayQrCode += (qrCode) =>
                {
                    ctx.QRCode = qrCode;
                    ctx.Notify();
                };

                // step 1: Find out where or if Fallout 4 is installed
                var fo4 = FindFallout4(ctx);

                // step 1.5: Handle user settings
                // before we start, we should request settings changes that the user may want
                // such as language, whether or not it should try to download all dlcs and/or hd textures pack.
                await HandleUserSettingsAsync(ctx);

                // step 2: login to steam
                while (!await LoginToSteamAsync(ctx))
                {
                    ctx.Error("Login failed. Invalid username or password.\nLogin using QR if problem persists");
                }

                // step 3: Download all depot files into the /depots/ folder.
                await DownloadDepotFilesAsync(ctx);

                // step 4: Delete all next gen files before we start copying over our files from the depot, this ensures we don't delete files that should be there.
                DeleteNextGenFiles(ctx);

                // step 5: copy all depot files from /depots/ to /fallout 4/ and then delete the /depots/ folder
                CopyDepotFiles(ctx);

                // step 6: delete creation club data if needed.
                if (ctx.DeleteCreationClubData)
                {
                    DeleteCreationClubData(ctx);
                }

                ctx.Step = FO4DowngraderStep.Finished;
                ctx.Notify("Your Fallout 4 installation has been downgraded. Enjoy!\nYou may now close this application.");

            }
            catch (FileNotFoundException ex)
            {
                ctx.Error(ex);
            }
            catch (Exception ex)
            {
                ctx.Report(ex);
            }
            finally
            {
                ctx.SaveLog();
            }
        }

        private async Task HandleUserSettingsAsync(DowngradeContext ctx)
        {
            ctx.Step = FO4DowngraderStep.UserSettings;
            var userSettings = await ctx.RequestAsync<UserProvidedSettings>("settings");
            if (userSettings != null)
            {
                ctx.Merge(userSettings);
            }
        }

        private static void CopyDepotFiles(DowngradeContext ctx)
        {
            ctx.Step = FO4DowngraderStep.CopyDepotFiles;

            AppSettings settings = ctx.Settings;
            SteamGame fo4 = ctx.Fallout4;

            var deleteAfterCopy = !settings.KeepDepotFiles && !Debugger.IsAttached;//Console.ReadKey().Key == ConsoleKey.Y;
            var depotFolders = System.IO.Directory
                .GetDirectories("depots")
                .Where(x => !new DirectoryInfo(x).Name.StartsWith("."))
                .ToArray();

            foreach (var depotFolder in depotFolders)
            {
                // will contain subfolder
                foreach (var folder in Directory.GetDirectories(depotFolder, "*"))
                    CopyFiles(ctx, folder, fo4.Path);

                if (deleteAfterCopy)
                {
                    try
                    {
                        Directory.Delete(depotFolder, true);
                    }
                    catch { }
                }
            }

            if (deleteAfterCopy)
            {
                try
                {
                    Directory.Delete("depots", true);
                }
                catch { }
            }
        }

        private static void CopyFiles(DowngradeContext ctx, string srcDirectory, string dstDirectory)
        {
            int copyCount = 0;
            var fileCount = 0;
            foreach (var file in Directory.GetFiles(srcDirectory, "*.*", SearchOption.AllDirectories))
            {
                if (file.IndexOf(".DepotDownloader") != -1)
                {
                    continue;
                }

                fileCount++;
                // must match path relative to srcDirectory and dstDirectory
                var newPath = file.Replace(srcDirectory, dstDirectory);
                if (newPath == file)
                {
                    ctx.Error("Failed determine destination for file: " + file);
                    continue;
                }

                var dir = Path.GetDirectoryName(newPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                ctx.Progress("Copying: " + file.Replace(srcDirectory, ""), copyCount / (float)fileCount);
                File.Copy(file, newPath, true);
                copyCount++;
            }

            ctx.Progress("All files copied.", 1);
        }

        private static void DeleteCreationClubData(DowngradeContext ctx)
        {
            ctx.Step = FO4DowngraderStep.DeleteCreationClubData;
            var fo4 = ctx.Fallout4;
            var deleteCreationClubData = ctx.DeleteCreationClubData;
            var dataFolder = System.IO.Path.Combine(fo4.Path, "Data");
            var filesToDelete = new List<string>
            {
                "ccBGSFO4006-PipBoy(Chrome) - Main.ba2",
                "ccFSVFO4002-MidCenturyModern - Textures.ba2",
                "ccFRSFO4001-HandmadeShotgun - Textures.ba2",
                "ccFSVFO4002-MidCenturyModern - Main.ba2",
                "ccFSVFO4001-ModularMilitaryBackpack - Textures.ba2",
                "ccFRSFO4001-HandmadeShotgun - Main.ba2",
                "ccFSVFO4001-ModularMilitaryBackpack - Main.ba2",
                "ccBGSFO4038-HorseArmor - Textures.ba2",
                "ccBGSFO4020-PowerArmorSkin(Black) - Main.ba2",
                "ccBGSFO4038-HorseArmor - Main.ba2",
                "ccBGSFO4018-GaussRiflePrototype - Textures.ba2",
                "ccBGSFO4019-ChineseStealthArmor - Textures.ba2",
                "ccBGSFO4020-PowerArmorSkin(Black) - Textures.ba2",
                "ccBGSFO4016-Prey - Textures.ba2",
                "ccBGSFO4018-GaussRiflePrototype - Main.ba2",
                "ccBGSFO4019-ChineseStealthArmor - Main.ba2",
                "ccBGSFO4001-PipBoy(Black) - Main.ba2",
                "ccBGSFO4001-PipBoy(Black) - Textures.ba2",
                "ccBGSFO4003-PipBoy(Camo01) - Main.ba2",
                "ccBGSFO4003-PipBoy(Camo01) - Textures.ba2",
                "ccBGSFO4004-PipBoy(Camo02) - Main.ba2",
                "ccBGSFO4004-PipBoy(Camo02) - Textures.ba2",
                "ccBGSFO4006-PipBoy(Chrome) - Textures.ba2",
                "ccBGSFO4016-Prey - Main.ba2"
            };

            ctx.Notify("Deleting Creation Club Content Files...");
            var paths = filesToDelete.Select(x => Path.Combine(dataFolder, x)).ToArray();
            var existingFiles = paths.Where(x => File.Exists(x)).ToArray();

            for (int i = 0; i < existingFiles.Length; i++)
            {
                string? file = existingFiles[i];
                ctx.Progress("Deleting " + Path.GetFileName(file) + " (" + (i + 1) + "/" + existingFiles.Length + ")", (i / (float)existingFiles.Length));
                File.Delete(file);
            }
            ctx.Progress("All Creation Club files deleted.", 1f);
        }

        private static void DeleteNextGenFiles(DowngradeContext ctx)
        {
            ctx.Step = FO4DowngraderStep.DeleteNextGenFiles;
            var fo4 = ctx.Fallout4;
            var downloadCreationKit = ctx.DownloadCreationKit;
            var dataFolder = System.IO.Path.Combine(fo4.Path, "Data");
            // finally delete following files from the fallout 4 folder:
            var filesToDelete = new List<string>
                {
                    "ccBGSFO4044-HellfirePowerArmor - Main.ba2",
                    "ccBGSFO4044-HellfirePowerArmor - Textures.ba2",
                    "ccBGSFO4044-HellfirePowerArmor.esl",
                    "ccBGSFO4046-TesCan - Main.ba2",
                    "ccBGSFO4046-TesCan - Textures.ba2",
                    "ccBGSFO4046-TesCan.esl",
                    "ccBGSFO4096-AS_Enclave - Main.ba2",
                    "ccBGSFO4096-AS_Enclave - Textures.ba2",
                    "ccBGSFO4096-AS_Enclave.esl",
                    "ccBGSFO4110-WS_Enclave - Main.ba2",
                    "ccBGSFO4110-WS_Enclave - Textures.ba2",
                    "ccBGSFO4110-WS_Enclave.esl",
                    "ccBGSFO4115-X02 - Main.ba2",
                    "ccBGSFO4115-X02 - Textures.ba2",
                    "ccBGSFO4115-X02.esl",
                    "ccBGSFO4116-HeavyFlamer - Main.ba2",
                    "ccBGSFO4116-HeavyFlamer - Textures.ba2",
                    "ccBGSFO4116-HeavyFlamer.esl",
                    "ccFSVFO4007-Halloween - Main.ba2",
                    "ccFSVFO4007-Halloween - Textures.ba2",
                    "ccFSVFO4007-Halloween.esl",
                    "ccOTMFO4001-Remnants - Main.ba2",
                    "ccOTMFO4001-Remnants - Textures.ba2",
                    "ccOTMFO4001-Remnants.esl",
                    "ccSBJFO4003-Grenade - Main.ba2",
                    "ccSBJFO4003-Grenade - Textures.ba2",
                    "ccSBJFO4003-Grenade.esl",
                };

            ctx.Notify("Deleting Next-Gen Content Files...");
            var paths = filesToDelete.Select(x => Path.Combine(dataFolder, x)).ToArray();
            var existingFiles = paths.Where(x => File.Exists(x)).ToArray();

            for (int i = 0; i < existingFiles.Length; i++)
            {
                string? file = existingFiles[i];
                ctx.Progress("Deleting " + Path.GetFileName(file) + " (" + (i + 1) + "/" + existingFiles.Length + ")", (i / (float)existingFiles.Length));
                File.Delete(file);
            }
            ctx.Progress("All Next-Gen Content Files deleted.", 1f);
        }


        private static async Task DownloadDepotFilesAsync(DowngradeContext ctx)
        {
            var settings = ctx.Settings;
            var downloadCreationKit = ctx.DownloadCreationKit;

            List<Depot> depots = new List<Depot>();

            depots.AddRange(DepotManager.GetLanguageNeutral(DepotTarget.Game));
            depots.AddRange(DepotManager.GetLanguageNeutral(DepotTarget.RequiredDlc));

            var language = settings.Language.ToLower();
            if (!settings.DownloadAllLanguages && string.IsNullOrEmpty(settings.Language))
                language = "english";
            if (settings.DownloadAllLanguages)
                language = null;

            depots.AddRange(DepotManager.Get(DepotTarget.Game, language));
            depots.AddRange(DepotManager.Get(DepotTarget.RequiredDlc, language));

            if (ctx.DownloadAllDLCs)
            {
                depots.AddRange(DepotManager.GetLanguageNeutral(DepotTarget.AllDlc));
                depots.AddRange(DepotManager.Get(DepotTarget.AllDlc, language));
            }

            if (ctx.DownloadHDTextures)
            {
                // <HD Textures>
                depots.Add(DepotManager.GetHDTextures());
            }

            var totalDepotsToDownload = depots.Count + (downloadCreationKit ? 2 : 0);

            ContentDownloader.Config.MaxDownloads = depots.Count;

            uint f4AppId = 377160;
            uint ckAppId = 1946160;

            //ctx.Notify("Downloading ")
            ctx.TotalDepotsToDownload = totalDepotsToDownload;
            ctx.Step = FO4DowngraderStep.DownloadGameDepotFiles;
            await ContentDownloader.DownloadAppAsync(
                f4AppId, depots.Select(x => x.AsTuple()).ToList(), "public", "windows", "64",
                settings.Language,
                false, false, ctx);

            // check if creation kit is available, if so, download and replace those as well.

            if (downloadCreationKit)
            {
                ctx.Step = FO4DowngraderStep.DownloadCreationKitDepotFiles;
                depots.Clear();
                depots.AddRange(DepotManager.GetCreationKit());
                await ContentDownloader.DownloadAppAsync(
                    ckAppId, depots.Select(x => x.AsTuple()).ToList(),
                    "public", "windows", "64",
                    settings.Language, false, false, ctx);
            }

            ctx.Notify();
        }


        private static SteamGame FindFallout4(DowngradeContext ctx)
        {
            ctx.Step = FO4DowngraderStep.LookingForFallout4Path;
            var steamPath = SteamGameLocator.GetSteamInstallPath();
            if (string.IsNullOrEmpty(steamPath))
            {
                throw new FileNotFoundException("Steam is not installed.");
            }

            //steamPath = "G:\\GitHub\\fallout4-downgrader\\publish\\Self-contained\\libraryfolders.vdf";
            ctx.LibraryFolders = SteamGameLocator.GetLibraryFolders(steamPath);
            ctx.InstalledGames = SteamGameLocator.GetInstalledGames(ctx.LibraryFolders);

            if (!ctx.InstalledGames.TryGetValue("Fallout 4", out var fo4))
            {
                throw new FileNotFoundException("Fallout 4 is not installed.");
            }

            ctx.Fallout4 = fo4;
            ctx.DownloadCreationKit = Directory.Exists(Path.Combine(fo4.Path, "Tools"))
                    || Directory.Exists(Path.Combine(fo4.Path, "Papyrus Compiler"))
                    || ctx.Settings.DownloadCreationKit;

            ctx.Notify();

            return fo4;
        }


        private static async Task<bool> LoginToSteamAsync(DowngradeContext ctx)
        {
            ctx.Step = FO4DowngraderStep.LoginToSteam;

            if (!AccountSettingsStore.Loaded)
                AccountSettingsStore.LoadFromFile("account.config");

            var settings = ctx.Settings;
            string username = null;
            string password = null;

            if (!string.IsNullOrEmpty(settings.Username))
            {
                username = settings.Username;
            }

            if (!string.IsNullOrEmpty(settings.Password))
            {
                password = settings.Password;
            }

            if (!settings.UseQrCode)
            {
                var noUser = string.IsNullOrEmpty(username);
                var noPass = string.IsNullOrEmpty(password);

                if (noUser || noPass)
                {
                    (username, password) = await ctx.RequestAsync<(string, string)>("credentials");

                    if (ctx.Settings.UseQrCode)
                    {
                        ContentDownloader.Config.UseQrCode = true;
                    }
                }
            }

            var result = ContentDownloader.InitializeSteam3(username, password, ctx);
            if (result)
            {
                ctx.Notify();
            }

            return result;
        }

        private static AppSettings LoadSettings(DowngradeContext ctx)
        {
            var p = Params.FromArgs(Environment.GetCommandLineArgs());
            var settings = AppSettings.FromParams(p);
            var c = ContentDownloader.Config;
            // c.Logger = new ConsoleLogger();
            c.Logger = new DelegateLogger((severity, format, args) =>
            {
                switch (severity)
                {
                    case LogSeverity.Error:
                        ctx.Error(format, args);
                        break;

                    case LogSeverity.Debug:
                    case LogSeverity.Information:
                    case LogSeverity.Warning:
                        ctx.Notify(format, args);
                        break;
                }
            });
            c.UseQrCode = settings.UseQrCode;
            c.DownloadAllLanguages = settings.DownloadAllLanguages;
            return settings;
        }
    }

    public enum FO4DowngraderStep
    {
        LookingForFallout4Path,
        UserSettings,
        LoginToSteam,
        DownloadDepotFiles,
        DownloadGameDepotFiles,
        DownloadCreationKitDepotFiles,
        DeleteNextGenFiles,
        CopyDepotFiles,
        Finished,
        DeleteCreationClubData,
    }

    public class DowngradeContext
    {
        public FO4DowngraderStep Step { get; set; }
        public bool IsError { get; set; }
        public bool Continue { get; set; }
        public bool ReportToDeveloper { get; set; }
        public string Message { get; set; }
        public string LastErrorMessage { get; set; }
        public Exception Exception { get; set; }

        public List<SteamLibFolder> LibraryFolders { get; set; }
        public Dictionary<string, SteamGame> InstalledGames { get; set; }
        public SteamGame Fallout4 { get; set; }
        public AppSettings Settings { get; internal set; }
        public bool DownloadCreationKit { get; set; }
        public bool DeleteCreationClubData { get; set; }
        public bool DownloadAllDLCs { get; set; }
        public bool DownloadHDTextures { get; set; }
        public Action<DowngradeContext> OnStepUpdate { get; internal set; }
        public StepRequest Request { get; set; }
        public float Fraction { get; internal set; }
        public string QRCode { get; internal set; }
        public IAuthenticator UserAuthenticator { get; set; }
        public int TotalDepotsToDownload { get; set; }
        public int DepotsDownloaded { get; set; }

        private StringBuilder log = new StringBuilder();

        public DowngradeContext()
        {
            UserAuthenticator = new UserAuthenticator(this);
        }

        public void Notify()
        {
            if (OnStepUpdate != null)
                this.OnStepUpdate(this);
        }

        public void Notify(string message, params object[] args)
        {
            var msg = args != null && args.Length > 0 ? string.Format(message, args) : message;
            if (msg != Message)
            {
                log.AppendLine(msg);
            }
            Message = msg;
            if (OnStepUpdate != null)
                this.OnStepUpdate(this);
        }

        public void Progress(string message, float fraction)
        {
            Fraction = fraction;

            if (!string.IsNullOrEmpty(message) && message != Message)
            {
                Message = message;
                log.AppendLine(message);
            }

            if (OnStepUpdate != null)
                this.OnStepUpdate(this);
        }


        public void Error(string message, params object[] args)
        {
            var msg = args != null && args.Length > 0 ? string.Format(message, args) : message; ;
            IsError = true;
            Continue = true;
            ReportToDeveloper = false;

            if (msg != message)
            {
                log.AppendLine(message);
            }

            Message = msg;
            LastErrorMessage = message;

            if (OnStepUpdate != null)
                this.OnStepUpdate(this);
        }

        public void Error(Exception exc)
        {
            IsError = true;
            Continue = false;
            Message = exc.Message;
            Exception = exc;
            LastErrorMessage = Message;
            log.AppendLine(exc.ToString());
            if (OnStepUpdate != null)
                this.OnStepUpdate(this);
        }

        public void Report(Exception exc)
        {
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

        public void Next(object value)
        {
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
            this.DownloadAllDLCs = userSettings.DownloadAllDLCs;
            this.DownloadHDTextures = userSettings.DownloadHDTextures;
            ContentDownloader.Config.DownloadAllLanguages = Settings.DownloadAllLanguages;
        }
    }

    public class ChunkDownloadProgress
    {
        private DowngradeContext stepContext;
        private DateTime startTime;

        public ChunkDownloadProgress(DowngradeContext stepContext)
        {
            this.stepContext = stepContext;
            this.startTime = DateTime.UtcNow;
        }

        public ulong Offset { get; internal set; }
        public uint ChunkSize { get; internal set; }
        public double Fraction { get; internal set; }
        public long BytesDownloaded { get; set; }
        public long ActualLength { get; set; }
        public string FileName { get; internal set; }
        public double KiloBytesPerSecond { get; set; }
        public DateTime CompletedTime { get; private set; }
        public bool Completed { get; private set; }
        internal void Progress(long totalRead, long totalLength, double v)
        {
            BytesDownloaded = totalRead;
            ActualLength = totalLength;
            Fraction = v;

            CalculateDownloadSpeed();
        }

        internal void SetCompleted()
        {
            CompletedTime = DateTime.UtcNow;
            Completed = true;
        }

        private void CalculateDownloadSpeed()
        {
            var currentTime = DateTime.UtcNow;
            var timeSpan = currentTime - startTime;
            if (timeSpan.TotalSeconds > 0) // Ensure we do not divide by zero
            {
                KiloBytesPerSecond = (BytesDownloaded / 1024.0) / timeSpan.TotalSeconds;
            }
        }
    }

    public abstract class StepRequest
    {
        public string Name { get; set; }
        public string[] Arguments { get; internal set; }

        protected object Value;
        protected TaskCompletionSource<object> src;

        public void SetResult(object value)
        {
            this.Value = value;
            src.SetResult(value);
        }

        protected StepRequest()
        {
            this.src = new TaskCompletionSource<object>();
        }
    }

    public class StepRequest<T> : StepRequest
    {
        public async Task<T> AwaitResponseAsync()
        {
            var result = await src.Task;
            return (T)result;
        }
    }

    public class UserAuthenticator : IAuthenticator
    {
        private readonly DowngradeContext ctx;

        public UserAuthenticator(DowngradeContext context)
        {
            this.ctx = context;
        }

        /// <inheritdoc />
        public async Task<string> GetDeviceCodeAsync(bool previousCodeWasIncorrect)
        {
            if (previousCodeWasIncorrect)
            {
                ctx.Error("The previous 2-factor auth code you have provided is incorrect.");
            }

            string? code;

            do
            {
                ctx.Error("STEAM GUARD! Please enter your 2-factor auth code from your authenticator app");

                code = await ctx.RequestAsync<string>("auth_code", "Please enter your 2-factor auth code from your authenticator app");

                if (code == null)
                {
                    break;
                }
                code = code.Trim();
            }
            while (string.IsNullOrEmpty(code));

            return code!;
        }

        /// <inheritdoc />
        public async Task<string> GetEmailCodeAsync(string email, bool previousCodeWasIncorrect)
        {
            if (previousCodeWasIncorrect)
            {
                ctx.Error("The previous 2-factor auth code you have provided is incorrect.");
            }

            string? code;

            do
            {
                ctx.Error($"STEAM GUARD! Please enter the auth code sent to the email at {email}");

                code = await ctx.RequestAsync<string>("auth_code", $"Please enter the auth code sent to the email at {email}");

                if (code == null)
                {
                    break;
                }

                code = code.Trim();
            }
            while (string.IsNullOrEmpty(code));

            return code!;
        }

        /// <inheritdoc />
        public Task<bool> AcceptDeviceConfirmationAsync()
        {
            ctx.Error("STEAM GUARD! Use the Steam Mobile App to confirm your sign in...");

            return Task.FromResult(true);
        }
    }
}
