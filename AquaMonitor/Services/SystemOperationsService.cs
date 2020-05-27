using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AquaMonitor.Data.Context;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Global;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerState = AquaMonitor.Data.Models.PowerState;

namespace AquaMonitor.Web.Services
{
    /// <summary>
    /// Service to track water levels
    /// </summary>
    public class SystemOperationsService : IHostedService, IDisposable
    {
        private int executionCount;
        private readonly ILogger<SystemOperationsService> logger;
        private Timer timer;
        private bool busy;
        private readonly IGlobalState globalData;
        private readonly IPowerRelayService relayService;
        private bool firstRun;
        private readonly AquaDbContext dbContext;
        private DateTime lastHistory = DateTime.MinValue;
        /// <summary>
        /// Service Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="globalData"></param>
        /// <param name="relayService"></param>
        /// <param name="dbContext"></param>
        public SystemOperationsService(ILogger<SystemOperationsService> logger, IGlobalState globalData, IPowerRelayService relayService, AquaServiceDbContext dbContext)
        {
            this.logger = logger;
            this.globalData = globalData;
            this.relayService = relayService;
            this.dbContext = dbContext;
            this.firstRun = true;
        }

        /// <summary>
        /// Starts service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("System Operations Service running.");

            timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            if (!busy)
            {
                busy = true;
                var count = Interlocked.Increment(ref executionCount);

                logger.LogInformation(
                    "System Operations Service is working. Count: {Count}", count);
                ProcessWork(state);
                busy = false;
            }
            else
            {
                var count = executionCount;
                logger.LogInformation(
                    "System Operations Service is busy on Count: {Count}", count);
            }
        }

        /// <summary>
        /// Stops service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("System Operations Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Process work
        /// </summary>
        /// <param name="state"></param>
        private void ProcessWork(object state)
        {

            if(!globalData.SystemOnline)
            {
                if (DateTime.Now.Subtract(lastHistory).TotalSeconds > globalData.DataCollectionRate)
                {
                    lastHistory = DateTime.Now;
                    dbContext?.SaveHistory(new HistoryRecord(globalData));
                }
                logger.LogWarning("The system is currently offline");
                return;
            }
            // perform logic to decide certain systems and when they should turn on and off
            
            // Logic for relay #1
            if(globalData.Relays.Any(t=> t.Letter == RelayLocation.A))
                ProcessRelay(RelayLocation.A, DateTime.Now);
            if (globalData.Relays.Any(t => t.Letter == RelayLocation.B))
                ProcessRelay(RelayLocation.B, DateTime.Now);
            if (globalData.Relays.Any(t => t.Letter == RelayLocation.C))
                ProcessRelay(RelayLocation.C, DateTime.Now);
            if (globalData.Relays.Any(t => t.Letter == RelayLocation.D))
                ProcessRelay(RelayLocation.D, DateTime.Now);
            firstRun = false;
            if (DateTime.Now.Subtract(lastHistory).TotalSeconds > globalData.DataCollectionRate)
            {
                lastHistory = DateTime.Now;
                dbContext?.SaveHistory(new HistoryRecord(globalData));
            }
        }

        /// <summary>
        /// Process RelayA cycle
        /// </summary>
        public int ProcessRelay(RelayLocation relay, DateTime currentTime)
        {
            logger.LogInformation("Processing Operations for Relay '" + relay.ToString() + "'");            
            PowerState currentState;
            // to support unit testing
            if(Math.Abs(DateTime.Now.Subtract(currentTime).TotalSeconds) > 100)
                currentState = globalData.GetRelay(relay).CurrentState;
            else
                currentState = relayService.GetState(relay);

            int shouldABeOn = 0;
            string reason = "";
            // if we have a start time window are we within that window?
            if (globalData.GetRelay(relay).Start.HasValue)
            {
                if (currentTime < globalData.GetRelay(relay).PowerStartToday(currentTime))
                {
                    shouldABeOn = -1;
                    reason += " Too early";
                }
                else
                {
                    shouldABeOn = 1;
                    reason += " Within start time";
                }
            }
            if (globalData.GetRelay(relay).Stop.HasValue && shouldABeOn >= 0)
            {
                if (currentTime > globalData.GetRelay(relay).PowerStopToday(currentTime))
                {
                    shouldABeOn = -1;
                    reason += " Too late";
                }
                else
                {
                    shouldABeOn = 1;
                    reason += " Within end time";
                }
            }


            // temp window
            if (globalData.TemperatureF.Equals(0) && globalData.Humidity.Equals(0) && 
                (globalData.GetRelay(relay).MinTempF.HasValue || globalData.GetRelay(relay).MaxTempF.HasValue))
            {
                // we cannot process this sensor, lets not provide any logic for this and just return
                logger.LogWarning("No temperature data and this relay {0} requres temperature data.", relay.ToString());
                return 0; // indetermined state - don't turn it on or off
            }

            if (globalData.GetRelay(relay).MinTempF.HasValue && shouldABeOn >= 0) // should only be on if a minimum temp is reached
            {
                if ((globalData.TemperatureF <
                    globalData.GetRelay(relay).MinTempF.Value && currentState == PowerState.Off) ||
                    (globalData.TemperatureF + (globalData.GetRelay(relay).TempVariance ?? 3) <=
                    globalData.GetRelay(relay).MinTempF.Value && currentState == PowerState.On))
                {
                    shouldABeOn = -1;
                    reason += " Too cool";
                }
                else
                {
                    shouldABeOn = 1;
                    reason += " Temp has been reached";
                }
            }

            if (globalData.GetRelay(relay).MaxTempF.HasValue && shouldABeOn >= 0) // should only be on if a minimum temp is reached
            {
                if ((globalData.TemperatureF >
                    globalData.GetRelay(relay).MaxTempF.Value && currentState == PowerState.Off) ||
                    (globalData.TemperatureF - (globalData.GetRelay(relay).TempVariance ?? 3) >=
                    globalData.GetRelay(relay).MaxTempF.Value && currentState == PowerState.On))
                {
                    shouldABeOn = -1;
                    reason += " Too hot";
                }
                else
                {
                    shouldABeOn = 1;
                    reason += " Temp not too high";
                }
            }

            // water sensor limiter

            if (globalData.GetRelay(relay).WaterId.HasValue && shouldABeOn >= 0) // should operate based on a water sensor
            {
                var currentWaterState =
                    globalData.WaterLevels.First(t => t.Id == globalData.GetRelay(relay).WaterId.Value).FloatHigh;
                if (globalData.GetRelay(relay).OnWhenFloatHigh && currentWaterState)
                {
                    reason += " Water level correct";
                    shouldABeOn = 1;
                }
                else
                {
                    shouldABeOn = -1;
                    reason += " Water level wrong";
                }
            }

            if (globalData.GetRelay(relay).Interval > 0 && shouldABeOn >= 0)
            {
                // we need to turn this on and off and on and off
                if (globalData.GetRelay(relay).IntervalRun == 0)
                    globalData.GetRelay(relay).IntervalRun = (globalData.GetRelay(relay).Interval / 2); // run for half as long as your interval length

                // lets see which cycle we are in
                var startOfDay = DateTime.Parse(currentTime.ToShortDateString() + " 00:00:00"); // midnight
                TimeSpan timeToday = currentTime.Subtract(startOfDay);
                var runCount = (int)(timeToday.TotalSeconds / globalData.GetRelay(relay).Interval); // number of runs since midnight
                TimeSpan t1 = new TimeSpan(0, 0, globalData.GetRelay(relay).Interval * runCount); // completed runs
                var lastRun = startOfDay.Add(t1);
                if (currentTime.Subtract(lastRun).TotalSeconds < globalData.GetRelay(relay).IntervalRun)
                {
                    reason += " on cycle";
                    shouldABeOn = 1; // on cycle
                }
                else
                {
                    shouldABeOn = -1; // off cycle
                    reason += " off cycle";
                }
            }

            if(shouldABeOn == -1)
                logger.LogInformation("Relay '" + relay.ToString() + "' results in an off state because " + reason);
            else if (shouldABeOn == 1)
                logger.LogInformation("Relay '" + relay.ToString() + "' results in an on state because " + reason);

            
            if ((currentState == PowerState.On && shouldABeOn == 1 ||
                currentState == PowerState.Off && shouldABeOn == -1) && !firstRun)
                return shouldABeOn; // we are good here folks

            // the current state may need to be changed
            if (shouldABeOn == 0) // this should never have reached this state
            {
                logger.LogWarning("The state cannot change for relay '" + relay.ToString() + "' because the shouldbeon value is 0 while the currentState is " + currentState.ToString());
                return shouldABeOn;
            }
            logger.LogInformation("Changing the state of the current relay '" + relay.ToString() + "' to " + (shouldABeOn == 1 ? " on" : " off"));
            try
            {
                relayService.SetState(relay, shouldABeOn == 1 ? PowerState.On : PowerState.Off);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed changing state of the current relay '" + relay.ToString() + "' to " + (shouldABeOn == 1 ? " on - " : " off - ") + ex.Message);
            }
            return shouldABeOn;
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
