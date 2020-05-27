using System.Text;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AquaMonitor.Web.Services
{
    /// <summary>
    /// Service to maintain network health
    /// </summary>
    public class NetworkHealthService : IHostedService, IDisposable
    {
        private int executionCount;
        private readonly ILogger<NetworkHealthService> logger;
        private Timer timer;
        private bool busy;
        
        /// <summary>
        /// Service Constructor
        /// </summary>
        /// <param name="logger"></param>
        public NetworkHealthService(ILogger<NetworkHealthService> logger)
        {
            this.logger = logger;                        
        }

        /// <summary>
        /// Starts service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Network Health Service running.");

            timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            if (!busy)
            {
                busy = true;
                var count = Interlocked.Increment(ref executionCount);

                logger.LogInformation(
                    "Network Health Service is working. Count: {Count}", count);
                ProcessWork(state);
                busy = false;
            }
            else
            {
                var count = executionCount;
                logger.LogInformation(
                    "Network Health Service is busy on Count: {Count}", count);
            }
        }

        /// <summary>
        /// Stops service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Network Health Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Process work
        /// </summary>
        /// <param name="state"></param>
        private void ProcessWork(object state)
        {
            if(PingNetwork())
                return;
            logger.LogWarning("The system appears to have a network connection issue attempting to fix ...");
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo("/etc/wifi.sh");
                var proc = System.Diagnostics.Process.Start(startInfo);
                if (proc == null)
                {
                    logger.LogError("Could not locate wifi script.");
                    return;
                }
                proc.WaitForExit();
                Thread.Sleep(30); // wait 30 seconds before we try the network again
                if (PingNetwork())
                {
                    logger.LogInformation("The network connection has been restored!");
                    return;
                }
                logger.LogWarning("The system still appears to have a network connection issue ...");
                Thread.Sleep(30); // wait 30 seconds before we try the network again
                if (PingNetwork())
                {
                    logger.LogInformation("The network connection has been restored!");
                    return;
                }
                logger.LogWarning("The system still appears to have a network connection issue ...");
                Thread.Sleep(30); // wait 30 seconds before we try the network again
                if (PingNetwork())
                {
                    logger.LogInformation("The network connection has been restored!");
                    return;
                }
                logger.LogWarning("The system still appears to have a network connection issue ...");
                Thread.Sleep(30); // wait 30 seconds before we try the network again
                                  // if this isn't long enough, allow the system to attempt this fix "again"
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to fix wifi: {0}", ex.Message);
            }
        }

        private bool PingNetwork() {
            try
            {
                var p = new Ping();
                var pOptions = new PingOptions()
                {
                    DontFragment = true
                };
                byte[] buffer = Encoding.ASCII.GetBytes("a quick sample ping");
                logger.LogDebug("Pinging google's name server");
                var reply = p.Send("8.8.8.8", 2000, buffer, pOptions);
                if (reply == null)
                {
                    return false; // major error
                }

                if (reply.Status == IPStatus.Success)
                {
                    // we are still alive move along, these are not the droids you are looking for
                    return true;
                }

                return false;
            }
            catch
            {
                // we do not care why it failed just that it failed
            }
            return false;
        }

        /// <summary>
        /// Dispose Service
        /// </summary>
        public void Dispose()
        {
            timer?.Dispose();
        }
    }


}
