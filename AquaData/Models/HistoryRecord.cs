using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Historical Record of data collected
    /// </summary>
    public class HistoryRecord
    {
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
        public virtual IEnumerable<WaterReading> WaterReadings { get; set; }
        public virtual IEnumerable<PowerReading> PowerReadings { get; set; }
        public bool SystemRunning { get; set; }
        public DateTime Created { get; set; }
        /// <summary>
        /// Used for future serialized data
        /// </summary>
        public string Serialize { get; set; }

        #region cTor

        public HistoryRecord()
        {
            this.Created = DateTime.UtcNow;
        }

        public HistoryRecord(double tempF, double tempC, double humidity, IEnumerable<WaterLevel> levels,
            IEnumerable<PowerRelay> relays, bool systemOnline)
        {
            this.TempC = tempC;
            this.TempF = tempF;
            this.Humidity = humidity;
            this.WaterReadings = levels.Select(t => new WaterReading(t)).ToList();
            this.PowerReadings = relays.Select(t => new PowerReading(t)).ToList();
            this.SystemRunning = systemOnline;
            this.Created = DateTime.UtcNow;
        }

        public HistoryRecord(double tempF, double tempC, double humidity, IEnumerable<WaterLevel> levels,
            IEnumerable<PowerRelay> relays, bool systemOnline, double outsideTempF, double outsideTempC, double outsideHumidity) :
            this(tempF, tempC, humidity, levels, relays, systemOnline)
        {
            this.OutsideHumidity = outsideHumidity;
            this.OutsideTempC = outsideTempC;
            this.OutsideTempF = outsideTempF;
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
            this.Created = DateTime.UtcNow;
        }

        public void UpdateCreatedToLocal(DateTimeOffset timeZone)
        {            
            this.Created = this.Created.Add(timeZone.Offset);
        }


        #endregion


    }
}
