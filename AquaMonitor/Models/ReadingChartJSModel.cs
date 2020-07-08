using System;
using System.Collections.Generic;
using System.Linq;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Helpers;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Reading chart JS model
    /// </summary>
    public class ReadingChartJSModel
    {
        /// <summary>
        /// labels for chart
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("labels")]
        public string[] Labels { get; set; }

        /// <summary>
        /// Datasets to display
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("datasets")]
        public ChartJSData<float>[] DataSets { get; set; }

        /// <summary>
        /// Create instance of the temp chart model
        /// </summary>
        public ReadingChartJSModel()
        {
            Labels = new string[] {};
            DataSets = new[]
            {
                new ChartJSData<float>()
                {
                    Label="Red",
                    Data = new float[]{},
                    BackgroundColor = "rgba(255,128,128,0.3)",
                    BorderColor = "rgba(255,128,128,1)",
                    PointBackgroundColor = "rgba(255,128,128,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<float>()
                {
                    Label="Blue",
                    Data = new float[]{},
                    BackgroundColor = "rgba(128,255,128,0.3)",
                    BorderColor = "rgba(128,255,128,1)",
                    PointBackgroundColor = "rgba(128,255,128,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<float>()
                {
                    Label="Yellow",
                    Data = new float[]{},
                    BackgroundColor = "rgba(200,128,200,0.3)",
                    BorderColor = "rgba(200,128,200,1)",
                    PointBackgroundColor = "rgba(200,128,200,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<float>()
                {
                    Label="Orange",
                    Data = new float[]{},
                    BackgroundColor = "rgba(225,192,128,0.3)",
                    BorderColor = "rgba(225,192,200,1)",
                    PointBackgroundColor = "rgba(225,192,128,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                }
            };
        }

        /// <summary>
        /// Creates a chart
        /// </summary>
        /// <param name="readings"></param>
        /// <param name="range"></param>
        public ReadingChartJSModel(IEnumerable<IReading> readings, TimeSpan range) : this()
        {
            if (readings.Count() == 0)
            {
                return;
            }
            var uniqueReadings = readings.GroupBy(t => t.Type).Select(z => z.First());
            var readers = uniqueReadings.Select(z => z.Type).ToList();
            for (int x = 0; x < readers.Count; x++)
            {
                DataSets.Skip(x).First().Label = readers.Skip(x).First().ToString();
            }
            
            string filter;

            if (range.TotalDays > 30)
            {
                filter = "MM/yyyy";
                // do months
                var months = readings.OrderBy(t => t.Taken).Select(t => t.Taken.ToString("MMMM yyyy")).Distinct().ToArray();
                this.Labels = months.ToArray();

            } 
            else
            {
                filter = "dd/MM/yyyy";
                // do days
                var months = readings.OrderBy(t => t.Taken).Select(t => t.Taken.ToString("MMM dd")).Distinct().ToArray();
                this.Labels = months.ToArray();

            }

            for (int x = 0; x < readers.Count; x++)
            {
                var dataToAnalyze = readings.Where(t => t.Type == readers[x]).GroupBy(t => t.Taken.ToString(filter));
                this.DataSets.Skip(x).First().Data = dataToAnalyze.Select(t => (float)t.NormalAverage(z => z.Value)).ToArray();
            }
        }


    }
}
