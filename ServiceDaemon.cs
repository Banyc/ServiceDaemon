using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ServiceDaemon
{
    public static class OperatingSystem
    {
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    public class ServiceDaemonOption
    {
        public TimeSpan CoolDownSpan { get; set; } = TimeSpan.FromSeconds(3);
        public TimeSpan RestartSpan { get; set; }
        // only concerning problem when running the process
        public TimeSpan RetrySpan { get; set; }
    }

    public class ServiceDaemon
    {
        Process process = null;
        ServiceDaemonOption option;

        // TODO: make it works
        private void ProcessDown(object sender, EventArgs e) 
        {
            // ResumeProcessAsync().Wait();
            Console.WriteLine("[INFO] Process is down");
        }

        private async Task ResumeProcessAsync()
        {
            Console.WriteLine("[INFO] Cooling down");
            await Task.Delay(this.option.CoolDownSpan);
            await LaunchAsync();
        }

        private void ActualLaunchOperation()
        {
            string script = File.ReadAllText("./script.sh").Trim();
            string cwd = File.ReadAllText("./cwd.txt").Trim();

            string cmd = script.Substring(0, script.IndexOf(' '));
            string argument = script.Substring(script.IndexOf(' ') + 1);;
            
            if (process != null)
                this.process.Exited -= ProcessDown;
            this.process = Execute(cwd, cmd, argument);
            this.process.Exited += ProcessDown;
        }

        private async Task LaunchAsync()
        {
            bool isSucceeded = false;
            while (!isSucceeded)
            {
                try
                {
                    Console.WriteLine("[INFO] Launch process");
                    ActualLaunchOperation();
                    isSucceeded = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.Delay(this.option.RetrySpan);
                }
            }
        }

        public async Task StartAsync(ServiceDaemonOption option)
        {
            this.option = option;
            await LaunchAsync();
            while (true)
            {
                await Task.Delay(this.option.RestartSpan);
                await RestartAsync();
                // Restart();
            }
        }

        private async Task RestartAsync()
        // private void Restart()
        {
            Console.WriteLine("[INFO] Stop process");
            this.process.Kill();
            this.process.WaitForExit();
            // this.process.CloseMainWindow();
            // this.process.StandardInput.Close();
            this.process.Close();
            await ResumeProcessAsync();
        }

        public static Process Execute(string cwd, string cmd, string argument)
        {
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = cmd;
            info.Arguments = argument;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.WorkingDirectory = cwd;

            p.StartInfo = info;
            p.Start();

            return p;
        }
    }
}
