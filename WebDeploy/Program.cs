using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace WebDeploy
{
    /// <summary>
    /// Web AutoDeploy Program
    /// </summary>
    class Program
    {


        private static Timer timer;
        private static Timer webTimer;
        private static Settings settings;
        private static DateTime lastGrab = DateTime.MinValue;
        private static DateTime lastGrab2 = DateTime.MinValue;
        private static bool appAlive = true;
        private static bool busy;

        static void Main(string[] args)

        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(),"appsettings.json")))
            {
                Console.WriteLine("Failed to locate settings file");
                Environment.ExitCode = -90;
                return;
            }
            settings = System.Text.Json.JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")));
            Console.WriteLine("Monitoring Web Deploy Path {0} to destination {1}", settings.src, settings.dest);
            Console.WriteLine($"AutoUpdate {(settings.autoUpdateFromWeb ? "Enabled": "Disabled")}");
            
            if (settings.src.EndsWith("/"))
                settings.src = settings.src.Substring(0, settings.src.Length - 1);
            if (settings.dest.EndsWith("/"))
                settings.dest = settings.dest.Substring(0, settings.dest.Length - 1);
            if (!settings.src.StartsWith("/"))
                settings.src = "/" + settings.src;
            if (!settings.dest.StartsWith("/"))
                settings.dest = "/" + settings.dest;
            timer = new Timer(10000);
            timer.Start();
            timer.Elapsed += Timer_Elapsed;

            if (settings.autoUpdateFromWeb)
            {
                // add web timer
                webTimer = new Timer(300000);
                webTimer.Start();
                webTimer.Elapsed += WebTimer_Elapsed;
            }

            AppDomain.CurrentDomain.ProcessExit += (s, e) => { appAlive = false; }; // allow system to gracefully exit

            while (appAlive)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (busy)
                return;
            busy = true;
            try
            {
                // check and see if new files were deployed in destination folder
                FileInfo sourceFi = new FileInfo(Path.Combine(settings.src + "/AquaMonitor.zip"));
                FileInfo sourceFi2 = new FileInfo(Path.Combine(settings.src + "/AquaMonitor32.zip"));
                if (lastGrab == DateTime.MinValue)
                {
                    // we do not know whats newer, so lets just track and bounce
                    lastGrab = sourceFi.CreationTimeUtc;
                }

                if (lastGrab2 == DateTime.MinValue)
                {
                    // we do not know whats newer, so lets just track and bounce
                    lastGrab2 = sourceFi2.CreationTimeUtc;
                }

                if (sourceFi.CreationTimeUtc > lastGrab)
                {
                    timer.Enabled = false;
                    lastGrab = sourceFi.CreationTimeUtc;
                    System.Threading.Thread.Sleep(30000); // wait for the file transfer to finish
                    Console.WriteLine("Loading new version ...");
                    // we have a new deploy

                    Bash("systemctl stop kestrel-aquamonitor.service");
                    System.Threading.Thread.Sleep(10000);
                    Console.WriteLine("Unpacking new version ...");
                    Bash($"unzip -o {settings.src}/AquaMonitor.zip -d {settings.dest}");
                    Console.WriteLine("Restarting new version ...");
                    Bash($"systemctl start kestrel-aquamonitor.service");
                    timer.Enabled = true;
                }

                if (sourceFi2.CreationTimeUtc > lastGrab2)
                {
                    timer.Enabled = false;
                    lastGrab2 = sourceFi2.CreationTimeUtc;
                    System.Threading.Thread.Sleep(30000); // wait for the file transfer to finish
                    Console.WriteLine("Loading new 32bit version ...");
                    // we have a new deploy

                    Bash("systemctl stop kestrel-aquamonitor.service");
                    System.Threading.Thread.Sleep(10000);
                    Console.WriteLine("Unpacking new version ...");
                    Bash($"unzip -o {settings.src}/AquaMonitor32.zip -d {settings.dest}");
                    Console.WriteLine("Restarting new version ...");
                    Bash($"systemctl start kestrel-aquamonitor.service");
                    timer.Enabled = true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed to update: "+ ex.Message);
            }
            busy = false;
        }

        private static void WebTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            webTimer.Enabled = false;
            Console.WriteLine("Testing github web files ...");
            try
            {
                FileInfo sourceFi = new FileInfo(Path.Combine(settings.src + "/AquaMonitor.zip"));
                FileInfo sourceFi2 = new FileInfo(Path.Combine(settings.src + "/AquaMonitor32.zip"));
                Task.Run(async () =>
                {
                    if (IntPtr.Size == 4)
                    {
                        // 32-bit
                        var am32 = await GitHubCommit.Fetch("AquaMonitor32.zip");
                        if (am32 != null && am32.Commit.Author.Date > sourceFi2.CreationTimeUtc)
                        {
                            Console.WriteLine("Updating 32 bit software ...");
                            busy = true;
                            timer.Enabled = false;
                            System.Threading.Thread.Sleep(500);
                            // we need to update the software
                            try
                            {
                                await GitHubCommit.Download("AquaMonitor32.zip",
                                    Path.Combine(settings.src + "/AquaMonitor32.zip"));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            timer.Enabled = true;
                        }
                    }
                    else if (IntPtr.Size == 8)
                    {
                        // 64-bit
                        var am64 = await GitHubCommit.Fetch("AquaMonitor.zip");
                        if (am64 != null && am64.Commit.Author.Date > sourceFi.CreationTimeUtc)
                        {
                            Console.WriteLine("Updating 64 bit software ...");
                            busy = true;
                            // we need to update the software
                            timer.Enabled = false;
                            // we need to update the software
                            System.Threading.Thread.Sleep(500);
                            try
                            {
                                await GitHubCommit.Download("AquaMonitor.zip",
                                    Path.Combine(settings.src + "/AquaMonitor.zip"));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            timer.Enabled = true;
                        }
                    }

                    webTimer.Enabled = true;
                });
            }
            catch (Exception ext)
            {
                Console.WriteLine("Failed to process github: " + ext.Message);
            }
        }

        private static void Bash(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

    }
}
