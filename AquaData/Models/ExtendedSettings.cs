using System;

namespace AquaMonitor.Data.Models
{

    /// <summary>
    /// Extended settings to avoid DB Updates
    /// </summary>
    public interface IExtendedSettings
    {
        /// <summary>
        /// Temperature offset
        /// </summary>
        double? TempOffset { get; set; }

        /// <summary>
        /// URL to fetch JPG of plant area
        /// </summary>
        // ReSharper disable once InconsistentNaming
        string CameraJPGUrl { get; set; }

        /// <summary>
        /// Food sessions
        /// </summary>
        FoodSession[] FoodSessions { get; set; }

        /// <summary>
        /// True or false if the water sensor is enabled
        /// </summary>
        bool WaterSensorEnabled {get; set; }
    }

    /// <summary>
    /// Extended settings to avoid DB updates
    /// </summary>
    class ExtendedSettings : IExtendedSettings
    {
        /// <summary>
        /// Temperature offset
        /// </summary>
        public double? TempOffset { get; set; }

        /// <summary>
        /// URL to fetch JPG of plant area
        /// </summary>
        public string CameraJPGUrl { get; set; }

        /// <summary>
        /// Food sessions
        /// </summary>
        public FoodSession[] FoodSessions { get; set; }

        /// <summary>
        /// True or false if the water sensor is enabled
        /// </summary>
        public bool WaterSensorEnabled { get; set; }
    }

    /// <summary>
    /// Contains a food session
    /// </summary>
    public class FoodSession
    {
        /// <summary>
        /// Start time of day
        /// </summary>
        public TimeSpan? StartTime { get; set; }
        /// <summary>
        /// Pin collection that runs the food process motor (e.g. 7, 11, 27, 22)
        /// </summary>
        public string PinCollection { get; set; }
        
        /// <summary>
        /// Total time to spend turning the motor
        /// </summary>
        public int TurnTime { get; set; }

        /// <summary>
        /// Last time it was ran
        /// </summary>
        public DateTime LastRan { get; set; }
    }
}
