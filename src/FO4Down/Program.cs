using FO4Down.Core;
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

#if DEBUG
        public static string[] CommandLineArguments; // only during debug so we can manipulate
#endif

        static async Task Main(string[] args)
        {
            //var s = Fallout4IniSettings.FromIni(@"G:\SteamLibrary\steamapps\common\Fallout 4\Fallout4_Default.ini");
            //var general = s["General"];
            //general["sLanguage"] = "de";
            //var archive = s["Archive"];
            //var props = archive.Properties;
            //foreach (var archives in archive.Properties)
            //{
            //    props[archives.Key]
            //}

//#if DEBUG
//            CommandLineArguments = ["-download-depots"];
//#endif


            if (args.Contains("-help") || args.Contains("-h") || args.Contains("/?") || args.Contains("-?"))
            {
                Console.WriteLine("Fallout 4 Downgrader - v" + Assembly.GetExecutingAssembly().GetName().Version);
                Console.WriteLine("Usage: FO4Down.exe (optional: arguments)");
                Console.WriteLine("  -install-plugins\tAutomatically install needed plugins into Fallout 4\\Data\\ folder necessary for patched exe to work.");
                Console.WriteLine("  -patch-files\tForces the application to start downgrade by Patching files");
                Console.WriteLine("  -download-depots\tForces the application to start downgrade by downloading depots");
                Console.WriteLine("  -user or -username <steam user>");
                Console.WriteLine("  -pass or -password <steam pass>");
                Console.WriteLine("  -qr\t\t\tLogin using QR code instead");
                Console.WriteLine("  -ck or -creation-kit\tForce downgrade the creation kit as well, this will automatically happen if you have creation kit already installed");
                Console.WriteLine("  -language <language>\tDownloads a specific language, default is english");
                Console.WriteLine("  -keep-depot\t\tKeep the downloaded depot files after finish");
                Console.WriteLine("  -help\t\t\tThis text");
                Console.ReadKey();
                return;
            }

            Application.Run<MainWindow>();
            Application.Shutdown();
        }
    }
}
