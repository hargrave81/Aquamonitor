using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Threading;
using System.Threading.Tasks;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Global;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AquaMonitor.Web.Services
{
    /// <summary>
    /// Service to track water levels
    /// </summary>
    public class WaterLevelService : IHostedService, IDisposable
    {
        private int executionCount;
        private readonly ILogger<WaterLevelService> logger;
        private readonly GpioController controller;
        private readonly bool enabled;
        private Timer timer;
        private bool busy;
        private readonly IGlobalState globalData;

        /// <summary>
        /// Service Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="globalData"></param>
        public WaterLevelService(ILogger<WaterLevelService> logger, IGlobalState globalData)
        {
            this.logger = logger;
            this.globalData = globalData;
            try
            {
                this.controller = new GpioController(PinNumberingScheme.Logical, new RaspberryPi3Driver());
                enabled = true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to initialize GPIO for Relays - " + ex.Message);
            }
        }

        /// <summary>
        /// Starts service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Water Level Service running.");

            timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            if (!busy)
            {
                busy = true;
                var count = Interlocked.Increment(ref executionCount);

                logger.LogInformation(
                    "Water Level Service is working. Count: {Count}", count);
                ProcessWork(state);
                busy = false;
            }
            else
            {
                var count = executionCount;
                logger.LogInformation(
                    "Water Level Service is busy on Count: {Count}", count);
            }
        }

        /// <summary>
        /// Stops service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Water Level Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Process work
        /// </summary>
        /// <param name="state"></param>
        private void ProcessWork(object state)
        {
            if (!enabled)
                return;
            foreach(var water in globalData.WaterLevels)
            {
                if(water.Pin != 0)
                {
                    try 
                    {
                        if (!controller.IsPinOpen(water.Pin))
                        {
                            // lets open the pin
                            controller.OpenPin(water.Pin, PinMode.InputPullDown);
                        }                        
                        var result = controller.Read(water.Pin);
                        water.FloatHigh = result == PinValue.Low;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Failed to read {0} pin #: {1} - {2}",water.Name, water.Pin, ex.Message);
                    }
                }
            }
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
