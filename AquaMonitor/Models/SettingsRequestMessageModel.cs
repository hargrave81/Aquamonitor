using System.Text.Json.Serialization;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Settings Request Message
    /// </summary>
    public class SettingsRequestMessageModel
    {
        /// <summary>
        /// Temperature Pin #
        /// </summary>
        [JsonPropertyName("TempPin")]
        public string TempPin { get; set; }

        /// <summary>
        /// Temperature Type (DHT11 or DHT22)
        /// </summary>
        [JsonPropertyName("TempType")]
        public string TempType { get; set; }

        /// <summary>
        /// Data collection rate in seconds (60 = 1 minute)
        /// </summary>
        [JsonPropertyName("DataCollectionRate")]
        public int DataCollectionRate { get; set; }


        /// <summary>
        /// Zipcode for weather API
        /// </summary>
        public string Zipcode { get; set; }

        /// <summary>
        /// Country for weather API
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// API Key for weather API
        /// </summary>
        public string APIKey { get; set; }
    }
}
