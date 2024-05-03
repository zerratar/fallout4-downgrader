using Fallout4Downgrader;
using FO4Down.Core;
using FO4Down.Steam;
using FO4Down.Steam.DepotDownloader;
using SharpCompress;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using SteamKit2.GC.CSGO.Internal;
using System.Diagnostics;
using System.IO.Compression;
using System.IO.Hashing;
using System.Net.Http;
using System.Security.Cryptography;
using ZstdSharp.Unsafe;

namespace FO4Down
{
    public class FO4Downgrader
    {
        private const string Fallout4_AppId = "377160";
        private const string Fallout4_Name = "Fallout 4";

        public async Task RunAsync(Action<ApplicationContext> onStepUpdate)
        {
            var ctx = new ApplicationContext();
            ctx.OnStepUpdate = onStepUpdate;
            ctx.Settings = LoadSettings(ctx);

            try
            {

                Steam3Session.OnDisplayQrCode += (qrCode) =>
                {
                    ctx.QRCode = qrCode;
                    ctx.Notify();
                };

                // step 1: Find out where or if Fallout 4 is installed
                var fo4 = FindFallout4(ctx);

                CheckIfF4SEIsInstalled(ctx);

                CheckIfF4SEAddressLibraryIsInstalled(ctx);

                CheckIfF4SEBAASIsInstalled(ctx);

                CheckIfPatchIsPossible(ctx);

                if (await HandlePatchAsync(ctx))
                {
                    return;
                }

                //// step 2.5: Handle user settings
                //// before we start, we should request settings changes that the user may want
                //// such as language, whether or not it should try to download all dlcs and/or hd textures pack.
                //await HandleUserSettingsAsync(ctx);

                // step 2: login to steam
                while (!await LoginToSteamAsync(ctx))
                {
                    ctx.Error("Login failed. Invalid username or password.\nLogin using QR if problem persists");
                }

                // step 2.5: Handle user settings
                // before we start, we should request settings changes that the user may want
                // such as language, whether or not it should try to download all dlcs and/or hd textures pack.
                await HandleUserSettingsAsync(ctx);

                // step 3: Download all depot files into the /depots/ folder.
                await DownloadDepotFilesAsync(ctx);

                // step 4: Delete all next gen files before we start copying over our files from the depot, this ensures we don't delete files that should be there.
                DeleteNextGenFiles(ctx);

                // step 5: copy all depot files from /depots/ to /fallout 4/ and then delete the /depots/ folder
                CopyDepotFiles(ctx);

                // step 6: delete creation club data if needed.
                if (ctx.Settings.DeleteCreationClubFiles)
                {
                    DeleteCreationClubData(ctx);
                }

                // 7. delete english language files if we selected a non-english language and update ini file
                await ApplyLanguageAsync(ctx);

                ctx.Step = FO4DowngraderStep.Finished;

                if (ctx.LoggedErrors.Count > 0)
                {
                    ctx.WarnAndReport("Your Fallout 4 installation has been downgraded but with " + ctx.LoggedErrors.Count + " error(s) and may not work as expected.");
                }
                else
                {
                    ctx.Notify("Your Fallout 4 installation has been downgraded without any problems. Enjoy!\nYou may now close this application.");
                }

            }
            catch (FileNotFoundException ex)
            {
                ctx.Error(ex);
            }
            catch (Exception ex)
            {
                ctx.Report(ex);
            }
            finally
            {
                ctx.SaveLog();
            }
        }

        private async Task<bool> HandlePatchAsync(ApplicationContext ctx)
        {
            if (!ctx.CanPatch)
            {
                if (ctx.Settings.InstallPlugins)
                {
                    await InstallPluginsAsync(ctx);
                }

                return false;
            }

            ctx.Step = FO4DowngraderStep.Patch;

            if (ctx.Settings.InstallPlugins)
            {
                await InstallPluginsAsync(ctx);
            }
            else if (ctx.Settings.InstallHelperEnabled && (!ctx.IsF4SEInstalled || !ctx.IsF4SEAddressLibraryInstalled || !ctx.IsF4SEBASSInstalled))
            {
                var shouldPatch = await ctx.RequestAsync<bool>("confirm");
                if (!shouldPatch) return false;
            }

            ctx.Notify("Patching Fallout4.exe...");
            Patch(ctx.Patch["Fallout4.exe"]);

            ctx.Notify("Patching steam_api64.dll...");
            Patch(ctx.Patch["steam_api64.dll"]);

            ctx.Notify("Patching Fallout4Launcher.exe...");
            Patch(ctx.Patch["Fallout4Launcher.exe"]);


            ctx.Step = FO4DowngraderStep.Finished;

            var helperText = "";

            if (!ctx.IsF4SEInstalled)
            {
                helperText += "\n* F4SE";
            }

            if (!ctx.IsF4SEAddressLibraryInstalled)
            {
                helperText += "\n* F4SE - Address Library Plugin";
            }

            if (!ctx.IsF4SEBASSInstalled)
            {
                helperText += "\n* F4SE - BASS Plugin";
            }

            if (helperText.Length > 0)
            {
                ctx.Notify("All files patched! Your Fallout 4 installation has been downgraded!\nDon't forget to install the following mods+plugins:" + helperText);
            }
            else
            {
                ctx.Notify("All files patched! Your Fallout 4 installation has been downgraded!\nHappy Modding!");
            }


            return true;
        }

        private static async Task InstallPluginsAsync(ApplicationContext ctx)
        {
            if (!ctx.IsF4SEInstalled)
            {
                await InstallF4SEAsync(ctx);
            }

            if (!ctx.IsF4SEAddressLibraryInstalled)
            {
                await InstallAddressLibraryPluginAsync(ctx);
            }

            if (!ctx.IsF4SEBASSInstalled)
            {
                await InstallBASSAsync(ctx);
            }
        }

        public static async Task InstallBASSAsync(ApplicationContext ctx)
        {
            ctx.Notify("Installing F4SE Plugin: BASS...");
            var zip = "Downloads/baas.zip";
            if (!File.Exists(zip))
            {
                if (ctx.HttpClient == null)
                    ctx.HttpClient = new HttpClient();
                await DownloadFileAsync(ctx.HttpClient, "https://github.com/zerratar/fallout4-downgrader/releases/download/v1.0.5.2/BackportedBA2Support-1_0-81859-1-0-1714516128.zip", zip);
            }

            var targetDirectory = System.IO.Path.Combine(ctx.Fallout4.Path);
            using (var archive = SharpCompress.Archives.Zip.ZipArchive.Open("Downloads/baas.zip"))
            {
                ExtractToDirectory(archive, targetDirectory);
            }
            ctx.IsF4SEBASSInstalled = true;
        }

        public static async Task InstallAddressLibraryPluginAsync(ApplicationContext ctx)
        {
            ctx.Notify("Installing F4SE Plugin: Address Library...");
            var zip = "Downloads/address.library.zip";
            if (!File.Exists(zip))
            {
                if (ctx.HttpClient == null)
                    ctx.HttpClient = new HttpClient();
                await DownloadFileAsync(ctx.HttpClient, "https://github.com/zerratar/fallout4-downgrader/releases/download/v1.0.5.2/Addres.Library-47327-1-10-163-0-1599728753.zip", zip);
            }
            var targetDirectory = Path.Combine(ctx.Fallout4.Path, "Data\\");
            using (var archive = SharpCompress.Archives.Zip.ZipArchive.Open("Downloads/address.library.zip"))
            {
                ExtractToDirectory(archive, targetDirectory);
            }
            ctx.IsF4SEAddressLibraryInstalled = true;
        }

        public static async Task InstallF4SEAsync(ApplicationContext ctx)
        {
            ctx.Notify("Installing F4SE...");
            var zip = "Downloads/f4se.zip";
            if (!File.Exists(zip))
            {
                if (ctx.HttpClient == null)
                    ctx.HttpClient = new HttpClient();
                await DownloadFileAsync(ctx.HttpClient, "https://github.com/zerratar/fallout4-downgrader/releases/download/v1.0.5.2/f4se.zip", zip);
            }
            var targetDirectory = Path.Combine(ctx.Fallout4.Path);
            using (var archive = SharpCompress.Archives.Zip.ZipArchive.Open("Downloads/f4se.zip"))
            {
                ExtractToDirectory(archive, targetDirectory);
            }
            ctx.IsF4SEInstalled = true;
        }

        public static async Task DownloadFileAsync(HttpClient client, string url, string destinationOnDisk)
        {
            // Send a GET request to the specified URL
            using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode(); // Throw an exception if the HTTP response status is an error

                // Read the content as a stream.
                var dir = Path.GetDirectoryName(destinationOnDisk);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                      fileStream = new FileStream(destinationOnDisk, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    // Copy the content stream to the file stream asynchronously
                    await contentStream.CopyToAsync(fileStream);
                }
            }
        }

        public static void ExtractToDirectory(IArchive archive, string destination, Action<double>? progressReport = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            long totalUncompressSize = archive.TotalUncompressSize;
            long num = 0L;
            HashSet<string> hashSet = new HashSet<string>();
            IReader reader = archive.ExtractAllEntries();
            while (reader.MoveToNextEntry())
            {
                cancellationToken.ThrowIfCancellationRequested();
                IEntry entry = reader.Entry;
                if (!entry.IsDirectory)
                {
                    string text = Path.Combine(destination, entry.Key.NotNull("Entry Key is null"));
                    string directoryName = Path.GetDirectoryName(text);
                    if (directoryName != null && hashSet.Add(text))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    if (!System.IO.File.Exists(text))
                    {
                        using FileStream writableStream = File.OpenWrite(text);
                        reader.WriteEntryTo(writableStream);
                    }
                    num += entry.Size;
                    progressReport?.Invoke((double)num / (double)totalUncompressSize);
                }
            }
        }

        public static void CheckIfF4SEAddressLibraryIsInstalled(ApplicationContext ctx)
        {
            if (ctx.IsF4SEAddressLibraryInstalled) return;
            if (!ctx.IsF4SEInstalled)
            {
                return;
            }

            var f = "F4SE\\Plugins\\version-1-10-163-0.bin";
            if (//File.Exists(Path.Combine(ctx.Fallout4.Path, f)) || 
                File.Exists(Path.Combine(ctx.Fallout4.Path, "Data", f)))
            {
                ctx.IsF4SEAddressLibraryInstalled = true;
            }
        }

        public static void CheckIfF4SEBAASIsInstalled(ApplicationContext ctx)
        {
            if (ctx.IsF4SEBASSInstalled) return;
            if (!ctx.IsF4SEInstalled)
            {
                return;
            }

            var f = "F4SE\\Plugins\\BackportedBA2Support.dll";
            if (//File.Exists(Path.Combine(ctx.Fallout4.Path, f)) || 
                File.Exists(Path.Combine(ctx.Fallout4.Path, "Data", f)))
            {
                ctx.IsF4SEBASSInstalled = true;
            }
        }

        public static void CheckIfF4SEIsInstalled(ApplicationContext ctx)
        {
            if (ctx.IsF4SEInstalled) return;

            var f4se = Path.Combine(ctx.Fallout4.Path, "f4se_loader.exe");
            if (File.Exists(f4se))
            {
                ctx.IsF4SEInstalled = true;
            }
        }

        private void CheckIfPatchIsPossible(ApplicationContext ctx)
        {
            // check if the patch is possible
            // get the hash of the fallout4 exe to determine
            var fo4Exe = Path.Combine(ctx.Fallout4.Path, "Fallout4.exe");
            var steamApi = Path.Combine(ctx.Fallout4.Path, "steam_api64.dll");
            var launcher = Path.Combine(ctx.Fallout4.Path, "Fallout4Launcher.exe");

            if (CanPatch(ctx, fo4Exe) && CanPatch(ctx, steamApi) && CanPatch(ctx, launcher))
            {
                ctx.CanPatch = true;
            }
        }

        private static bool CanPatch(ApplicationContext ctx, string file)
        {
            var id = Crc64.HashToUInt64(File.ReadAllBytes(file));
            byte[] hash = null;
            using (var read = File.OpenRead(file))
            {
                hash = GetChecksumBuffered(read);
            }
            var pi = ctx.Patch[Path.GetFileName(file)] = new PatchInfo
            {
                Id = id,
                Hash = hash,
                Target = file,
                Files = GetPatches(id),
            };
            return pi.Files.Length > 0;
        }

        private static string[] GetPatches(ulong id)
        {
            return Directory
                .GetFiles("Patch", id + "_*.patch")
                .OrderBy(x => int.Parse(x.Split('_')[1].Split('.')[0])).ToArray();
        }

        private static void Patch(PatchInfo pi)
        {
            string fileToBeReplaced = pi.Target;
            ulong id = pi.Id;
            byte[] hash = pi.Hash;
            var patches = pi.Files;
            if (patches.Length == 0)
            {
                return;
            }

            if (System.IO.File.Exists(fileToBeReplaced) && !System.IO.File.Exists(fileToBeReplaced + "_backup"))
                File.Move(fileToBeReplaced, fileToBeReplaced + "_backup");

            using (var output = File.OpenWrite(fileToBeReplaced))
            {
                for (var i = 0; i < patches.Length; ++i)
                {
                    var file = patches[i];
                    using (var read = File.OpenRead(file))
                    //using (var gzip = new GZipStream(read, CompressionMode.Decompress))
                    {
                        var buffer = new byte[read.Length];
                        read.Read(buffer, 0, buffer.Length);
                        var toWrite = Decrypt(buffer, hash);
                        output.Write(toWrite, 0, toWrite.Length);
                    }
                }
            }
        }

        public static byte[] Decrypt(byte[] source, byte[] key)
        {
            if (!(key.Length == 16 || key.Length == 24 || key.Length == 32))
                throw new ArgumentException("Key size is not valid for AES. Key must be either 128, 192, or 256 bits.");

            using (var aesAlg = System.Security.Cryptography.Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.Mode = CipherMode.CBC;

                // Extract the IV from the original data and create decryptor
                byte[] iv = new byte[aesAlg.BlockSize / 8];
                Array.Copy(source, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(source, iv.Length, source.Length - iv.Length))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] buffer = new byte[source.Length];
                        var bytesRead = csDecrypt.Read(buffer, 0, buffer.Length);
                        Array.Resize(ref buffer, bytesRead);
                        return buffer;
                    }
                }
            }
        }

        private static byte[] GetChecksumBuffered(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
            {
                return SHA256.HashData(bufferedStream);
            }
        }

        private static async Task ApplyLanguageAsync(ApplicationContext ctx)
        {
            ctx.Step = FO4DowngraderStep.ApplyLanguage;

            var fo4 = ctx.Fallout4;
            var targetCulture = ctx.GetTargetCultureInfo();
            var targetLanguage = targetCulture.EnglishName;
            if (!ctx.Settings.DownloadAllLanguages && targetLanguage.IndexOf("english", StringComparison.OrdinalIgnoreCase) == -1)
            {
                var fallout4Files = Directory.GetFiles(fo4.Path, "*_" + targetCulture.TwoLetterISOLanguageName + ".*", SearchOption.AllDirectories);
                var englishFilesToDelete = new List<string>();
                foreach (var file in fallout4Files)
                {
                    var name = System.IO.Path.GetFileName(file);
                    var dir = System.IO.Path.GetDirectoryName(file);
                    var duplicate = Path.Combine(dir, name.Replace("_" + targetCulture.TwoLetterISOLanguageName, "_en", StringComparison.OrdinalIgnoreCase));
                    if (File.Exists(duplicate))
                    {
                        englishFilesToDelete.Add(duplicate);
                    }
                }

                if (englishFilesToDelete.Count > 0)
                {
                    var result = ctx.Settings.DeleteEnglishLanguageFiles ||
                        await ctx.RequestAsync<bool>("confirm", englishFilesToDelete.Count + " English archives have been found in your Data folder.\nThis will most likely prevent your selected language \"" + targetLanguage + "\" from working.\nDo you want to delete these?");
                    if (result)
                    {
                        foreach (var f in englishFilesToDelete)
                        {
                            System.IO.File.Delete(f);
                        }
                    }
                }

                // only update fallout 4 ini if it exists. as default one will be replaced
                if (ctx.Fallout4Ini != null)
                {
                    var ini = ctx.Fallout4Ini;
                    var langCode = targetCulture.TwoLetterISOLanguageName.ToLower();
                    var general = ini["General"];
                    var existingLanguageSettings = general["sLanguage"];
                    if (existingLanguageSettings != langCode)
                    {
                        ini["General"]["sLanguage"] = langCode;
                        var archive = ini["Archive"];
                        foreach (var prop in archive.Properties)
                        {
                            archive.Properties[prop.Key] = prop.Value.Replace("_en.", "_" + langCode + ".");
                        }

                        ini.Save();
                    }
                }
            }
        }

        private static T Random<T>(params T[] values)
        {
            if (values.Length == 0) return default;
            if (values.Length == 1) return values[0];
            return values[System.Random.Shared.Next(values.Length)];
        }

        private async Task HandleUserSettingsAsync(ApplicationContext ctx)
        {
            ctx.Step = FO4DowngraderStep.UserSettings;
            ctx.Message = Random([
                "Fine-tune your survival kit.",
                "Adjust your settings before venturing out into the Commonwealth.",
                "Customize your experience, wastelander.",
                "Tweak the dials, tune the wasteland.",
                "Choose your gear wisely.",
            ]);

            var userSettings = await ctx.RequestAsync<UserProvidedSettings>("settings");
            if (userSettings != null)
            {
                ctx.Merge(userSettings);
            }
        }

        private static void CopyDepotFiles(ApplicationContext ctx)
        {
            ctx.Step = FO4DowngraderStep.CopyDepotFiles;

            AppSettings settings = ctx.Settings;
            SteamGame fo4 = ctx.Fallout4;

            var deleteAfterCopy = !settings.KeepDepotFiles && !Debugger.IsAttached;//Console.ReadKey().Key == ConsoleKey.Y;
            var depotFolders = System.IO.Directory
                .GetDirectories("depots")
                .Where(x => !new DirectoryInfo(x).Name.StartsWith("."))
                .ToArray();

            foreach (var depotFolder in depotFolders)
            {
                // will contain subfolder
                foreach (var folder in Directory.GetDirectories(depotFolder, "*"))
                    CopyFiles(ctx, folder, fo4.Path);

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

        private static void CopyFiles(ApplicationContext ctx, string srcDirectory, string dstDirectory)
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
                    ctx.Error("Failed determine destination for file: " + file);
                    continue;
                }

                var dir = Path.GetDirectoryName(newPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                ctx.Progress("Copying: " + file.Replace(srcDirectory, ""), copyCount / (float)fileCount);
                File.Copy(file, newPath, true);
                copyCount++;
            }

            ctx.Progress("All files copied.", 1);
        }

        private static void DeleteCreationClubData(ApplicationContext ctx)
        {
            ctx.Step = FO4DowngraderStep.DeleteCreationClubData;
            var fo4 = ctx.Fallout4;
            var deleteCreationClubData = ctx.Settings.DeleteCreationClubFiles;
            var dataFolder = System.IO.Path.Combine(fo4.Path, "Data");
            var filesToDelete = new List<string>
            {
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
            };

            ctx.Notify("Deleting Creation Club Content Files...");
            var paths = filesToDelete.Select(x => Path.Combine(dataFolder, x)).ToArray();
            var existingFiles = paths.Where(x => File.Exists(x)).ToArray();

            for (int i = 0; i < existingFiles.Length; i++)
            {
                string? file = existingFiles[i];
                ctx.Progress("Deleting " + Path.GetFileName(file) + " (" + (i + 1) + "/" + existingFiles.Length + ")", (i / (float)existingFiles.Length));
                File.Delete(file);
            }
            ctx.Progress("All Creation Club files deleted.", 1f);
        }

        private static void DeleteNextGenFiles(ApplicationContext ctx)
        {
            ctx.Step = FO4DowngraderStep.DeleteNextGenFiles;
            var fo4 = ctx.Fallout4;
            var downloadCreationKit = ctx.DownloadCreationKit;
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

            ctx.Notify("Deleting Next-Gen Content Files...");
            var paths = filesToDelete.Select(x => Path.Combine(dataFolder, x)).ToArray();
            var existingFiles = paths.Where(x => File.Exists(x)).ToArray();

            for (int i = 0; i < existingFiles.Length; i++)
            {
                string? file = existingFiles[i];
                ctx.Progress("Deleting " + Path.GetFileName(file) + " (" + (i + 1) + "/" + existingFiles.Length + ")", (i / (float)existingFiles.Length));
                File.Delete(file);
            }
            ctx.Progress("All Next-Gen Content Files deleted.", 1f);
        }


        private static async Task DownloadDepotFilesAsync(ApplicationContext ctx)
        {
            ctx.Step = FO4DowngraderStep.DownloadDepotFiles;
            var settings = ctx.Settings;
            var downloadCreationKit = ctx.DownloadCreationKit;

            List<Depot> depots = new List<Depot>();

            var language = settings.Language?.ToLower();
            if (!settings.DownloadAllLanguages && string.IsNullOrEmpty(settings.Language))
                language = "english";
            if (settings.DownloadAllLanguages)
                language = null;

            // verify whether or not we are downloading the correct language.
            if (!settings.DownloadAllLanguages)
            {
                var targetLanguage = ctx.GetTargetCultureInfo();
                if (!targetLanguage.EnglishName.Equals(language, StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Message = "Preparing for download...";
                    if (await ctx.RequestAsync<bool>("confirm",
                        "You are about to download English version of the game\n" +
                        "but your current Fallout 4 installation is in \"" + targetLanguage.EnglishName + "\".\n" +
                        "Do you want to download " + targetLanguage.EnglishName + " instead?"))
                    {
                        language = targetLanguage.EnglishName.ToLower();
                    }
                }
            }

            depots.AddRange(DepotManager.GetLanguageNeutral(DepotTarget.Game));
            depots.AddRange(DepotManager.GetLanguageNeutral(DepotTarget.RequiredDlc));

            depots.AddRange(DepotManager.Get(DepotTarget.Game, language));
            depots.AddRange(DepotManager.Get(DepotTarget.RequiredDlc, language));

            if (ctx.Settings.DownloadAllDLCs)
            {
                depots.AddRange(DepotManager.GetLanguageNeutral(DepotTarget.AllDlc));
                depots.AddRange(DepotManager.Get(DepotTarget.AllDlc, language));
            }

            if (ctx.Settings.DownloadHDTextures)
            {
                // <HD Textures>
                depots.Add(DepotManager.GetHDTextures());
            }

            var totalDepotsToDownload = depots.Count + (downloadCreationKit ? 2 : 0);

            ContentDownloader.Config.MaxDownloads = depots.Count;

            uint f4AppId = 377160;
            uint ckAppId = 1946160;

            //ctx.Notify("Downloading ")
            ctx.TotalDepotsToDownload = totalDepotsToDownload;
            ctx.Step = FO4DowngraderStep.DownloadGameDepotFiles;
            await ContentDownloader.DownloadAppAsync(
                f4AppId, depots.Select(x => x.AsTuple()).ToList(), "public", "windows", "64",
                settings.Language,
                false, false, ctx);

            // check if creation kit is available, if so, download and replace those as well.

            if (downloadCreationKit)
            {
                ctx.Step = FO4DowngraderStep.DownloadCreationKitDepotFiles;
                depots.Clear();
                depots.AddRange(DepotManager.GetCreationKit());
                await ContentDownloader.DownloadAppAsync(
                    ckAppId, depots.Select(x => x.AsTuple()).ToList(),
                    "public", "windows", "64",
                    settings.Language, false, false, ctx);
            }

            ctx.Notify();
        }

        private static SteamGame FindFallout4(ApplicationContext ctx)
        {
            ctx.Step = FO4DowngraderStep.LookingForFallout4Path;
            ctx.Notify("Searching the wasteland to find your installation of Fallout 4");

            var fo4 = TryFindFalloutInCurrentPath(ctx);

            if (fo4 == null)
            {
                // try finding the game path based on standard paths that the game may have
                fo4 = FindFallout4ByKnownPaths(ctx, fo4);
            }

            if (fo4 == null)
            {
                var steamPath = SteamGameLocator.GetSteamInstallPath();
                if (string.IsNullOrEmpty(steamPath))
                {
                    throw new FileNotFoundException("Steam is not installed.");
                }

                //steamPath = "G:\\GitHub\\fallout4-downgrader\\publish\\Self-contained\\libraryfolders.vdf";
                SteamGameLocator.GetLibraryFolders(ctx, steamPath);
                SteamGameLocator.GetInstalledGames(ctx);

                if (!ctx.InstalledGames.TryGetValue("Fallout 4", out fo4))
                {
                    //add fallback to find fallout 4 folder in parent folder or same folder.look for manifest

                    throw new FileNotFoundException("Fallout 4 is not installed.");
                }

                ctx.Fallout4 = fo4;
            }


            ctx.DownloadCreationKit = Directory.Exists(Path.Combine(fo4.Path, "Tools"))
                    || Directory.Exists(Path.Combine(fo4.Path, "Papyrus Compiler"))
                    || ctx.Settings.DownloadCreationKit;

            var defaultIni = Path.Combine(fo4.Path, "Fallout4_Default.ini");
            if (System.IO.File.Exists(defaultIni))
            {
                ctx.Fallout4DefaultIni = Fallout4IniSettings.FromIni(defaultIni);
            }

            var ini = Path.Combine(fo4.Path, "Fallout4.ini");
            if (System.IO.File.Exists(ini))
            {
                ctx.Fallout4Ini = Fallout4IniSettings.FromIni(ini);
            }

            ctx.Notify();
            return fo4;
        }

        private static SteamGame? TryFindFalloutInCurrentPath(ApplicationContext ctx)
        {
            SteamGame? fo4 = null;

            var currentPath = Directory.GetCurrentDirectory();
            if (currentPath.IndexOf(@"system32", StringComparison.OrdinalIgnoreCase) != -1)
            {
                throw new Exception("You cannot run FO4Down.exe directly from the zip file\nPlease extract it to a folder before running.");
            }

            var isInsideFallout4Folder = currentPath.IndexOf("\\Fallout 4\\", StringComparison.OrdinalIgnoreCase) > 0;
            var isFallout4Folder = currentPath.EndsWith("\\Fallout 4", StringComparison.OrdinalIgnoreCase);
            if (isInsideFallout4Folder || isFallout4Folder)
            {
                if (isFallout4Folder && File.Exists(Path.Combine(currentPath, "Fallout4.exe")))
                {
                    fo4 = ctx.Fallout4 = new SteamGame
                    {
                        AppId = Fallout4_AppId,
                        Name = Fallout4_Name,
                        Path = currentPath
                    };
                }
                else if (isInsideFallout4Folder)
                {
                    // look into parent folders
                    while (true)
                    {
                        var info = Directory.GetParent(currentPath);
                        if (info == null) break;

                        currentPath = info.FullName;
                        var fo4Path = Path.Combine(currentPath, "Fallout4.exe");
                        if (info.Name.Equals("Fallout 4", StringComparison.OrdinalIgnoreCase) && File.Exists(fo4Path))
                        {
                            fo4 = ctx.Fallout4 = new SteamGame
                            {
                                AppId = Fallout4_AppId,
                                Name = Fallout4_Name,
                                Path = currentPath
                            };
                            break;
                        }
                    }
                }
            }

            return fo4;
        }

        private static SteamGame? FindFallout4ByKnownPaths(ApplicationContext ctx, SteamGame? fo4)
        {
            if (TryGetDrives(out var drives))
            {
                foreach (var drive in drives)
                {
                    try
                    {
                        var root = drive.RootDirectory.FullName;
                        var libraryPath = Path.Combine(root, @"SteamLibrary\steamapps\common\Fallout 4");
                        if (!Path.Exists(libraryPath))
                        {
                            libraryPath = Path.Combine(root, @"Program Files (x86)\Steam\steamapps\common\Fallout 4");
                        }

                        var f4exe = System.IO.Path.Combine(libraryPath, "Fallout4.exe");
                        if (Path.Exists(f4exe))
                        {
                            fo4 = ctx.Fallout4 = new SteamGame
                            {
                                AppId = Fallout4_AppId,
                                Name = Fallout4_Name,
                                Path = libraryPath
                            };
                            break;
                        }
                    }
                    catch { }
                }
            }

            return fo4;
        }

        private static bool TryGetDrives(out DriveInfo[] drives)
        {
            try
            {
                drives = DriveInfo.GetDrives();
                return true;
            }
            catch
            {
                drives = [];
            }
            return false;
        }

        private static async Task<bool> LoginToSteamAsync(ApplicationContext ctx)
        {
            ctx.Step = FO4DowngraderStep.LoginToSteam;

            if (!AccountSettingsStore.Loaded)
                AccountSettingsStore.LoadFromFile("account.config");

            var settings = ctx.Settings;
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

            if (!settings.UseQrCode)
            {
                var noUser = string.IsNullOrEmpty(username);
                var noPass = string.IsNullOrEmpty(password);

                if (noUser || noPass)
                {
                    (username, password) = await ctx.RequestAsync<(string, string)>("credentials");

                    if (ctx.Settings.UseQrCode)
                    {
                        ContentDownloader.Config.UseQrCode = true;
                    }
                }
            }

            var result = ContentDownloader.InitializeSteam3(username, password, ctx);
            if (result)
            {
                ctx.Notify();
            }

            return result;
        }

        private static AppSettings LoadSettings(ApplicationContext ctx)
        {
            var p = Params.FromArgs(Environment.GetCommandLineArgs());
            var settings = AppSettings.FromParams(p);
            var c = ContentDownloader.Config;
            // c.Logger = new ConsoleLogger();
            c.Logger = new DelegateLogger((severity, format, args) =>
            {
                switch (severity)
                {
                    case LogSeverity.Error:
                        ctx.Error(format, args);
                        break;

                    case LogSeverity.Debug:
                    case LogSeverity.Information:
                    case LogSeverity.Warning:
                        ctx.Notify(format, args);
                        break;
                }
            });
            c.UseQrCode = settings.UseQrCode;
            c.DownloadAllLanguages = settings.DownloadAllLanguages;
            return settings;
        }
    }

    public class StepRequest<T> : StepRequest
    {
        public async Task<T> AwaitResponseAsync()
        {
            var result = await src.Task;
            if (result == null || result is not T)
                return default(T);

            return (T)result;
        }

    }
}
