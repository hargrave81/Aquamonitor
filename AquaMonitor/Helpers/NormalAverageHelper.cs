using System;
using System.Collections.Generic;
using System.Linq;
using AquaMonitor.Data.Models;

namespace AquaMonitor.Web.Helpers
{
    /// <summary>
    /// Average helper function to normalize data
    /// </summary>
    public static class NormalAverageHelper
    {
        /// <summary>
        /// Returns the normalized average of values
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static double NormalAverage(this IEnumerable<double> entries)
        {
            var avg = entries.Average();
            var max = entries.Max();
            var min = entries.Min();
            var newmin = min;
            var newmax = max;
            if (max - avg > 5)
            {
                newmax = max - ((max - avg) * .1); // remove just 10% of the rogue entries
            }

            if (avg - min > 5)
            {
                newmin = min + ((avg - min) * .1); // remove just 10% of the rogue entries
            }

            try
            {
                var subentries = entries.Where(t => t <= newmax && t >= newmin);
                if (subentries.Any())
                    return subentries.Average();
                // we need to favor either min or max, so he was the farthest
                if (max - avg > avg - min)
                {
                    // lets favor the min side, max seems a bit wonky
                    newmax = max - ((max - avg) * .2); // remove just 10% of the rogue entries
                    return entries.Where(t => t <= newmax).Average();
                }
                else
                {
                    // min may be wonky
                    newmin = min + ((avg - min) * .2); // remove just 10% of the rogue entries
                    return entries.Where(t => t >= newmin).Average();
                }
            }
            catch
            {

            }
            return avg;
        }

        /// <summary>
        /// Returns the normalized average of values
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static double NormalAverage<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            var avg = Enumerable.Average(Enumerable.Select(source, selector));
            var max = Enumerable.Max(Enumerable.Select(source, selector));
            var min = Enumerable.Min(Enumerable.Select(source, selector));
            var newmin = min;
            var newmax = max;
            if(max - avg > 5) {
                newmax = max - ((max - avg) * .1); // remove just 10% of the rogue entries
            }

            if (avg - min > 5)
            {
                newmin = min + ((avg - min) * .1); // remove just 10% of the rogue entries
            }

            try
            {
                var entries = Enumerable.Select(source, selector).Where(t => t <= newmax && t >= newmin);
                if (entries.Any())
                    return Math.Round(entries.Average(),2);
                // we need to favor either min or max, so he was the farthest
                if (max - avg > avg - min)
                {
                    // lets favor the min side, max seems a bit wonky
                    newmax = max - ((max - avg) * .2); // remove just 10% of the rogue entries
                    return Math.Round(Enumerable.Select(source, selector).Where(t => t <= newmax).Average(),2);
                }
                else
                {
                    // min may be wonky
                    newmin = min + ((avg - min) * .2); // remove just 10% of the rogue entries
                    return Math.Round(Enumerable.Select(source, selector).Where(t => t >= newmin).Average(),2);
                }
            }
            catch
            {
                // our new average is no good just go with non-normalized
                
            }
            return Math.Round(avg,2);
        }

        /// <summary>
        /// Returns the Average Power State using both average and noteworthy average
        /// </summary>
        /// <param name="records">Group of records over time</param>
        /// <param name="readerId">Id of reader you want to report on</param>
        /// <returns></returns>
        public static int[] AveragePowerState(this IEnumerable<IGrouping<string, HistoryRecord>> records, int readerId)
        {
            var initialResults = records.Select(t => t.AveragePowerState(z => z.HistoricalPower(readerId).PowerState)).ToArray();
            System.Diagnostics.Debug.WriteLine($"Average Power: {readerId}  " + string.Join(',',initialResults.Select(p => p.ToString("##0.000")).ToArray()));
            var result = new List<int>(){ (int)initialResults[0]};
            for (int x = 1; x < records.Count(); x++)
            {
                result.Add( records.Skip(x).First()
                    .NoteWorthyAveragePowerState(e => e.HistoricalPower(readerId).PowerState, (int)initialResults[x - 1]));
            }
            return result.ToArray();
        }


        /// <summary>
        /// Returns the average power state over time
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static float AveragePowerState(this IEnumerable<PowerState> entries)
        {
            return AveragePowerState(entries, e => e);
        }

        /// <summary>
        /// Returns the average power state over time
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static float AveragePowerState<TSource>(this IEnumerable<TSource> source, Func<TSource, PowerState> selector)
        {
            var onCount = Enumerable.Select(source, selector).Count(z => z == PowerState.On);
            var offCount = Enumerable.Select(source, selector).Count(z => z == PowerState.Off);
            if (onCount > offCount)
                return 1 + (onCount / (offCount + onCount)/10);
            return Math.Min(0.999f,0 + (offCount / (onCount+ offCount))/10);
        }

        /// <summary>
        /// Returns the average power state over time that is of note
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="previous">Previous detected value</param>
        /// <returns></returns>
        /// /// <remarks>This will trigger an unlikely value if the previous value was the same</remarks>
        public static int NoteWorthyAveragePowerState(this IEnumerable<PowerState> entries, int previous)
        {
            return NoteWorthyAveragePowerState(entries, e => e, previous);
        }

        /// <summary>
        /// Returns the average power state over time that is of note
        /// </summary>
        /// <param name="source">source values</param>
        /// <param name="selector">selector to trim results</param>
        /// <param name="previous">Previous detected value</param>
        /// <returns></returns>
        /// <remarks>This will trigger an unlikely value if the previous value was the same</remarks>
        public static int NoteWorthyAveragePowerState<TSource>(this IEnumerable<TSource> source, Func<TSource, PowerState> selector, int previous)
        {
            var onCount = (float)Enumerable.Select(source, selector).Count(z => z == PowerState.On);
            var offCount = (float)Enumerable.Select(source, selector).Count(z => z == PowerState.Off);
            if (onCount > offCount)
            {
                // typically one
                if (onCount > offCount * 1.5)
                    return 1; // not worth showing
                return previous == 1 ? 0 : 1;
            }
            else
            {
                // typically zero
                if (offCount > onCount * 1.5)
                    return 0; // not worth showing
                return previous == 0 ? 1 : 0;
            }
        }

        /// <summary>
        /// Returns the Average Water State using both average and noteworthy average
        /// </summary>
        /// <param name="records">Group of records over time</param>
        /// <param name="readerId">Id of reader you want to report on</param>
        /// <returns></returns>
        public static int[] AverageWaterState(this IEnumerable<IGrouping<string, HistoryRecord>> records, int readerId)
        {
            var initialResults = records.Select(t => t.AverageWaterState(z => z.HistoricalWater(readerId).WaterLevelHigh)).ToArray();
            System.Diagnostics.Debug.WriteLine($"Average Water: {readerId}  " + string.Join(',',initialResults.Select(p => p.ToString("##0.000")).ToArray()));
            var result = new List<int>(){ (int)initialResults[0]};
            for (int x = 1; x < records.Count(); x++)
            {
                result.Add( records.Skip(x).First()
                    .NoteWorthyAverageWaterState(e => e.HistoricalWater(readerId).WaterLevelHigh, (int)initialResults[x - 1]));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Returns the average power state over time
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static float AverageWaterState(this IEnumerable<bool> entries)
        {
            return AverageWaterState(entries, e => e);
        }

        /// <summary>
        /// Returns the average power state over time
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static float AverageWaterState<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> selector)
        {
            var onCount = (float)Enumerable.Select(source, selector).Count(z => z);
            var offCount = (float)Enumerable.Select(source, selector).Count(z => z == false);
            if (onCount > offCount)
                return 1 + (onCount / (offCount + onCount)/10);
            return Math.Min(0.999f,0 + (offCount / (onCount+ offCount))/10);
        }

        /// <summary>
        /// Returns the average power state over time
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="previous">Previous detected value</param>
        /// <returns></returns>
        /// <remarks>This will trigger an unlikely value if the previous value was the same</remarks>
        public static int NoteWorthyAverageWaterState(this IEnumerable<bool> entries, int previous)
        {
            return NoteWorthyAverageWaterState(entries, e => e, previous);
        }

        /// <summary>
        /// Returns the average power state over time
        /// </summary>
        /// <param name="source">source values</param>
        /// <param name="selector">selector to trim results</param>
        /// <param name="previous">Previous detected value</param>
        /// <returns></returns>
        /// <remarks>This will trigger an unlikely value if the previous value was the same</remarks>
        public static int NoteWorthyAverageWaterState<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> selector, int previous)
        {
            var onCount = Enumerable.Select(source, selector).Count(z => z);
            var offCount = Enumerable.Select(source, selector).Count(z => z == false);
            if (onCount > offCount)
            {
                // typically one
                if (onCount > offCount * 1.5)
                    return 1; // not worth showing
                return previous == 1 ? 0 : 1;
            }
            else
            {
                // typically zero
                if (offCount > onCount * 1.5)
                    return 0; // not worth showing
                return previous == 0 ? 1 : 0;
            }
        }
    }
}
