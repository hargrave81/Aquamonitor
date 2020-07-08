using AquaMonitor.Data.Models;
using System;
using System.Collections.Generic;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Relay request model for editing relays
    /// </summary>
    public class PowerRelayRequestMessageModel
    {
        /// <summary>
        /// Array of relays
        /// </summary>
        public IEnumerable<PowerRelayModel> Relays { get; set; }
    }

    /// <summary>
    /// Relay Model
    /// </summary>
    public class PowerRelayModel
    {
        /// <summary>
        /// Power Relay Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Logical Name for Power Relay
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Logical Relay Letter
        /// </summary>
        public int Letter { get; set; }

        /// <summary>
        /// Interval for Power Relay in seconds
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Run time for Power Relay in seconds
        /// </summary>
        public int IntervalRun { get; set; }

        /// <summary>
        /// Start time for Power to begin working
        /// </summary>
        public string Start { get; set; }

        /// <summary>
        /// Stop time for Power to stop working
        /// </summary>
        public string Stop { get; set; }

        /// <summary>
        /// Min Temp to start operating - ideal for cool mode
        /// </summary>
        public double? MinTempF { get; set; }

        /// <summary>
        /// Max Temp before stop operating - ideal for heat mode
        /// </summary>
        public double? MaxTempF { get; set; }

        /// <summary>
        /// Min Outside Temp to start operating - ideal for cool mode
        /// </summary>
        public double? MinOutTempF { get; set; }

        /// <summary>
        /// Max Outside Temp before stop operating - ideal for heat mode
        /// </summary>
        public double? MaxOutTempF { get; set; }

        /// <summary>
        /// Variance before triggering an on/off operation from desired set temp
        /// </summary>
        public double? TempVariance { get; set; }

        /// <summary>
        /// Power functions based on a water level ID
        /// </summary>
        public int? WaterId { get; set; }

        /// <summary>
        /// Power is on when float is High (true float high power is on, false float is low power is on)
        /// </summary>
        public bool OnWhenFloatHigh { get; set; }

        /// <summary>
        /// The pin within the system board
        /// </summary>
        public int Pin { get; set; }

        /// <summary>
        /// Creates a power relay from the model
        /// </summary>
        /// <returns></returns>
        public PowerRelay ToPowerRelay()
        {
            var result = new PowerRelay()
            {
                Id = this.Id,
                Name = this.Name,
                Letter = (RelayLocation)this.Letter,
                Interval = this.Interval,
                IntervalRun = this.IntervalRun,
                MinTempF = this.MinTempF,
                MaxTempF = this.MaxTempF,
                MinOutTempF = this.MinOutTempF,
                MaxOutTempF = this.MaxOutTempF,
                TempVariance = this.TempVariance,
                WaterId = this.WaterId,
                OnWhenFloatHigh = this.OnWhenFloatHigh
            };
            if (!string.IsNullOrEmpty(this.Stop))
            {
                result.Stop = TimeSpan.Parse(this.Stop);
            }
            if (!string.IsNullOrEmpty(this.Start))
            {
                result.Start = TimeSpan.Parse(this.Start);
            }
            return result;
        }

        /// <summary>
        /// Updates a relay
        /// </summary>
        /// <param name="fromDb"></param>
        public void UpdateRelay(PowerRelay fromDb)
        {
            fromDb.Name = this.Name;
            fromDb.Interval = this.Interval;
            fromDb.IntervalRun = this.IntervalRun;
            fromDb.MinTempF = this.MinTempF;
            fromDb.MaxTempF = this.MaxTempF;
            fromDb.MinOutTempF = this.MinOutTempF;
            fromDb.MaxOutTempF = this.MaxOutTempF;
            fromDb.TempVariance = this.TempVariance;
            fromDb.WaterId = this.WaterId;
            fromDb.OnWhenFloatHigh = this.OnWhenFloatHigh;
            if (!string.IsNullOrEmpty(this.Stop))
            {
                fromDb.Stop = TimeSpan.Parse(this.Stop);
            }
            else
            {
                fromDb.Stop = null;
            }
            if (!string.IsNullOrEmpty(this.Start))
            {
                fromDb.Start = TimeSpan.Parse(this.Start);
            }
            else
            {
                fromDb.Start = null;
            }            
        }
    }
}
