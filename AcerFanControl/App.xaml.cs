using System;
using System.Diagnostics;
using System.Windows;

namespace AcerFanControl
{
    public partial class App
    {
        public App()
        {
            string[] args = Environment.GetCommandLineArgs();
            int delay;
            if (args.Length == 0 || !int.TryParse(args[0], out delay))
                delay = 200;

            string assemblyPath = typeof(App).Assembly.Location;
            string delayStr = TimeSpan.FromSeconds(delay).ToString("\\0\\0mm\\:ss");
            string arguments = $"/create /tn AcerFanControl /sc ONSTART /tr \"'{assemblyPath}' {delay}\" /delay {delayStr} /rl highest /f";
            ProcessStartInfo startInfo = new ProcessStartInfo("schtasks", arguments)
            {
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process proc = Process.Start(startInfo);
            if (proc != null)
            {
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                    MessageBox.Show(proc.StandardError.ReadToEnd());
            }
        }
    }
}
