using System.Reflection;

namespace Fallout4Downgrader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Fallout 4 Downgrader - Lets go back in time!";
            Console.WriteLine("Fallout 4 Downgrader - v" + Assembly.GetCallingAssembly().GetName().Version);
            Console.WriteLine("Contact: zerratar@gmail.com");
            Console.WriteLine("Source: https://www.github.com/zerratar/fallout4-downgrader");
            Console.WriteLine();

            var steamPath = SteamGameLocator.GetSteamInstallPath();
            if (string.IsNullOrEmpty(steamPath))
            {
                Console.WriteLine("Steam is not installed!");
                Console.ReadKey();
                return;
            }

            var libraryFolders = SteamGameLocator.GetLibraryFolders(steamPath);
            var installedGames = SteamGameLocator.GetInstalledGames(libraryFolders);

            Console.WriteLine("Installed games and their paths:");
            foreach (var game in installedGames)
            {
                Console.WriteLine($"{game.Key} is installed in {game.Value.Path}");
            }
            Console.ReadKey();
        }
    }

}
