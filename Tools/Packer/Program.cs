using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.IO.Hashing;

namespace Packer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Split target file into chunks
            // encrypt each chunk using a 256 bit hashed byte array
            // write each chunk to a new file

            // old versions
            var oldFiles = new string[] {
                "G:\\SteamLibrary\\steamapps\\common\\F4Old\\Fallout4Launcher.exe",
                "G:\\SteamLibrary\\steamapps\\common\\F4Old\\steam_api64.dll",
                "G:\\SteamLibrary\\steamapps\\common\\F4Old\\Fallout4.exe"
            };

            var newFiles = new string[]
            {
                "G:\\SteamLibrary\\steamapps\\common\\F4New\\Fallout4Launcher.exe",
                "G:\\SteamLibrary\\steamapps\\common\\F4New\\steam_api64.dll",
                "G:\\SteamLibrary\\steamapps\\common\\F4New\\Fallout4.exe"
            };

            var outputDir = "G:\\SteamLibrary\\steamapps\\common\\F4Patch";
            for (var i = 0; i < oldFiles.Length; i++)
            {
                var fileToBeReplaced = newFiles[i];
                var fileToBePacked = oldFiles[i];
                byte[] hash = null;
                using (var stream = File.OpenRead(fileToBeReplaced))
                {
                    hash = GetChecksumBuffered(stream);
                }
                var id = Crc64.HashToUInt64(File.ReadAllBytes(fileToBeReplaced));

                System.IO.File.WriteAllText(
                    "G:\\GitHub\\fallout4-downgrader\\publish\\Self-contained\\Assets\\Keys\\" +
                    Path.GetFileName(fileToBePacked) + ".key", id + "\t" + Convert.ToBase64String(hash));

                SplitFile(fileToBePacked, id, hash, outputDir);
                //RebuildFile(fileToBeReplaced, hash);
            }
        }

        private static void RebuildFile(string fileToBeReplaced, ulong id, byte[] hash)
        {
            var patches = Directory
                .GetFiles(".", id + "_*.patch")
                .OrderBy(x => int.Parse(x.Split('_')[1].Split('.')[0])).ToArray();

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

        private static void SplitFile(string fileToBePacked, ulong id, byte[] hash, string outputDir = null)
        {
            var fi = new FileInfo(fileToBePacked);

            // determine how big chunks we can split into
            // if size is less than max chunk size then we can take it as it is.
            const long MaxChunkSize = 1024 * 1000 * 20; // 10 mb

            var patches = new List<FileChunk>();
            var dataLeft = fi.Length;
            var index = 0;

            do
            {
                var size = MaxChunkSize;
                if (dataLeft <= MaxChunkSize)
                {
                    size = dataLeft;
                    dataLeft = 0;
                }
                else
                {
                    dataLeft -= MaxChunkSize;
                }
                patches.Add(new FileChunk
                {
                    Index = index++,
                    Length = size,
                });
            } while (dataLeft > 0);

            using (var stream = File.OpenRead(fileToBePacked))
            {
                for (var i = 0; i < patches.Count; ++i)
                {
                    var chunk = patches[i];
                    var buffer = new byte[chunk.Length];
                    stream.Read(buffer, 0, (int)chunk.Length);

                    var toWrite = Encrypt(buffer, hash);
                    var name = id + "_" + i + ".patch";
                    var chunkFile = outputDir != null ? System.IO.Path.Combine(outputDir, name) : name;

                    if (File.Exists(chunkFile))
                        File.Delete(chunkFile);

                    using (var write = File.OpenWrite(chunkFile))
                    //using (var gzip = new GZipStream(write, CompressionLevel.SmallestSize))
                    {
                        write.Write(toWrite, 0, toWrite.Length);
                    }
                }
            }
        }

        public static byte[] Encrypt(byte[] value, byte[] key)
        {
            if (!(key.Length == 16 || key.Length == 24 || key.Length == 32))
                throw new ArgumentException("Key size is not valid for AES. Key must be either 128, 192, or 256 bits.");

            using (var aesAlg = System.Security.Cryptography.Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.GenerateIV();
                aesAlg.Mode = CipherMode.CBC;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    // Writing IV before the actual data
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(value, 0, value.Length);
                        csEncrypt.FlushFinalBlock();
                    }

                    return msEncrypt.ToArray();
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

    }

    public class FileChunk
    {
        public int Index;
        public long Length;
    }

}
