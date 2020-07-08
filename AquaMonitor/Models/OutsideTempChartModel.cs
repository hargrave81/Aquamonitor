using System;
using System.Collections.Generic;
using System.Linq;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Helpers;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Outside Temperature chart model
    /// </summary>
    public class OutsideTempChartModel
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
        public OutsideTempChartModel()
        {
            Labels = new string[] {};
            DataSets = new[]
            {
                new ChartJSData<float>()
                {
                    Label="Temperature(F)",
                    Data = new float[]{},
                    BackgroundColor = "rgba(205,151,151,0.3)",
                    BorderColor = "rgba(220,151,151,1)",
                    PointBackgroundColor = "rgba(220,151,151,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<float>()
                {
                    Label="Humidity",
                    Data = new float[]{},
                    BackgroundColor = "rgba(151,187,205,0.3)",
                    BorderColor = "rgba(151,190,210,1)",
                    PointBackgroundColor = "rgba(151,190,210,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<float>()
                {
                    Label="Wind",
                    Data = new float[]{},
                    BackgroundColor = "rgba(210,210,255,0.4)",
                    BorderColor = "rgba(210,210,255,1)",
                    PointBackgroundColor = "rgba(210,210,255,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<float>()
                {
                    Label="Clouds",
                    Data = new float[]{},
                    BackgroundColor = "rgba(210,210,210,0.3)",
                    BorderColor = "rgba(210,210,210,1)",
                    PointBackgroundColor = "rgba(210,210,210,.6)",
                    PointBorderColor = "#fff",
                    Fill = true
                }
            };
        }

        /// <summary>
        /// Create instance of the temp chart model with data
        /// </summary>
        /// <param name="records"></param>
        /// <param name="range"></param>
        public OutsideTempChartModel(IEnumerable<HistoryRecord> records, TimeSpan range) : this()
        {
            string filter;
            if (range.TotalDays > 90)
            {
                // do months
                filter = "MM/yyyy";
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("MMMM yyyy")).Distinct().ToArray();
                this.Labels = months.ToArray();
            } else if (range.TotalDays > 6)
            {
                // do days
                filter = "dd/MM/yyyy";
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("MMM dd")).Distinct().ToArray();
                this.Labels = months.ToArray();
            } else if (range.TotalHours > 8)
            {
                // do hours
                filter = "dd/MM/yyyy HH";
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("dd HH") + ":00").Distinct().ToArray();
                this.Labels = months.ToArray();
            } else if (range.TotalMinutes > 10)
            {
                // do minutes
                filter = "dd/MM/yyyy HH:mm";
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("dd HH:mm")).Distinct().ToArray();
                this.Labels = months.ToArray();
            }
            else
            {
                // do seconds
                filter = "dd/MM/yyyy HH:mm:ss";
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("dd mm:ss")).Distinct().ToArray();
                this.Labels = months.ToArray();

            }
            this.DataSets.First().Data = records.GroupBy(t => t.Created.ToString(filter))
                .Select(t => (float)t.NormalAverage(z => z.OutsideTempF)).ToArray();
            this.DataSets.Skip(1).First().Data = records.GroupBy(t => t.Created.ToString(filter))
                .Select(t => (float)t.NormalAverage(z => z.OutsideHumidity)).ToArray();
            try
            {
                this.DataSets.Skip(2).First().Data = records.Where(t => t.WindSpeed.HasValue)
                    .GroupBy(t => t.Created.ToString(filter))
                    .Select(t => (float) t.NormalAverage(z => z.WindSpeed.Value)).ToArray();
                this.DataSets.Skip(3).First().Data = records.Where(t => t.CloudCoverage.HasValue)
                    .GroupBy(t => t.Created.ToString(filter))
                    .Select(t => (float) t.NormalAverage(z => z.CloudCoverage.Value)).ToArray();
            }
            catch
            {
                // ignored
            }
        }

    }
}
