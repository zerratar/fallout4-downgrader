using FO4Down;
using FO4Down.Core;
using FO4Down.Steam;
using FO4Down.Steam.DepotDownloader;
using FO4Down.Windows;
using System.Reflection;
using Terminal.Gui;

namespace Fallout4Downgrader
{
    internal class Program
    {
        public static string GetTitle()
        {
            return "Fallout 4 Downgrader";
        }

        static async Task Main(string[] args)
        {

            //args = ["-qr"];
            var settings = LoadSettingsFromArgs(args);

            if (args.Contains("-help") || args.Contains("-h") || args.Contains("/?") || args.Contains("-?"))
            {
                Console.WriteLine("Fallout 4 Downgrader - v" + Assembly.GetExecutingAssembly().GetName().Version);
                Console.WriteLine("Usage: FO4Down.exe (optional: arguments)");
                Console.WriteLine("  -user or -username <steam user>");
                Console.WriteLine("  -pass or -password <steam pass>");
                Console.WriteLine("  -qr\t\t\tLogin using QR code instead");
                Console.WriteLine("  -ck or -creation-kit\tForce downgrade the creation kit as well, this will automatically happen if you have creation kit already installed");
                Console.WriteLine("  -all-languages\tDownload all languages");
                Console.WriteLine("  -language <language>\tDownloads a specific language, default is english");
                Console.WriteLine("  -keep-depot\t\tKeep the downloaded depot files after finish");
                Console.WriteLine("  -help\t\t\tThis text");
                Console.ReadKey();
                return;
            }

            Application.Run<MainWindow>();
            Application.Shutdown();
            return;

            // setup qr code hook
            Steam3Session.OnDisplayQrCode += (qrCode) =>
            {
                Console.WriteLine("Scan the QR code with your Steam mobile app to login.");
                Console.WriteLine(qrCode);
            };

            try
            {
                Console.ResetColor();
                Console.Title = "Fallout 4 Downgrader - Lets go back in time!";
                Console.WriteLine("Fallout 4 Downgrader - v" + Assembly.GetExecutingAssembly().GetName().Version);
                Console.WriteLine("Contact: zerratar@gmail.com");
                Console.WriteLine("Source: https://www.github.com/zerratar/fallout4-downgrader");
                Console.WriteLine("Using DepotDownloader for downloading depots.");
                Console.WriteLine("Ref: https://github.com/SteamRE/DepotDownloader");
                Console.WriteLine();

                // step 1: Find out where or if Fallout 4 is installed
                var fo4 = FindFallout4();

                // Do we need to downgrade the creation kit?
                var downloadCreationKit = Directory.Exists(Path.Combine(fo4.Path, "Tools"))
                    || Directory.Exists(Path.Combine(fo4.Path, "Papyrus Compiler"))
                    || settings.DownloadCreationKit;

                // step 2: Login to steam, init the connection
                LoginToSteam(settings);

                // step 3: Download all depot files into the /depots/ folder.
                await DownloadDepotFilesAsync(settings, downloadCreationKit);

                // step 4: Delete all next gen files before we start copying over our files from the depot, this ensures we don't delete files that should be there.
                DeleteNextGenFiles(fo4, downloadCreationKit);

                // step 5: copy all depot files from /depots/ to /fallout 4/ and then delete the /depots/ folder
                CopyDepotFiles(settings, fo4);

                // step 6: All done, exit the application
                Console.Title = GetTitle() + " - Finished";
                Console.WriteLine("Downgrade complete! Application will exit in 30 seconds. It's safe to close it now.");
                Thread.Sleep(30_000);
                Environment.Exit(0);
                return;
            }
            catch (Exception exc)
            {
                Console.WriteLine($"An error occurred when running the application.");
                Console.WriteLine("Please report the following error to Zerratar on discord or nexusmods");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exc);
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("A copy of this error has also been saved to error.txt, you can press any button to exit.");
                File.WriteAllText("error.txt", exc.ToString());
                Console.ReadKey();
            }
        }

        private static SteamGame FindFallout4()
        {
            var steamPath = SteamGameLocator.GetSteamInstallPath();
            if (string.IsNullOrEmpty(steamPath))
            {
                throw new FileNotFoundException("Steam is not installed.");
            }

            //steamPath = "G:\\GitHub\\fallout4-downgrader\\publish\\Self-contained\\libraryfolders.vdf";
            var libraryFolders = SteamGameLocator.GetLibraryFolders(steamPath);
            var installedGames = SteamGameLocator.GetInstalledGames(libraryFolders);

            Console.WriteLine("Steam is installed at " + steamPath);
            Console.WriteLine();
            Console.WriteLine("Library Folders:");
            foreach (var lib in libraryFolders)
            {
                Console.WriteLine("  * " + lib.Path);
            }

            Console.WriteLine();
            Console.WriteLine("Installed Games:");
            foreach (var game in installedGames)
            {
                Console.WriteLine("  * " + game.Key);
                Console.WriteLine("    - " + game.Value.Path);
            }

            Console.WriteLine("------");
            Console.WriteLine();

            if (!installedGames.TryGetValue("Fallout 4", out var fo4))
            {
                throw new FileNotFoundException("Fallout 4 is not installed.");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Found Fallout 4 at " + fo4.Path);
            Console.ResetColor();
            return fo4;
        }

        private static void LoginToSteam(AppSettings settings)
        {
            var ctx = new DowngradeContext();
            AccountSettingsStore.LoadFromFile("account.config");

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
            for (; ; )
            {
                if (!settings.UseQrCode)
                {
                    var noUser = string.IsNullOrEmpty(username);
                    var noPass = string.IsNullOrEmpty(password);

                    if (noUser || noPass)
                    {
                        Console.WriteLine("Please login using your credentials");

                        if (noUser)
                        {
                            Console.Write("Username: ");
                            username = Console.ReadLine();
                        }

                        if (noPass)
                        {
                            Console.Write("Password: ");
                            password = Console.ReadLine();
                        }
                    }

                    if (!InitializeSteam(username, password, ctx))
                    {
                        username = null;
                        password = null;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Login to steam failed. Press ESC to cancel or any other key to try again.");
                        var k = Console.ReadKey(true);
                        if (k.Key == ConsoleKey.Escape)
                        {
                            return;
                        }

                        continue;
                    }

                    break;
                }
                else
                {
                    if (!InitializeSteam(username, password, ctx))
                    {
                        Console.WriteLine("Login to steam failed. Restart the app and try again");
                        Console.ReadKey();
                        return;
                    }

                    break;
                }
            }
        }

        private static void CopyDepotFiles(AppSettings settings, SteamGame fo4)
        {
            Console.Title = GetTitle() + " - Copying Depot files";

            var deleteAfterCopy = !settings.KeepDepotFiles;//Console.ReadKey().Key == ConsoleKey.Y;
            var depotFolders = System.IO.Directory
                .GetDirectories("depots")
                .Where(x => !new DirectoryInfo(x).Name.StartsWith("."))
                .ToArray();

            foreach (var depotFolder in depotFolders)
            {
                // will contain subfolder
                foreach (var folder in Directory.GetDirectories(depotFolder, "*"))
                    CopyFiles(folder, fo4.Path);

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

        private static async Task DownloadDepotFilesAsync(AppSettings settings, bool downloadCreationKit)
        {

            /*
                download_depot 377160 377161 7497069378349273908
                download_depot 377160 377162 5847529232406005096
                download_depot 377160 377163 5819088023757897745
                download_depot 377160 377164 2178106366609958945
                download_depot 377160 435880 1255562923187931216
                download_depot 377160 435870 1691678129192680960
                download_depot 377160 435871 5106118861901111234
             */

            List<(uint, ulong)> depots = new List<(uint, ulong)>
                {
                    (377161,7497069378349273908),
                    (377162,5847529232406005096),
                    (377163,5819088023757897745),
                    (377164,2178106366609958945),
                    (435880,1255562923187931216),
                    (435870,1691678129192680960),
                    (435871,5106118861901111234)
                };


            ContentDownloader.Config.MaxDownloads = depots.Count;

            var language = settings.Language;
            if (!settings.DownloadAllLanguages && string.IsNullOrEmpty(settings.Language))
                language = "english";
            if (settings.DownloadAllLanguages)
                language = null;

            uint f4AppId = 377160;
            uint ckAppId = 1946160;

            await ContentDownloader.DownloadAppAsync(f4AppId, depots, "public", "windows", "64", settings.Language, false, false);

            // check if creation kit is available, if so, download and replace those as well.

            if (downloadCreationKit)
            {
                depots.Clear();
                depots.AddRange([
                    (1946161, 6928748513006443409),
                        (1946162, 3951536123944501689),
                    ]);
                await ContentDownloader.DownloadAppAsync(
                    ckAppId, depots, "public", "windows", "64", settings.Language, false, false);
            }
        }

        private static void DeleteNextGenFiles(SteamGame fo4, bool downloadCreationKit)
        {
            Console.Title = GetTitle() + " - Deleting Next Gen patches";

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

            // if we did not download the creation kit, delete all CC files
            if (!downloadCreationKit)
            {
                filesToDelete.AddRange([
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
                ]);
            }

            //Console.WriteLine("Next step will delete " + filesToDelete.Count + " files. Press any key to continue or CTRL+C to cancel.");
            //Console.ReadKey();

            Console.WriteLine("Deleting Next-Gen Content Files...");
            foreach (var file in filesToDelete)
            {
                var filePath = System.IO.Path.Combine(dataFolder, file);
                if (System.IO.File.Exists(filePath))
                {
                    Console.WriteLine("Deleting " + file);
                    System.IO.File.Delete(filePath);
                }
            }
        }

        private static AppSettings LoadSettingsFromArgs(params string[] args)
        {
            var p = Params.FromArgs(args);
            var settings = AppSettings.FromParams(p);
            var c = ContentDownloader.Config;
            c.Logger = new ConsoleLogger();
            c.UseQrCode = settings.UseQrCode;
            c.DownloadAllLanguages = settings.DownloadAllLanguages;
            return settings;
        }

        static bool InitializeSteam(string username, string password, DowngradeContext ctx)
        {
            if (!ContentDownloader.Config.UseQrCode)
            {
                if (username != null && password == null && (!ContentDownloader.Config.RememberPassword || !AccountSettingsStore.Instance.LoginTokens.ContainsKey(username)))
                {
                    do
                    {
                        Console.Write("Enter account password for \"{0}\": ", username);
                        if (Console.IsInputRedirected)
                        {
                            password = Console.ReadLine();
                        }
                        else
                        {
                            // Avoid console echoing of password
                            password = Util.ReadPassword();
                        }

                        Console.WriteLine();
                    } while (string.Empty == password);
                }
                else if (username == null)
                {
                    Console.WriteLine("No username given. Using anonymous account with dedicated server subscription.");
                }
            }

            return ContentDownloader.InitializeSteam3(username, password, ctx);
        }

        private static void CopyFiles(string srcDirectory, string dstDirectory)
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
                    Console.WriteLine("Failed to determine destination for file: " + file);
                    continue;
                }

                var dir = Path.GetDirectoryName(newPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                Console.WriteLine("Copying " + file + " to " + newPath + " (" + copyCount + "/" + fileCount + ")");

                File.Copy(file, newPath, true);
                copyCount++;
            }
        }
    }
}
