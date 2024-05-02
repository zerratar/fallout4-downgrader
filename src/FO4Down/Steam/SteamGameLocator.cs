using Microsoft.Win32;
using SteamKit2.WebUI.Internal;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace FO4Down.Steam
{
    public class SteamGameLocator
    {
        public static string GetSteamInstallPath()
        {
            string keyPath = Environment.Is64BitOperatingSystem ?
                @"SOFTWARE\Wow6432Node\Valve\Steam" :
                @"SOFTWARE\Valve\Steam";
            return (string)Registry.LocalMachine.OpenSubKey(keyPath)?.GetValue("InstallPath");
        }

        public static void GetLibraryFolders(DowngradeContext ctx, string path)
        {
            ctx.LibraryFolders = new List<SteamLibFolder>();
            var vdfPath = path.EndsWith(".vdf") ? path : Path.Combine(path, @"steamapps\libraryfolders.vdf");
            if (File.Exists(vdfPath))
            {
                ctx.LibraryFolders.AddRange(SteamJson.ParseSteamFolders(vdfPath));
            }
        }


        public static void GetInstalledGames(DowngradeContext ctx)
        {
            var libraryFolders = ctx.LibraryFolders;
            var installedGames = new ConcurrentDictionary<string, SteamGame>(); // Use thread-safe collection

            var start = DateTime.Now;
            int reportedSlowSearch = 0;
            // Use Parallel.ForEach to handle each library folder concurrently
            var r = Parallel.ForEach(libraryFolders, folder =>
            {
                var now = DateTime.Now;
                var elapsed = now - start;

                if (elapsed > TimeSpan.FromSeconds(5) && Interlocked.CompareExchange(ref reportedSlowSearch, 1, 0) == 0)
                {
                    ctx.Warn("Looking for Fallout 4 installation is taking longer than expected\nJust keep the app running, it will find it eventually.");
                }

                if (!TryGetAppManifests(folder, out string[] acfFiles, out var manifestError))
                {
                    if (manifestError != null)
                    {
                        ctx.Error("Error reading appmanifests in '" + folder.Path + "'\nError: " + manifestError.Message);
                    }
                    else
                    {
                        ctx.Warn("No appmanifest files found in '" + folder.Path + "'");
                    }

                    return;
                }


                foreach (var apps in acfFiles)
                {
                    var gameName = ExtractGameNameFromAcf(ctx, apps);
                    if (string.IsNullOrEmpty(gameName))
                        continue;

                    var game = new SteamGame
                    {
                        Name = gameName,
                        Path = Path.Combine(folder.Path, "steamapps", "common", gameName),
                        AppId = apps
                    };

                    installedGames[gameName] = game; // Safe update of ConcurrentDictionary
                }
            });

            ctx.InstalledGames = new Dictionary<string, SteamGame>(installedGames); // Convert back to standard dictionary if needed
        }

        //public static void GetInstalledGames(DowngradeContext ctx)//List<SteamLibFolder> libraryFolders)
        //{
        //    var libraryFolders = ctx.LibraryFolders;
        //    ctx.InstalledGames = new Dictionary<string, SteamGame>();
        //    foreach (var folder in libraryFolders)
        //    {
        //        // Get by appmanifest_*.acf files
        //        if (!TryGetAppManifests(folder, out string[] acfFiles, out var manifestError))
        //        {
        //            if (manifestError != null)
        //            {
        //                ctx.Error("Error reading appmanifests in '" + folder.Path + "'\nError: " + manifestError.Message);
        //            }
        //            else
        //            {
        //                ctx.Warn("No appmanifest files found in '" + folder.Path);
        //            }
        //            continue;
        //        }
        //        foreach (var apps in acfFiles)
        //        {
        //            var gameName = ExtractGameNameFromAcf(ctx, apps);
        //            if (string.IsNullOrEmpty(gameName))
        //                continue;
        //            ctx.InstalledGames[gameName] = new SteamGame
        //            {
        //                Name = gameName,
        //                Path = Path.Combine(folder.Path, "steamapps", "common", gameName),
        //                AppId = apps
        //            };
        //        }
        //    }
        //}

        private static bool TryGetAppManifests(SteamLibFolder folder, out string[] acfFiles, out Exception exception)
        {
            acfFiles = [];
            exception = null;
            try
            {
                var path = Path.Combine(folder.Path, "steamapps");
                if (!Directory.Exists(path))
                    return false;

                acfFiles = Directory.GetFiles(path, "appmanifest_*.acf", SearchOption.AllDirectories);
                return acfFiles.Length > 0;
            }
            catch (Exception exc)
            {
                exception = exc;
            }

            return false;
        }

        private static string ExtractGameNameFromAcf(DowngradeContext ctx, string acfPath)
        {
            try
            {
                var lines = File.ReadAllLines(acfPath);
                Regex regex = new Regex("\"name\"\\s+\"(.+?)\"");
                foreach (string line in lines)
                {
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
            catch (Exception exc)
            {
                ctx.Error("Failed to extract game name from acf '" + acfPath + "': " + exc.Message);
            }
            return null;
        }
    }
}
