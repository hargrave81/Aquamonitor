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
    }
}
