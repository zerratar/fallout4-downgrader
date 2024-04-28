using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FO4Down.Steam
{
    public static class SteamJson
    {
        public static IReadOnlyList<SteamLibFolder> ParseSteamFolders(string path)
        {
            var obj = SteamJsonParser.Parse(File.ReadAllText(path));
            var list = new List<SteamLibFolder>();

            foreach (var folder in obj.Children)
            {
                if (folder.Identifier == null)
                {
                    foreach (var f in folder.Children)
                    {
                        list.Add(ParseSteamFolder(f));
                    }
                }
                else
                {
                    list.Add(ParseSteamFolder(folder));
                }
            }

            return list;
        }

        private static SteamLibFolder ParseSteamFolder(SteamJsonParser.SteamJsonObject folder)
        {
            folder = folder.Children.First();
            var appsChild = folder.Children.First(x => x.Identifier == "apps");
            var apps = appsChild.Children.First().Strings.ToArray();

            return new SteamLibFolder
            {
                Path = folder["path"],
                Label = folder["label"],
                ContentId = folder["contentid"],
                TotalSize = folder["totalsize"],
                UpdateCleanBytesTally = folder["update_clean_bytes_tally"],
                TimeLastUpdateCorruption = folder["time_last_update_corruption"],
                Apps = apps
            };
        }
    }
}
