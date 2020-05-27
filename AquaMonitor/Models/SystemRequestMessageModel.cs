using System.Text.Json.Serialization;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// System Request Message
    /// </summary>
    public class SystemRequestMessageModel
    {
        /// <summary>
        /// on or off
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }
    }
}
