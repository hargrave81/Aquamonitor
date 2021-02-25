using System;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Devices;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.DHTxx;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace AquaMonitor.Web.Services
{
    /// <summary>
    /// Atmospheric information
    /// </summary>
    public class AtmosphereService : IHostedService, IDisposable
    {
        private int executionCount;
        private readonly ILogger<AtmosphereService> logger;
        private Timer timer;
        private bool busy;
        private readonly IGlobalState globalData;
        private int cyclesSinceWorking;
        private readonly Random random;
        private bool i2c;
        private I2cDevice bmeI2c;
        
        private int CurrentSensor;

        /// <summary>
        /// Service Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="globalData"></param>
        public AtmosphereService(ILogger<AtmosphereService> logger, IGlobalState globalData)
        {
            i2c = false;
            this.logger = logger;
            this.globalData = globalData;
            random = new Random();
        }

        /// <summary>
        /// Starts service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Atmosphere Service running.");

            timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(6 + random.NextDouble())); // this ensures the .net timer issues with DHT  sensors isn't constantly hit
            
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
                busy = true;
                var count = Interlocked.Increment(ref executionCount);

                logger.LogInformation(
                    "Atmosphere Service is working. Count: {Count}", count);
                try
                {
                    ProcessWork(state);
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to process Atmosphere: " + ex.Message);
                }

                busy = false;
            }
            else
            {
                var count = executionCount;
                logger.LogInformation(
                    "Atmosphere Service is busy on Count: {Count}", count);
            }
        }

        /// <summary>
        /// Stops service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Atmosphere Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        
        /// <summary>
        /// Process work
        /// </summary>
        /// <param name="state"></param>
        private void ProcessWork(object state)
        {
            cyclesSinceWorking++;
            double h;
            Temperature tc;

            if (CurrentSensor != globalData.TempType)
            {
                // clean up any current sensor data
                if (CurrentSensor != 0)
                {
                    if (CurrentSensor == 21)
                    {
                        // clean up the BUS
                        Htu21D.DisposeI2C();   
                    }

                    if (CurrentSensor == 28)
                    {
                        // clean up the BUS
                        bmeI2c.Dispose();
                        bmeI2c = null;
                    }
                }
            }

            CurrentSensor = globalData.TempType;

            if (globalData.TempType == 11)
            {
                using var tempSensor = new Dht11(globalData.TempPin);
                h = tempSensor.Humidity.Value;
                if (tempSensor.IsLastReadSuccessful)
                {
                    cyclesSinceWorking = 0;
                    logger.LogInformation("Port {0} detected information and is being tracked.", globalData.TempPin);
                    globalData.Humidity = h;                    
                    tc = tempSensor.Temperature;
                    if (tempSensor.IsLastReadSuccessful)
                    {
                        if (globalData.More?.TempOffset != null)
                            tc = Temperature.FromDegreesFahrenheit(tc.DegreesFahrenheit - globalData.More.TempOffset.Value);
                        globalData.TemperatureC = tc.DegreesCelsius;
                        globalData.TemperatureF = tc.DegreesFahrenheit;
                    }
                }
            }
            else if(globalData.TempType == 22)
            {
                using var tempSensor = new Dht22(globalData.TempPin);
                h = tempSensor.Humidity.Value;
                if (tempSensor.IsLastReadSuccessful)
                {
                    cyclesSinceWorking = 0;
                    logger.LogInformation("Port {0} detected information and is being tracked.", globalData.TempPin);
                    globalData.Humidity = h;
                    tc = tempSensor.Temperature;
                    if (tempSensor.IsLastReadSuccessful)
                    {
                        if (globalData.More?.TempOffset != null)
                            tc = Temperature.FromDegreesFahrenheit(tc.DegreesFahrenheit - globalData.More.TempOffset.Value);
                        globalData.TemperatureC = tc.DegreesCelsius;
                        globalData.TemperatureF = tc.DegreesFahrenheit;
                    }
                }
            }
            else if(globalData.TempType == 21)
            {
                this.i2c = true;
                using var htu = Htu21D.CreateDevice(globalData.TempPin, I2cAddress.AddrLow, Resolution.Medium);
                if (htu == null)
                {
                    logger.LogError("Failed to create temp sensor");
                }
                else
                {
                    h = htu.Humidity;
                    if (htu.IsLastReadSuccessful)
                    {
                        cyclesSinceWorking = 0;
                        logger.LogInformation("I2C detected humid information and is being tracked.");
                        globalData.Humidity = h;
                        Thread.Sleep(100);
                        tc = htu.Temperature;
                        if (htu.IsLastReadSuccessful)
                        {
                            if (globalData.More?.TempOffset != null)
                                tc = Temperature.FromDegreesFahrenheit(
                                    tc.DegreesFahrenheit - globalData.More.TempOffset.Value);
                            logger.LogInformation("I2C detected temp information and is being tracked.");
                            globalData.TemperatureC = tc.DegreesCelsius;
                            globalData.TemperatureF = tc.DegreesFahrenheit;
                        }
                    }
                }
            }
            else if(globalData.TempType == 28)
            {
                this.i2c = true;
                bool firstGo = false;
                if (bmeI2c == null)
                {
                    firstGo = true;
                    bmeI2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x76));
                }
                using var bme = new Bme280(bmeI2c);
                if (firstGo)
                {
                    bme.SetPowerMode(Bmx280PowerMode.Normal);
                }
                bme.HumiditySampling = Sampling.HighResolution;
                bme.TemperatureSampling = Sampling.HighResolution;
                bme.FilterMode = Bmx280FilteringMode.X4;
                Thread.Sleep(10);
                bool success = bme.TryReadHumidity(out var hh);
                if(success)
                {
                    cyclesSinceWorking = 0;
                    logger.LogInformation("I2C detected humid information and is being tracked.");
                    globalData.Humidity = hh.Value;
                    Thread.Sleep(50);
                    success = bme.TryReadTemperature(out tc);
                    if (success)
                    {
                        if(globalData.More?.TempOffset != null)
                            tc = Temperature.FromDegreesFahrenheit(tc.DegreesFahrenheit - globalData.More.TempOffset.Value);
                        logger.LogInformation("I2C detected temp information and is being tracked.");
                        globalData.TemperatureC = tc.DegreesCelsius;
                        globalData.TemperatureF = tc.DegreesFahrenheit;
                    }                    
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
            if(this.i2c)
                Htu21D.DisposeI2C(); // clean up the I2C
            timer?.Dispose();
        }
    }


}
