namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Global objects
    /// </summary>
    public interface IAppSetting
    {
        /// <summary>
        /// Temperature Pin
        /// </summary>
        int TempPin { get; set; }

        /// <summary>
        /// Temp Type (11 or 22)
        /// </summary>
        int TempType { get; set; }

        /// <summary>
        /// How fast to record history in seconds default is 60
        /// </summary>
        int DataCollectionRate { get; set; }

        /// <summary>
        /// Password required for changing settings and interfacing with the website
        /// </summary>
        string AdminPassword { get; set; }

        /// <summary>
        /// Zipcode for weather API
        /// </summary>
        string Zipcode { get; set; }

        /// <summary>
        /// Country for weather API
        /// </summary>
        string Country { get; set; }

        /// <summary>
        /// API Key for weather API
        /// </summary>
        string APIKey { get; set; }
    }
}
