using System;

namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Reading provided by the user
    /// </summary>
    public interface IReading
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        int Id { get; set; }
        /// <summary>
        /// gets or sets the reading value
        /// </summary>
        double Value { get; set; }

        /// <summary>
        /// Type of reading
        /// </summary>
        ReadingType Type { get; set; }

        /// <summary>
        /// Scale at which the reading is taken
        /// </summary>
        ReadingScale Scale { get; set; }

        /// <summary>
        /// Date time in which the reading was taken
        /// </summary>
        DateTime Taken { get; set; }

        /// <summary>
        /// Location in the farm where the reading was taken, allows grouping by location
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// Ideal minimum value
        /// </summary>
        public double IdealMin { get; }

        /// <summary>
        /// Ideal maximum value
        /// </summary>
        public double IdealMax { get; }

        /// <summary>
        /// Notes taken about reading
        /// </summary>
        string Note { get; set; }

        /// <summary>
        /// Converts a reading to its base type
        /// </summary>
        /// <returns></returns>
        IReading ToType();

        /// <summary>
        /// Returns the display string for the Scale
        /// </summary>
        string ScaleString { get; }
    }
}
