namespace WebDeploy
{
    /// <summary>
    /// Settings file
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// source location
        /// </summary>
        public string src { get; set; }
        /// <summary>
        /// dest location
        /// </summary>
        public string dest { get; set; }

        /// <summary>
        /// Automatically get updates from the web
        /// </summary>
        public bool autoUpdateFromWeb { get; set; }

    }
}
