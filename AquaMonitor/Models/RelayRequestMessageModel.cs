using System.Text.Json.Serialization;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Relay Request Message
    /// </summary>
    public class RelayRequestMessageModel
    {
        /// <summary>
        /// a or b
        /// </summary>
        [JsonPropertyName("relay")]
        public string Relay { get; set; }
        /// <summary>
        /// on or off
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }
    }
}
