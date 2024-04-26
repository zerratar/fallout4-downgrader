using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Fallout4Downgrader
{
    public class SteamCMD : IDisposable
    {
        private Process process;
        private Queue<string> outReadQueue = new Queue<string>();
        private Queue<string> outErrorQueue = new Queue<string>();

        //public event DataReceivedEventHandler OutputDataReceived;
        //public event DataReceivedEventHandler ErrorDataReceived;
        public void Start()
        {
            var workingDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "Steam");
            var startInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = System.IO.Path.Combine(workingDirectory, "steamcmd.exe"),
                WorkingDirectory = workingDirectory
            };

            this.process = new Process();
            process.StartInfo = startInfo;
            process.OutputDataReceived += OutputDataReceived;
            process.ErrorDataReceived += ErrorDataReceived;

            process.Start();
            process.BeginOutputReadLine();
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            outErrorQueue.Enqueue(e.Data);
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            outReadQueue.Enqueue(e.Data);
        }

        public string ReadLine(int timeout = int.MaxValue)
        {
            int waited = 0;
            while (waited < timeout)
            {
                if (outReadQueue.Count > 0)
                    return outReadQueue.Dequeue();

                System.Threading.Thread.Sleep(10);
                waited += 10;
            }
            return null;
        }

        public void SendCommand(string command)
        {
            process.StandardInput.WriteLine(command);
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
