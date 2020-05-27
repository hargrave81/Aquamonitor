
namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Water level monitor
    /// </summary>
    public class WaterLevel
    {
        /// <summary>
        /// ID of water level
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Logical name of water level
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Float status
        /// </summary>
        public bool FloatHigh { get; set; }
        /// <summary>
        /// GPIO Pin
        /// </summary>
        /// <value></value>
        public int Pin { get; set; }
    }
}
