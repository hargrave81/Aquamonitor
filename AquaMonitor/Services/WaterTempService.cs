using System;
using System.Threading;
using System.Threading.Tasks;
using AquaMonitor.Data.Context;
using AquaMonitor.Data.Models;
using Iot.Device.OneWire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace AquaMonitor.Web.Services
{
    /// <summary>
    /// Atmospheric information
    /// </summary>
    public class WaterTempService : IHostedService, IDisposable
    {
        private int executionCount;
        private readonly ILogger<WaterTempService> logger;
        private Timer timer;
        private bool busy;
        private readonly IGlobalState globalData;
        private int cyclesSinceWorking;
        private readonly Random random;
        private readonly AquaServiceDbContext dbContext;

        /// <summary>
        /// Service Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="globalData"></param>
        /// <param name="dbContext"></param>
        public WaterTempService(ILogger<WaterTempService> logger, IGlobalState globalData, AquaServiceDbContext dbContext)
        {
            this.logger = logger;
            this.globalData = globalData;
            this.dbContext = dbContext;
            random = new Random();
        }

        /// <summary>
        /// Starts service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("WaterTemp Service running.");

            timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(600 + random.NextDouble())); // this ensures the .net timer issues with DHT  sensors isn't constantly hit
            
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            while (!globalData.SettingsLoaded)
            {
                return; // cannot work without settings
            }
            if (!busy)
            {
                if (globalData.More.WaterSensorEnabled)
                {
                    busy = true;
                    var count = Interlocked.Increment(ref executionCount);
                    logger.LogInformation(
                        "WaterTemp Service is working. Count: {Count}", count);
                    try
                    {

                        Task.Run(() => ProcessWork(state));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Failed to process WaterTemp: " + ex.Message);
                    }
                    busy = false;
                }
            }
            else
            {
                var count = executionCount;
                logger.LogInformation(
                    "WaterTemp Service is busy on Count: {Count}", count);
            }
        }

        /// <summary>
        /// Stops service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("WaterTemp Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        
        /// <summary>
        /// Process work
        /// </summary>
        /// <param name="state"></param>
        private async Task ProcessWork(object state)
        {
            cyclesSinceWorking++;
            logger.LogInformation("Processing w1 BUS ...");

            OneWireBus sensor = null;
            foreach (var bus in OneWireBus.EnumerateBusIds())
            {
                logger.LogInformation("Bus Found: " + bus);
                sensor = new OneWireBus(bus);
                break;
            }
            await sensor.ScanForDeviceChangesAsync();
            foreach (string devId in sensor.EnumerateDeviceIds())
            {
                logger.LogInformation("Found device: " + devId);
                if (OneWireThermometerDevice.IsCompatible(sensor.BusId, devId))
                {
                    OneWireThermometerDevice devTemp = new OneWireThermometerDevice(sensor.BusId,devId);
                    var temp = (await devTemp.ReadTemperatureAsync()).DegreesFahrenheit;
                    logger.LogInformation(temp.ToString("F2") + "\u00B0C");
                    await dbContext.Readings.AddAsync(new WaterTempReading() { Location = "Probe", Taken = DateTime.Now, Value = temp});
                    globalData.WaterTemp = (float)temp; // set to current temp
                    cyclesSinceWorking = 0;
                    break; // only read one sensor currently
                }
            }
            if(cyclesSinceWorking > 5)
                logger.LogWarning("The sensor was unable to be read at this time on port {0}.", globalData.TempPin);
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
