using System;

namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Power Relay object
    /// </summary>
    public class PowerRelay
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
        public RelayLocation Letter { get; set; }

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
        public TimeSpan? Start { get; set; }

        /// <summary>
        /// Stop time for Power to stop working
        /// </summary>
        public TimeSpan? Stop { get; set; }

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
        /// Current power state based on last operation
        /// </summary>
        public PowerState CurrentState { get; set; }

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
        /// Gets today's powerStart time
        /// </summary>
        /// <returns></returns>
        public DateTime? PowerStartToday()
        { return PowerStartToday(DateTime.Now); }

        /// <summary>
        /// Gets today's powerStart time
        /// </summary>
        /// <returns></returns>
        public DateTime? PowerStartToday(DateTime now)
        {
            if (!Start.HasValue)
                return null;
            var startOfDay = DateTime.Parse(now.ToShortDateString() + " 00:00:00"); // midnight
            return startOfDay.Add(Start.Value);
        }

        /// <summary>
        /// Gets today's powerStop time
        /// </summary>
        /// <returns></returns>
        public DateTime? PowerStopToday() 
        { return PowerStopToday(DateTime.Now); }

        /// <summary>
        /// Gets today's powerStop time
        /// </summary>
        /// <returns></returns>
        public DateTime? PowerStopToday(DateTime now)
        {
            if (!Stop.HasValue)
                return null;
            var startOfDay = DateTime.Parse(now.ToShortDateString() + " 00:00:00"); // midnight
            return startOfDay.Add(Stop.Value);
        }

    }

}
