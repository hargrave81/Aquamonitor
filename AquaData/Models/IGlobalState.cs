using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace AquaMonitor.Data.Models
{

    /// <summary>
    /// Global State Interface
    /// </summary>
    public interface IGlobalState : IAppSetting
    {
        /// <summary>
        /// Current humidity level
        /// </summary>
        double Humidity { get; set; }
        /// <summary>
        /// Current Temperature in Celsius
        /// </summary>
        double TemperatureC { get; set; }
        /// <summary>
        /// Current Temperature in Fahrenheit
        /// </summary>
        double TemperatureF { get; set; }

        /// <summary>
        /// Water level sensors
        /// </summary>
        IEnumerable<WaterLevel> WaterLevels { get; set; }

        /// <summary>
        /// Relays
        /// </summary>
        IEnumerable<PowerRelay> Relays { get; set; }

        /// <summary>
        /// Gets the Relay based on relay letter
        /// </summary>
        /// <param name="relay"></param>
        /// <returns></returns>
        PowerRelay GetRelay(RelayLocation relay);

        /// <summary>
        /// True or false if the System Operations service is online
        /// </summary>
        bool SystemOnline { get; set; }

        /// <summary>
        /// Outside temperature 
        /// </summary>
        double? OutsideTempF { get; set; }

        /// <summary>
        /// Outside temperature celcius
        /// </summary>
        double? OutsideTempC { get; set; }

        /// <summary>
        /// Outside humidity
        /// </summary>
        double? OutsideHumidity { get; set; }

        /// <summary>
        /// Wind speeds
        /// </summary>
        double? WindSpeed { get; set; }

        /// <summary>
        /// Sunrise
        /// </summary>
        DateTime? Sunrise { get; set; }

        /// <summary>
        /// Sunset
        /// </summary>
        DateTime? Sunset { get; set; }

        /// <summary>
        /// Cloud coverage
        /// </summary>
        double? CloudCoverage { get; set; }

        /// <summary>
        /// Rain
        /// </summary>
        bool? Rain { get; set; }

        /// <summary>
        /// Used to draw a weather Icon
        /// </summary>
        string WeatherIcon { get; set; }
    }

}
