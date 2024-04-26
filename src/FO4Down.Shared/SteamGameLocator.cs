using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Fallout4Downgrader
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

        public static List<SteamLibFolder> GetLibraryFolders(string path)
        {
            var libraryFolders = new List<SteamLibFolder>();
            var vdfPath = path.EndsWith(".vdf") ? path : Path.Combine(path, @"steamapps\libraryfolders.vdf");
            if (File.Exists(vdfPath))
            {
                libraryFolders.AddRange(SteamJson.ParseSteamFolders(vdfPath));
            }
            return libraryFolders;
        }

        public static Dictionary<string, SteamGame> GetInstalledGames(List<SteamLibFolder> libraryFolders)
        {
            var games = new Dictionary<string, SteamGame>();
            foreach (var folder in libraryFolders)
            {
                foreach (var apps in folder.Apps)
                {
                    var manifestPath = Path.Combine(folder.Path, "steamapps", "appmanifest_" + apps + ".acf");
                    if (System.IO.File.Exists(manifestPath))
                    {
                        var gameName = ExtractGameNameFromAcf(manifestPath);

                        games[gameName] = new SteamGame
                        {
                            Name = gameName,
                            Path = Path.Combine(folder.Path, "steamapps", "common", gameName),
                            AppId = apps
                        };
                    }
                }
            }
            return games;
        }

        private static string ExtractGameNameFromAcf(string acfPath)
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
            return null;
        }
    }
}
