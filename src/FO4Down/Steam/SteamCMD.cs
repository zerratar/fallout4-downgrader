using System;
using System.Diagnostics;
using System.Reflection;

namespace FO4Down.Steam
{
    public class SteamCMD : IDisposable
    {
        private Process process;
        //private Queue<string> outReadQueue = new Queue<string>();
        //private Queue<string> outErrorQueue = new Queue<string>();

        public static void DownloadDepot(int appid, int depotid, long manifestid, Action<string> onData, Action onComplete)
        {
            RunCommand($"+download_depot {appid} {depotid} {manifestid} +quit", onData, onComplete);
        }

        public static void RunCommand(string command, Action<string> onData, Action onComplete)
        {
            var workingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Steam");
            var startInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = command,
                FileName = Path.Combine(workingDirectory, "steamcmd.exe"),
                WorkingDirectory = workingDirectory
            };
            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.ErrorDataReceived += (s, e) =>
                {
                    onData(e.Data);
                };
                process.OutputDataReceived += (s, e) =>
                {
                    onData(e.Data);
                };

                process.WaitForExit();
                onComplete();
                return;
            }
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        //public event DataReceivedEventHandler OutputDataReceived;
        //public event DataReceivedEventHandler ErrorDataReceived;

        public Action<string> OnData { get; set; }

        public void Start()
        {
            var workingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Steam");
            var startInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = Path.Combine(workingDirectory, "steamcmd.exe"),
                WorkingDirectory = workingDirectory
            };

            process = new Process();
            process.StartInfo = startInfo;
            process.OutputDataReceived += OutputDataReceived;
            process.ErrorDataReceived += ErrorDataReceived;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnData?.Invoke(e.Data);
            //outErrorQueue.Enqueue(e.Data);
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnData?.Invoke(e.Data);
            //outReadQueue.Enqueue(e.Data);
        }

        public void SendCommand(string command)
        {
            process.StandardInput.WriteLine(command);
            process.StandardInput.Flush();
        }

        public void Stop()
        {
            if (process.HasExited)
            {
                process = null;
                return;
            }

            try
            {
                process.Close();
            }
            catch { }
            try { process.Dispose(); }
            catch { }

            process = null;
        }


        public void Dispose()
        {
            Stop();
        }

    }
}
