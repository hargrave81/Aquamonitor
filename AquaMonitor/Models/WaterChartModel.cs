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
    public class WaterChartModel
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
        public WaterChartModel()
        {
            Labels = new string[] { };
            DataSets = new ChartJSData<int>[]
            {
                new ChartJSData<int>()
                {
                    Label="Purple",
                    Data = new int[]{},
                    BackgroundColor = "rgba(240,200,128,0.3)",
                    BorderColor = "rgba(240,200,128,1)",
                    PointBackgroundColor = "rgba(240,200,128,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<int>()
                {
                    Label="Aqua",
                    Data = new int[]{},
                    BackgroundColor = "rgba(128,240,220,0.3)",
                    BorderColor = "rgba(128,240,220,1)",
                    PointBackgroundColor = "rgba(128,240,220,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<int>()
                {
                    Label="Navy",
                    Data = new int[]{},
                    BackgroundColor = "rgba(64,128,200,0.3)",
                    BorderColor = "rgba(64,128,200,1)",
                    PointBackgroundColor = "rgba(64,128,200,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<int>()
                {
                    Label="Raspberry",
                    Data = new int[]{},
                    BackgroundColor = "rgba(210,31,60,0.3)",
                    BorderColor = "rgba(210,31,60,1)",
                    PointBackgroundColor = "rgba(210,31,60,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<int>()
                {
                    Label="Hunter",
                    Data = new int[]{},
                    BackgroundColor = "rgba(63,122,77,0.3)",
                    BorderColor = "rgba(63,122,77,1)",
                    PointBackgroundColor = "rgba(63,122,77,.9)",
                    PointBorderColor = "#fff",
                    Fill = true
                },
                new ChartJSData<int>()
                {
                    Label="HotPink",
                    Data = new int[]{},
                    BackgroundColor = "rgba(255,0,127,0.3)",
                    BorderColor = "rgba(255,0,127,1)",
                    PointBackgroundColor = "rgba(255,0,127,.9)",
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
        public WaterChartModel(IEnumerable<HistoryRecord> records, TimeSpan Range) : this()
        {
            DataSets.First().Label = records.Last().WaterReadings.First().Name;

            if (records.Last().WaterReadings.Count() > 1)
            {
                DataSets.Skip(1).First().Label = records.Last().WaterReadings.Skip(1).First().Name;
                if (records.Last().WaterReadings.Count() > 2)
                {
                    DataSets.Skip(2).First().Label = records.Last().WaterReadings.Skip(2).First().Name;
                    if (records.Last().WaterReadings.Count() > 3)
                    {
                        DataSets.Skip(3).First().Label = records.Last().WaterReadings.Skip(3).First().Name;
                        if (records.Last().WaterReadings.Count() > 4)
                        {
                            DataSets.Skip(4).First().Label = records.Last().WaterReadings.Skip(4).First().Name;
                            if (records.Last().WaterReadings.Count() > 5)
                            {
                                DataSets.Skip(5).First().Label = records.Last().WaterReadings.Skip(5).First().Name;
                            }
                            else
                                DataSets = new ChartJSData<int>[]
                                    {DataSets[0], DataSets[1], DataSets[2], DataSets[3], DataSets[4]};
                        }
                        else
                            DataSets = new ChartJSData<int>[] { DataSets[0], DataSets[1], DataSets[2], DataSets[3] };
                    }
                    else
                        DataSets = new ChartJSData<int>[] { DataSets[0], DataSets[1], DataSets[2] };
                }
                else
                    DataSets = new ChartJSData<int>[] { DataSets[0], DataSets[1] };
            }
            else
                DataSets = new ChartJSData<int>[] { DataSets[0] };

            string filter = "";

            if (Range.TotalDays > 90)
            {
                filter = "MM/yyyy";
                // do months
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("MMMM yyyy")).Distinct().ToArray();
                this.Labels = months.ToArray();

            }
            else if (Range.TotalDays > 6)
            {
                filter = "dd/MM/yyyy";
                // do days
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("MMM dd")).Distinct().ToArray();
                this.Labels = months.ToArray();

            }
            else if (Range.TotalHours > 8)
            {
                filter = "dd/MM/yyyy HH";
                // do hours
                var months = records.OrderBy(t => t.Created).Select(t => t.Created.ToString("dd HH") + ":00").Distinct().ToArray();
                this.Labels = months.ToArray();

            }
            else if (Range.TotalMinutes > 10)
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
                .Select(t => t.AverageWaterState(z => z.WaterReadings.First().WaterLevelHigh)).ToArray();
            if (this.DataSets.Length > 1)
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString(filter))
                    .Select(t => t.AverageWaterState(z => z.WaterReadings.Skip(1).First().WaterLevelHigh)).ToArray();
            if (this.DataSets.Length > 2)
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString(filter))
                    .Select(t => t.AverageWaterState(z => z.WaterReadings.Skip(2).First().WaterLevelHigh)).ToArray();
            if (this.DataSets.Length > 3)
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString(filter))
                    .Select(t => t.AverageWaterState(z => z.WaterReadings.Skip(3).First().WaterLevelHigh)).ToArray();
            if (this.DataSets.Length > 4)
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString(filter))
                    .Select(t => t.AverageWaterState(z => z.WaterReadings.Skip(4).First().WaterLevelHigh)).ToArray();
            if (this.DataSets.Length > 5)
                this.DataSets.Last().Data = records.GroupBy(t => t.Created.ToString(filter))
                    .Select(t => t.AverageWaterState(z => z.WaterReadings.Skip(5).First().WaterLevelHigh)).ToArray();
        }


    }
}
