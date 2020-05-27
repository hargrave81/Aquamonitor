using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// Returns the average power state over time
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static int AveragePowerState(this IEnumerable<PowerState> entries)
        {
            var OnCount = entries.Count(z => z == PowerState.On);
            var OffCount = entries.Count(z => z == PowerState.Off);
            if (OnCount > OffCount)
                return 1;
            return 0;
        }

        /// <summary>
        /// Returns the average power state over time
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static int AveragePowerState<TSource>(this IEnumerable<TSource> source, Func<TSource, PowerState> selector)
        {
            var OnCount = Enumerable.Select(source, selector).Count(z => z == PowerState.On);
            var OffCount = Enumerable.Select(source, selector).Count(z => z == PowerState.Off);
            if (OnCount > OffCount)
                return 1;
            return 0;
        }

        /// <summary>
        /// Returns the average power state over time
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static int AverageWaterState(this IEnumerable<bool> entries)
        {
            var OnCount = entries.Count(z => z == true);
            var OffCount = entries.Count(z => z == false);
            if (OnCount > OffCount)
                return 1;
            return 0;
        }

        /// <summary>
        /// Returns the average power state over time
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static int AverageWaterState<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> selector)
        {
            var OnCount = Enumerable.Select(source, selector).Count(z => z == true);
            var OffCount = Enumerable.Select(source, selector).Count(z => z == false);
            if (OnCount > OffCount)
                return 1;
            return 0;
        }
    }
}
