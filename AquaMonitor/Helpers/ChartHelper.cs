using System;
using System.Threading.Tasks;
using AquaMonitor.Data.Models;

namespace AquaMonitor.Web.Helpers
{
    /// <summary>
    /// Chart helper
    /// </summary>
    public static class ChartHelper
    {
        /// <summary>
        /// Generate a chart of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static async Task<T> GetChartAsync<T>(this Data.Context.AquaDbContext context, DateTime startDate, DateTime endDate)
        {
            var records = await context.GetHistoryAsync(startDate, endDate);
            var chartResult = (T)Activator.CreateInstance(typeof(T), new object[] { records, endDate.Subtract(startDate) });
            return chartResult;
        }


        /// <summary>
        /// Generate a chart of Type T for readings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static async Task<T> GetChartAsync<T>(this Data.Context.AquaDbContext context, ReadingType type, DateTime startDate, DateTime endDate)
        {
            var records = await context.GetReadingsAsync(type, startDate, endDate);
            var chartResult = (T)Activator.CreateInstance(typeof(T), new object[] { records, endDate.Subtract(startDate) });
            return chartResult;
        }
    }
}
