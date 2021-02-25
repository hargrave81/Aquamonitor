using System;
using System.Text.Json.Serialization;
using UnitsNet;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Results from weather API
    /// </summary>
    public class WeatherResult
    {
        /// <summary>
        /// Longitude and Latitude
        /// </summary>
        [JsonPropertyName("coord")]
        public Coord Coord { get; set; }

        /// <summary>
        /// Primary weather state
        /// </summary>
        [JsonPropertyName("weather")]
        public Weather[] Weather { get; set; }

        /// <summary>
        /// Primary temperature
        /// </summary>
        [JsonPropertyName("main")]
        public Main Main { get; set; }

        /// <summary>
        /// Visibility distance
        /// </summary>
        [JsonPropertyName("visibility")]
        public long Visibility { get; set; }

        /// <summary>
        /// Wind speeds
        /// </summary>
        [JsonPropertyName("wind")]
        public Wind Wind { get; set; }

        /// <summary>
        /// Cloud coverage percentage
        /// </summary>
        [JsonPropertyName("clouds")]
        public Clouds Clouds { get; set; }

        /// <summary>
        /// Time of day calculations were made
        /// </summary>
        [JsonPropertyName("dt")]
        public long Dt { get; set; }

        /// <summary>
        /// Sunrise and fall details
        /// </summary>
        [JsonPropertyName("sys")]
        public Sys Sys { get; set; }

        /// <summary>
        /// Timezone
        /// </summary>
        [JsonPropertyName("timezone")]
        public long Timezone { get; set; }

        /// <summary>
        /// City ID
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// City Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Returns current Temperature
        /// </summary>
        [JsonIgnore]
        public Temperature Temp { get { return Temperature.FromDegreesCelsius(Main.Temp); } }

        /// <summary>
        /// Returns today's low Temperature
        /// </summary>
        [JsonIgnore]
        public Temperature TempMin { get { return Temperature.FromDegreesCelsius(Main.TempMin); } }

        /// <summary>
        /// Returns a today's high Temperature
        /// </summary>
        [JsonIgnore]
        public Temperature TempMax { get { return Temperature.FromDegreesCelsius(Main.TempMax); } }

        /// <summary>
        /// Returns current Feels Like Temperature
        /// </summary>
        [JsonIgnore]
        public Temperature TempFeelsLike { get { return Temperature.FromDegreesCelsius(Main.FeelsLike); } }
    }

    /// <summary>
    /// Cloud results
    /// </summary>

    public class Clouds
    {
        /// <summary>
        /// Cloud coverage 0 - 100
        /// </summary>
        [JsonPropertyName("all")]
        public long All { get; set; }
    }

    /// <summary>
    /// Coordinates
    /// </summary>
    public class Coord
    {
        /// <summary>
        /// Longitude
        /// </summary>
        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        /// <summary>
        /// Latitude
        /// </summary>
        [JsonPropertyName("lat")]
        public double Lat { get; set; }
    }

    /// <summary>
    /// Main temperature weather
    /// </summary>
    public class Main
    {
        /// <summary>
        /// Temp in celcius
        /// </summary>
        [JsonPropertyName("temp")]
        public double Temp { get; set; }

        /// <summary>
        /// What it feels like factoring things like humdiity
        /// </summary>
        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }

        /// <summary>
        /// Low temp
        /// </summary>
        [JsonPropertyName("temp_min")]
        public double TempMin { get; set; }

        /// <summary>
        /// High temp
        /// </summary>
        [JsonPropertyName("temp_max")]
        public double TempMax { get; set; }

        /// <summary>
        /// Pressure in hPa
        /// </summary>
        [JsonPropertyName("pressure")]
        public long Pressure { get; set; }

        /// <summary>
        /// Humidity 0 - 100  (%)
        /// </summary>
        [JsonPropertyName("humidity")]
        public long Humidity { get; set; }
    }    

    /// <summary>
    /// Sunrise Information
    /// </summary>
    public class Sys
    {
        /// <summary>
        /// Sunrise unix time UTC
        /// </summary>
        [JsonPropertyName("sunrise")]
        public long UnixSunrise { get; set; }

        /// <summary>
        /// Sunset unixtime UTC
        /// </summary>
        [JsonPropertyName("sunset")]
        public long UnixSunset { get; set; }

        /// <summary>
        /// Sunrise
        /// </summary>
        [JsonIgnore]
        public DateTime Sunrise { get { return UnixTimeStampToDateTime(UnixSunrise); } }

        /// <summary>
        /// Sunrise
        /// </summary>
        [JsonIgnore]
        public DateTime Sunset { get { return UnixTimeStampToDateTime(UnixSunset); } }


        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

    /// <summary>
    /// Weather details
    /// </summary>
    public class Weather
    {
        /// <summary>
        /// Main weather e.g. Rain, snow, clouds
        /// </summary>
        [JsonPropertyName("main")]
        public string Main { get; set; }

        /// <summary>
        /// Description of weather
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Icon of weather
        /// </summary>
        [JsonPropertyName("icon")]
        public string Icon { get; set; }
    }

    /// <summary>
    /// Wind data
    /// </summary>
    public class Wind
    {
        /// <summary>
        /// Wind speed in meter/sec
        /// </summary>
        [JsonPropertyName("speed")]
        public double Speed { get; set; }

        /// <summary>
        /// Returns wind speed in MPH
        /// </summary>
        /// <returns></returns>
        public double SpeedMph()
        {
            return Math.Round(Speed * 2.23694f,4);
        }
    }
}
