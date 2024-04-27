using DepotDownloader;
using SteamKit2.GC.Artifact.Internal;
using SteamKit2.Internal;
using System.Reflection;

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
            Console.ResetColor();
            Console.Title = "Fallout 4 Downgrader - Lets go back in time!";
            Console.WriteLine("Fallout 4 Downgrader - v" + Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("Contact: zerratar@gmail.com");
            Console.WriteLine("Source: https://www.github.com/zerratar/fallout4-downgrader");
            Console.WriteLine("Using DepotDownloader for downloading depots.");
            Console.WriteLine("Ref: https://github.com/SteamRE/DepotDownloader");
            Console.WriteLine();

            var steamPath = SteamGameLocator.GetSteamInstallPath();
            if (string.IsNullOrEmpty(steamPath))
            {
                Console.WriteLine("Steam is not installed!");
                Console.ReadKey();
                return;
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Fallout 4 is not installed.");
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Found Fallout 4 at " + fo4.Path);
            Console.ResetColor();

            AccountSettingsStore.LoadFromFile("account.config");
            Console.WriteLine("Please login using your credentials");
            Console.Write("Username: ");
            var username = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();

            if (!InitializeSteam(username, password))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Login to steam failed.");
                Console.ReadKey();
                return;
            }

            /*
                download_depot 377160 377161 7497069378349273908
                download_depot 377160 377162 5847529232406005096
                download_depot 377160 377163 5819088023757897745
                download_depot 377160 377164 2178106366609958945
                download_depot 377160 435880 1255562923187931216
                download_depot 377160 435870 1691678129192680960
                download_depot 377160 435871 5106118861901111234
             */

            uint appId = 377160;
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

            try
            {
                await ContentDownloader.DownloadAppAsync(appId, depots, "public", "windows", "64", "english", false, false);
            }
            catch (Exception exc)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exc.ToString());
                Console.ReadKey();
                return;
            }

            // when download is complete, copy all the files into the fallout 4 folder
            //Console.WriteLine("All depots download completed. Do you wish to delete the downloaded files after copying it to " + fo4.Path + "? [Y/N]");

            Console.Title = GetTitle() + " - Copying Depot files";

            var deleteAfterCopy = true;//Console.ReadKey().Key == ConsoleKey.Y;
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

            Console.Title = GetTitle() + " - Finished";
            Console.WriteLine("Downgrade complete! Application will exit");
        }

        static bool InitializeSteam(string username, string password)
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

            return ContentDownloader.InitializeSteam3(username, password);
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


                File.Copy(file, newPath, true);
                copyCount++;
            }

        }
    }

}
