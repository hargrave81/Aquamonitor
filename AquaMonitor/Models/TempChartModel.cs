﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Helpers;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Temperature chart model
    /// </summary>
    public class TempChartModel
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
        public TempChartModel()
        {
            Labels = new string[] {};
            DataSets = new ChartJSData<float>[]
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
                }
            };
        }

        /// <summary>
        /// Create instance of the temp chart model with data
        /// </summary>
        /// <param name="records"></param>
        /// <param name="Range"></param>
        public TempChartModel(IEnumerable<HistoryRecord> records, TimeSpan Range) : this()
        {
            if (Range.TotalDays > 90)
            {
                // do months
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("MMMM yyyy")).Distinct().ToArray();
                this.Labels = months.ToArray();
                this.DataSets.First().Data = records.GroupBy(t => t.Created.ToString("MM/yyyy"))
                    .Select(t => (float) t.NormalAverage(z => z.TempF)).ToArray();
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString("MM/yyyy"))
                    .Select(t => (float)t.NormalAverage(z => z.Humidity)).ToArray();
            } else if (Range.TotalDays > 6)
            {
                // do days
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("MMM dd")).Distinct().ToArray();
                this.Labels = months.ToArray();
                this.DataSets.First().Data = records.GroupBy(t => t.Created.ToString("dd/MM/yyyy"))
                    .Select(t => (float)t.NormalAverage(z => z.TempF)).ToArray();
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString("dd/MM/yyyy"))
                    .Select(t => (float)t.NormalAverage(z => z.Humidity)).ToArray();
            } else if (Range.TotalHours > 8)
            {
                // do hours
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("dd HH") + ":00").Distinct().ToArray();
                this.Labels = months.ToArray();
                this.DataSets.First().Data = records.GroupBy(t => t.Created.ToString("dd/MM/yyyy HH"))
                    .Select(t => (float)t.NormalAverage(z => z.TempF)).ToArray();
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString("dd/MM/yyyy HH"))
                    .Select(t => (float)t.NormalAverage(z => z.Humidity)).ToArray();
            } else if (Range.TotalMinutes > 10)
            {
                // do minutes
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("dd HH:mm")).Distinct().ToArray();
                this.Labels = months.ToArray();
                this.DataSets.First().Data = records.GroupBy(t => t.Created.ToString("dd/MM/yyyy HH:mm"))
                    .Select(t => (float)t.NormalAverage(z => z.TempF)).ToArray();
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString("dd/MM/yyyy HH:mm"))
                    .Select(t => (float)t.NormalAverage(z => z.Humidity)).ToArray();
            }
            else
            {
                // do seconds
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("dd mm:ss")).Distinct().ToArray();
                this.Labels = months.ToArray();
                this.DataSets.First().Data = records.GroupBy(t => t.Created.ToString("dd/MM/yyyy HH:mm:ss"))
                    .Select(t => (float)t.NormalAverage(z => z.TempF)).ToArray();
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString("dd/MM/yyyy HH:mm:ss"))
                    .Select(t => (float)t.NormalAverage(z => z.Humidity)).ToArray();
            }
        }

    }
}
