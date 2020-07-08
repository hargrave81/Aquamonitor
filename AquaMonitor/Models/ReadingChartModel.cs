using AquaMonitor.Data.Models;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Model to render a reading chart
    /// </summary>
    public class ReadingChartModel
    {
        /// <summary>
        /// Chart Caption
        /// </summary>
        public string ChartCaption { get; set; }

        /// <summary>
        /// Type of chart to render
        /// </summary>
        public ReadingType Type { get; set; }

        /// <summary>
        /// Background of chart
        /// </summary>
        public string Background { get; set; }

        /// <summary>
        /// Collection of all readings
        /// </summary>
        public ReadingCollection AllReadings { get; set; }

        /// <summary>
        /// CTOR
        /// </summary>
        public ReadingChartModel()
        {
            this.Background = "bg-gradient-indigo";
        }

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="type"></param>
        /// <param name="collection"></param>
        public ReadingChartModel(string caption, ReadingType type, ReadingCollection collection) : this()
        {
            this.ChartCaption = caption;
            this.Type = type;
            this.AllReadings = collection;
        }

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="type"></param>
        /// <param name="collection"></param>
        /// <param name="background"></param>
        public ReadingChartModel(string caption, ReadingType type, ReadingCollection collection, string background) : this()
        {
            this.ChartCaption = caption;
            this.Type = type;
            this.AllReadings = collection;
            this.Background = background;
        }
    }

}
