
namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Reading of water state
    /// </summary>
    public class WaterReading
    {
        public int Id { get; set; }
        public int ReaderId { get; set; }
        public string Name { get; set; }
        public bool WaterLevelHigh { get; set; }

        public WaterReading()
        {
        }

        public WaterReading(WaterLevel state)
        {
            this.ReaderId = state.Id;
            this.Name = state.Name;
            this.WaterLevelHigh = state.FloatHigh;
        }
    }
}
