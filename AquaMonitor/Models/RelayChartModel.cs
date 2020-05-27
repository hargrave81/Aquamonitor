using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Helpers;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Relay chart model
    /// </summary>
    public class RelayChartModel
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
        public ChartJSData<int>[] DataSets { get; set; }

        /// <summary>
        /// Create instance of the temp chart model
        /// </summary>
        public RelayChartModel()
        {
            Labels = new string[] {};
            DataSets = new ChartJSData<int>[]
            {
                new ChartJSData<int>()
                {
                    Label="Red",
                    Data = new int[]{},
                    BackgroundColor = "rgba(255,128,128,0.3)",
                    BorderColor = "rgba(255,128,128,1)",
                    PointBackgroundColor = "rgba(255,128,128,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<int>()
                {
                    Label="Blue",
                    Data = new int[]{},
                    BackgroundColor = "rgba(128,255,128,0.3)",
                    BorderColor = "rgba(128,255,128,1)",
                    PointBackgroundColor = "rgba(128,255,128,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<int>()
                {
                    Label="Yellow",
                    Data = new int[]{},
                    BackgroundColor = "rgba(200,128,200,0.3)",
                    BorderColor = "rgba(200,128,200,1)",
                    PointBackgroundColor = "rgba(200,128,200,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<int>()
                {
                    Label="Orange",
                    Data = new int[]{},
                    BackgroundColor = "rgba(225,192,128,0.3)",
                    BorderColor = "rgba(225,192,200,1)",
                    PointBackgroundColor = "rgba(225,192,128,.9)",
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
        public RelayChartModel(IEnumerable<HistoryRecord> records, TimeSpan Range) : this()
        {
            DataSets.First().Label = records.Last().PowerReadings.First().Name;

            if (records.Last().PowerReadings.Count() > 1)
            {
                DataSets.Skip(1).First().Label = records.Last().PowerReadings.Skip(1).First().Name;
                if (records.Last().PowerReadings.Count() > 2)
                {
                    DataSets.Skip(2).First().Label = records.Last().PowerReadings.Skip(2).First().Name;
                    if (records.Last().PowerReadings.Count() > 3)
                        DataSets.Skip(3).First().Label = records.Last().PowerReadings.Skip(3).First().Name;
                    else
                        DataSets = new ChartJSData<int>[] {DataSets[0], DataSets[1], DataSets[2]};
                }
                else
                    DataSets = new ChartJSData<int>[] {DataSets[0], DataSets[1]};
            }
            else
                DataSets = new ChartJSData<int>[] {DataSets[0]};

            string filter = "";

            if (Range.TotalDays > 90)
            {
                filter = "MM/yyyy";
                // do months
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("MMMM yyyy")).Distinct().ToArray();
                this.Labels = months.ToArray();

            } else if (Range.TotalDays > 6)
            {
                filter = "dd/MM/yyyy";
                // do days
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("MMM dd")).Distinct().ToArray();
                this.Labels = months.ToArray();

            } else if (Range.TotalHours > 8)
            {
                filter = "dd/MM/yyyy HH";
                // do hours
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("dd HH") + ":00").Distinct().ToArray();
                this.Labels = months.ToArray();

            } else if (Range.TotalMinutes > 10)
            {
                filter = "dd/MM/yyyy HH:mm";
                // do minutes
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("dd HH:mm")).Distinct().ToArray();
                this.Labels = months.ToArray();
            }
            else
            {
                filter = "dd/MM/yyyy HH:mm:ss";
                // do seconds
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("dd mm:ss")).Distinct().ToArray();
                this.Labels = months.ToArray();
            }

            this.DataSets.First().Data = records.GroupBy(t => t.Created.ToString(filter))
                .Select(t => t.AveragePowerState(z => z.PowerReadings.First().PowerState)).ToArray();
            if (this.DataSets.Length > 1)
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString(filter))
                    .Select(t => t.AveragePowerState(z => z.PowerReadings.Skip(1).First().PowerState)).ToArray();
            if (this.DataSets.Length > 2)
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString(filter))
                    .Select(t => t.AveragePowerState(z => z.PowerReadings.Skip(2).First().PowerState)).ToArray();
            if (this.DataSets.Length > 3)
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString(filter))
                    .Select(t => t.AveragePowerState(z => z.PowerReadings.Skip(3).First().PowerState)).ToArray();
        }


    }
}
