using FO4Down.Steam;
using FO4Down.SteamJson;

namespace SteamAppManifestGrabber
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var content = System.IO.File.ReadAllText("G:\\SteamLibrary\\steamapps\\appmanifest_377160.acf");
            var data = SteamJsonParser.Parse(content);
            //var str = data.AsString();
            //obj.Children

            var installedDepots = data.GetChild("InstalledDepots");
            if (installedDepots != null)
            {
                var manifestIds = await SteamDb.GetLatestManifestIDsAsync(installedDepots.Children.Select(x => x.Identifier));
                for (int i = 0; i < installedDepots.Children.Count; i++)
                {
                    var child = installedDepots.Children[i];
                    var depotId = child.Identifier;
                    if (manifestIds.ContainsKey(depotId))
                    {
                        child["manifest"] = manifestIds[depotId];
                    }
                }
            }



            // Target build is not necessary, it will most likely remain the same (for now?)



        }

    }
}
