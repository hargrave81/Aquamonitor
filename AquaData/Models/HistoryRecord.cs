using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Historical Record of data collected
    /// </summary>
    public sealed class HistoryRecord
    {
        [Key]
        public int Id { get; set; }
        public double TempF { get; set; }
        public double TempC { get; set; }
        public double Humidity { get; set; }
        public double OutsideTempF { get; set; }
        public double OutsideTempC { get; set; }
        public double OutsideHumidity { get; set; }
        public double? WindSpeed { get; set; }
        public DateTime? Sunrise { get; set; }
        public DateTime? Sunset { get; set; }
        public double? CloudCoverage { get; set; }
        public bool? Rain { get; set; }
        public IEnumerable<WaterReading> WaterReadings { get; set; }
        public IEnumerable<PowerReading> PowerReadings { get; set; }
        public bool SystemRunning { get; set; }
        public DateTime Created { get; set; }

        /// <summary>
        /// Used for future serialized data
        /// </summary>
        public string Serialize
        {
            get => ExtendedData != null ? System.Text.Json.JsonSerializer.Serialize(ExtendedData, jsonOptions) : "{}";
            set => ExtendedData = string.IsNullOrEmpty(value) ? new HistoryExtended() : System.Text.Json.JsonSerializer.Deserialize<HistoryExtended>(value, jsonOptions);
        }

        private JsonSerializerOptions jsonOptions
        {
            get
            {
                var result = new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                    IgnoreNullValues = true,
                    IgnoreReadOnlyProperties = true,
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                };
                result.Converters.Add(new TimeSpanConverter());
                return result;
            }
        }

        [NotMapped]
        [System.Text.Json.Serialization.JsonIgnore]
        public IHistoryExtended ExtendedData { get; set; }

        #region cTor

        public HistoryRecord()
        {
            this.Created = DateTime.UtcNow;
            this.ExtendedData = new HistoryExtended();
        }

        public HistoryRecord(double tempF, double tempC, double humidity, IEnumerable<WaterLevel> levels,
            IEnumerable<PowerRelay> relays, bool systemOnline) : this()
        {
            TempC = tempC;
            TempF = tempF;
            Humidity = humidity;
            WaterReadings = levels.Select(t => new WaterReading(t)).ToList();
            PowerReadings = relays.Select(t => new PowerReading(t)).ToList();
            SystemRunning = systemOnline;
        }

        public HistoryRecord(double tempF, double tempC, double humidity, IEnumerable<WaterLevel> levels,
            IEnumerable<PowerRelay> relays, bool systemOnline, double outsideTempF, double outsideTempC, double outsideHumidity) :
            this(tempF, tempC, humidity, levels, relays, systemOnline)
        {
            this.OutsideHumidity = outsideHumidity;
            this.OutsideTempC = outsideTempC;
            this.OutsideTempF = outsideTempF;
        }

        public HistoryRecord(double tempF, double tempC, double humidity, IEnumerable<WaterLevel> levels,
            IEnumerable<PowerRelay> relays, bool systemOnline, double outsideTempF, double outsideTempC, double outsideHumidity, double waterTemp) :
            this(tempF, tempC, humidity, levels, relays, systemOnline, outsideTempF, outsideTempC, outsideHumidity)
        {
            this.ExtendedData.WaterTemp = waterTemp;
        }

        public HistoryRecord(IGlobalState state)
        {
            this.TempC = state.TemperatureC;
            this.TempF = state.TemperatureF;
            this.Humidity = state.Humidity;
            this.WaterReadings = state.WaterLevels.Select(t => new WaterReading(t)).ToList();
            this.PowerReadings = state.Relays.Select(t => new PowerReading(t)).ToList();
            this.SystemRunning = state.SystemOnline;
            this.CloudCoverage = state.CloudCoverage;
            this.WindSpeed = state.WindSpeed;
            this.OutsideHumidity = state.OutsideHumidity ?? 0;
            this.OutsideTempF = state.OutsideTempF ?? 0;
            this.OutsideTempC = state.OutsideTempC ?? 0;
            this.Rain = state.Rain;
            this.Sunrise = state.Sunrise;
            this.Sunset = state.Sunset;
            this.ExtendedData = new HistoryExtended();
            this.ExtendedData.WaterTemp = state.WaterTemp??0;
            this.Created = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets a historical reading by reader ID or returns a blank low level reading if none found
        /// </summary>
        /// <param name="readerId"></param>
        /// <returns>WaterReading</returns>
        public WaterReading HistoricalWater(int readerId)
        {
            if (this.WaterReadings.Any(t => t.ReaderId == readerId))
            {
                return WaterReadings.First(t => t.ReaderId == readerId);
            }
            return new WaterReading() {Id = 0, Name = "", ReaderId = readerId, WaterLevelHigh = false};
        }


        /// <summary>
        /// Gets a historical reading by relay ID or returns a blank no power reading if none found
        /// </summary>
        /// <param name="relayId"></param>
        /// <returns>PowerReading</returns>
        public PowerReading HistoricalPower(int relayId)
        {
            if (this.PowerReadings.Any(t => t.ReaderId == relayId))
            {
                return PowerReadings.First(t => t.ReaderId == relayId);
            }
            return new PowerReading() {Id = 0, Name = "", PowerState = PowerState.Off, ReaderId = relayId};
        }

        public void UpdateCreatedToLocal(DateTimeOffset timeZone)
        {            
            this.Created = this.Created.Add(timeZone.Offset);
        }


        #endregion


    }

    /// <summary>
    /// Extended History values
    /// </summary>
    public class HistoryExtended : IHistoryExtended
    {
        public double WaterTemp { get; set; }
    }

    /// <summary>
    /// Extended History interface
    /// </summary>
    public interface IHistoryExtended
    {
        double WaterTemp { get; set; }
    }
}
