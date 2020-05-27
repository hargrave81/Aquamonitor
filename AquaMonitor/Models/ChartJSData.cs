using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Chart values
    /// </summary>
    public class ChartJSData<T>
    {
        /// <summary>
        /// Label for dataset
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("label")]
        public string Label { get; set; }

        /// <summary>
        /// Fill color for data area
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Stroke color for line
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("borderColor")]
        public string BorderColor { get; set; }

        /// <summary>
        /// Point color for when data is marked on chart
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("pointBackgroundColor")]
        public string PointBackgroundColor { get; set; }

        /// <summary>
        /// Point stroke color for where data is marked on chart
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("pointBorderColor")]
        public string PointBorderColor { get; set; }

        /// <summary>
        /// true or false to fill the line region
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("fill")]
        public bool Fill { get; set; }

        /// <summary>
        /// Data values to plot on the chart
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public T[] Data { get; set; }

    }
}
