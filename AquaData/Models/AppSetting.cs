using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// App Settings
    /// </summary>
    public class AppSetting : IAppSetting
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Temperature Pin
        /// </summary>
        public int TempPin { get; set; }

        /// <summary>
        /// Temp Type (11 or 22)
        /// </summary>
        public int TempType { get; set; }

        /// <summary>
        /// How fast to record history in seconds default is 60
        /// </summary>
        public int DataCollectionRate { get; set; }

        /// <summary>
        /// Password required for changing settings and interfacing with the website
        /// </summary>
        public string AdminPassword { get; set; }

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


        #region Extra settings
        /// <summary>
        /// Gets or sets extended settings
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public IExtendedSettings More { get; set; }

        /// <summary>
        /// Extended settings for serialization
        /// </summary>
        public string SettingA
        {
            get => More != null ? System.Text.Json.JsonSerializer.Serialize(More) : "{}";
            set => More = string.IsNullOrEmpty(value) ? new ExtendedSettings() : System.Text.Json.JsonSerializer.Deserialize<ExtendedSettings>(value);
        }

        /// <summary>
        /// Extended settings for serialization future use
        /// </summary>
        public string SettingB { get; set; }

        /// <summary>
        /// Extended settings for serialization future use
        /// </summary>
        public string SettingC { get; set; }
        #endregion

        public AppSetting() {
            this.More = new ExtendedSettings();
        }
    }


}
