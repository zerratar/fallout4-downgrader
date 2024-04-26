using Fallout4Downgrader;
using System;
using System.Reflection;

namespace FO4Down
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ResetColor();


            Console.Title = "Fallout 4 Downgrader - Lets go back in time!";
            Console.WriteLine("Fallout 4 Downgrader - v" + Assembly.GetCallingAssembly().GetName().Version);
            Console.WriteLine("Contact: zerratar@gmail.com");
            Console.WriteLine("Source: https://www.github.com/zerratar/fallout4-downgrader");
            Console.WriteLine();

            var steamPath = SteamGameLocator.GetSteamInstallPath();
            if (string.IsNullOrEmpty(steamPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Steam is not installed!");
                Console.ReadKey();
                return;
            }

            var libraryFolders = SteamGameLocator.GetLibraryFolders(steamPath); // C:\\Program Files (x86)\\Steam\\steamapps\\temp\\libraryfolders.vdf"
            var installedGames = SteamGameLocator.GetInstalledGames(libraryFolders);

            //if (!installedGames.TryGetValue("Fallout 4", out var fo4))
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.WriteLine("Fallout 4 is not installed!");
            //    Console.ReadKey();
            //    return;
            //}

            var cmd = new SteamCMD();

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Console.ResetColor();
                cmd.Dispose();
            };

            Console.WriteLine("Starting up steamcmd... This can take some time.");

            using (cmd)
            {
                cmd.Start();

                string value = null;
                while ((value = cmd.ReadLine()) != null)
                {
                    Console.WriteLine("[SteamCMD] " + value);

                    if (value.Contains("Loading Steam API...OK"))
                    {
                        // Base game
                        cmd.SendCommand("download_depot 377160 377161 7497069378349273908");

                        Console.WriteLine("[SteamCMD] " + cmd.ReadLine());

                        cmd.SendCommand("download_depot 377160 377162 5847529232406005096");
                        cmd.SendCommand("download_depot 377160 377163 5819088023757897745");
                        cmd.SendCommand("download_depot 377160 377164 2178106366609958945");

                        // Workshop
                        cmd.SendCommand("download_depot 377160 435880 1255562923187931216");

                        // Automatron
                        cmd.SendCommand("download_depot 377160 435870 1691678129192680960");
                        cmd.SendCommand("download_depot 377160 435871 5106118861901111234");
                    }
                }
                cmd.Stop();
            }


            Console.ReadKey();
        }
    }
}
