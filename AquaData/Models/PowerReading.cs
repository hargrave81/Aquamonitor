namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Power Reading value
    /// </summary>
    public class PowerReading
    {
        public int Id { get; set; }
        public int ReaderId { get; set; }
        public string Name { get; set; }
        public PowerState PowerState { get; set; }

        public PowerReading() { }

        public PowerReading(PowerRelay state)
        {
            this.ReaderId = state.Id;
            this.Name = state.Name;
            this.PowerState = state.CurrentState;
        }
    }
}
