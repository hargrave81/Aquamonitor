namespace AquaMonitor.Web.Devices
{
    /// <summary>
    /// Htu21D Resolution
    /// </summary>
    public enum Resolution : byte
    {
        /// <summary>High resolution</summary>
        High = 0x00,

        /// <summary>Medium resolution</summary>
        Medium = 0x08,

        /// <summary>Low resolution</summary>
        Low = 0x16
    }
}
