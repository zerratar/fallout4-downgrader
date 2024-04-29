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
    }
}
