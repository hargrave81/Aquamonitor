using AquaMonitor.Data.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AquaMonitor.Web.Services
{
    /// <summary>
    /// Weather Service
    /// </summary>
    public class WeatherServices : IHostedService, IDisposable
    {
        const string serviceUrl = "http://api.openweathermap.org/data/2.5/weather?zip={0},{1}&appid={2}&units=metric";

        private int executionCount;
        private readonly ILogger<WeatherServices> logger;
        private Timer timer;
        private bool busy;
        private readonly IGlobalState globalData;
        private readonly Random random;
        private readonly HttpClient thisClient;
        /// <summary>
        /// Service Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="globalData"></param>
        public WeatherServices(ILogger<WeatherServices> logger, IGlobalState globalData)
        {
            this.logger = logger;
            this.globalData = globalData;
            random = new Random();
            thisClient = new HttpClient();
        }

        /// <summary>
        /// Starts service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Weather Service running.");

            timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1 + random.NextDouble())); // this ensures the .net timer issues with DHT  sensors isn't constantly hit

            return Task.CompletedTask;
        }

        async private void DoWork(object state)
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
                    "Weather Service is working. Count: {Count}", count);
                try
                {
                    await ProcessWork(state);
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
                    "Weather Service is busy on Count: {Count}", count);
            }
        }

        /// <summary>
        /// Stops service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Weather Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Process work
        /// </summary>
        /// <param name="state"></param>
        private async Task ProcessWork(object state)
        {
            try
            {
                if (string.IsNullOrEmpty(globalData.Zipcode) || string.IsNullOrEmpty(globalData.APIKey))
                {
                    logger.LogInformation("Cannot perform weather services no API Key.");
                    return;
                }
                var result = await thisClient.GetStringAsync(string.Format(serviceUrl, globalData.Zipcode, globalData.Country, globalData.APIKey));
                var weatherResult = System.Text.Json.JsonSerializer.Deserialize<Models.WeatherResult>(result);
                this.globalData.CloudCoverage = weatherResult.Clouds.All;
                this.globalData.OutsideHumidity = weatherResult.Main.Humidity;
                this.globalData.OutsideTempC = weatherResult.Temp.DegreesCelsius;
                this.globalData.OutsideTempF = weatherResult.Temp.DegreesFahrenheit;
                this.globalData.Rain = weatherResult.Weather.Any(t => t.Main == "Rain");
                this.globalData.WindSpeed = weatherResult.Wind.SpeedMph();
                this.globalData.Sunrise = weatherResult.Sys.Sunrise;
                this.globalData.Sunset = weatherResult.Sys.Sunset;
                this.globalData.WeatherIcon = weatherResult.Weather.First().Icon;
            }
            catch (Exception ex)
            {
                logger.LogError("could not get request from weather service: " + ex.Message);
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
