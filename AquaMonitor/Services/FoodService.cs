using System;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Devices;
using Iot.Device.Uln2003;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AquaMonitor.Web.Services
{
    /// <summary>
    /// Food Processor
    /// </summary>
    public class FoodService : IHostedService, IDisposable
    {
        private int executionCount;
        private readonly ILogger<FoodService> logger;
        private Timer timer;
        private bool busy;
        private readonly IGlobalState globalData;
        private readonly Random random;
        private bool stopFood = false;

        /// <summary>
        /// Service Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="globalData"></param>
        public FoodService(ILogger<FoodService> logger, IGlobalState globalData)
        {
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
            
            logger.LogInformation("Food Service running.");

            timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(30));
        
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            logger.LogInformation("Starting Food Run ...");
            while (!globalData.SettingsLoaded)
            {
                return; // cannot work without settings
            }

            if (globalData.More.FoodSessions == null || !globalData.More.FoodSessions.Any())
            {
                logger.LogWarning("No Food Detected!");
                return; // no food sessions to process
            }

            if (!busy)
            {
                busy = true;
                var count = Interlocked.Increment(ref executionCount);

                logger.LogInformation(
                    "Food Service is working. Count: {Count}", count);
                try
                {
                    ProcessWork(state);
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to process Food: " + ex.Message);
                }

                busy = false;
            }
            else
            {
                var count = executionCount;
                logger.LogInformation(
                    "Food Service is busy on Count: {Count}", count);
            }
        }

        /// <summary>
        /// Stops service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Food Service is stopping.");

            stopFood = true;

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        
        /// <summary>
        /// Process work
        /// </summary>
        /// <param name="state"></param>
        private void ProcessWork(object state)
        {
            var clock = DateTime.Now.TimeOfDay;
            int index = 1;
            foreach (var entry in globalData.More.FoodSessions)
            {
                if (entry.StartTime.HasValue && entry.TurnTime > 0)
                {
                    logger.LogInformation($"Checking Food Run #{index} {entry.StartTime.ToString()}");
                    if (entry.StartTime < clock && entry.LastRan.Date < DateTime.Now.Date)
                    {
                        // we need to process this food
                        entry.LastRan = DateTime.Now;
                        Task.Run(() => SpillFood(entry.TurnTime, entry.PinCollection));
                    }
                }

                index++;
            }
        }

        /// <summary>
        /// Spills food
        /// </summary>
        /// <param name="turnTime"></param>
        /// <param name="pinCollection"></param>
        /// <returns></returns>
        private async Task SpillFood(int turnTime, string pinCollection)
        {
            logger.LogInformation($"Dumping food for {turnTime} on {pinCollection}");
            var pins = pinCollection.Split(',');
            var pin0 = int.Parse(pins[0]);
            var pin1 = .5d;
            var pin2 = 400;
            if (pins.Length > 1)
                pin1 = double.Parse(pins[1]);
            if (pins.Length > 2)
                pin2 = int.Parse(pins[2]);
            var pin4 = 999;
            if (pins.Length > 3)
                pin4 = int.Parse(pins[3]);
            var pin5 = 1900;
            if (pins.Length > 4)
                pin5 = int.Parse(pins[4]);
            using var pwm = new System.Device.Pwm.Drivers.SoftwarePwmChannel(pin0, pin2, pin1);
            //new pwm(pin0,pin2,pin1,true)
            using var servo = new Iot.Device.ServoMotor.ServoMotor(pwm);
            servo.Start();
            DateTime nowTime = DateTime.Now.AddMilliseconds(turnTime);
            servo.WritePulseWidth(pin4);
            while (DateTime.Now < nowTime)
            {
                if (stopFood)
                    break;
                System.Threading.Thread.Sleep(500);
            }
            servo.WritePulseWidth(pin5);
            servo.Stop();
            logger.LogInformation("Food dumped!");
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
