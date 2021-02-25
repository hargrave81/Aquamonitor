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

        /// <summary>
        /// Temperature Offset
        /// </summary>
        public double? More_TempOffset { get; set; }

        /// <summary>
        /// Camera JPG url
        /// </summary>
        public string More_CameraJPGUrl { get; set; }

        /// <summary>
        /// Feeding Pins
        /// </summary>
        public string More_FeedingPins { get; set; }

        /// <summary>
        /// Feed Start Time
        /// </summary>
        public string More_Feed1_Start { get; set; }
        /// <summary>
        /// Feed Turn TIme
        /// </summary>
        public string More_Feed1_Turn { get; set; }

        /// <summary>
        /// Feed Start Time
        /// </summary>
        public string More_Feed2_Start { get; set; }

        /// <summary>
        /// Feed Turn Time
        /// </summary>
        public string More_Feed2_Turn { get; set; }

        /// <summary>
        /// Feed Start Time
        /// </summary>
        public string More_Feed3_Start { get; set; }

        /// <summary>
        /// Feed Turn Time
        /// </summary>
        public string More_Feed3_Turn { get; set; }

        /// <summary>
        /// Monitor water temps
        /// </summary>
        public bool More_WaterSensorEnabled { get; set; }
    }
}
