namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Error view model
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Request ID
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// True or false to show requestID
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
