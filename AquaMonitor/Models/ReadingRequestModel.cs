using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AquaMonitor.Data.Models;
using Microsoft.CodeAnalysis;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Model to accept reading requests
    /// </summary>
    public class ReadingRequestModel
    {
        /// <summary>
        /// Type of reading
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Value of reading
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Scale of reading
        /// </summary>
        public string Scale { get; set; }

        /// <summary>
        /// Location of reading taken
        /// </summary>
        public string Location { get;set; }

        /// <summary>
        /// Notes of reading
        /// </summary>
        public string Note { get; set; }
        
        /// <summary>
        /// Date time reading was taken
        /// </summary>
        public string Taken { get; set; }

        /// <summary>
        /// Convert to reading
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        public IReading ToReading(out string errors)
        {
            var result = new Reading();
            errors = string.Empty;

            ReadingType resultType;
            if (Enum.TryParse<ReadingType>(this.Type, out resultType))
                result.Type = resultType;
            else
                errors += "Could not parse the type.";

            ReadingScale resultScale;
            if (!string.IsNullOrEmpty(this.Scale))
            {
                if (Enum.TryParse<ReadingScale>(this.Scale, out resultScale))
                    result.Scale = resultScale;
                else
                {
                    if (errors.Length > 0)
                        errors += "; ";
                    errors += "Could not parse the scale.";
                }
            }

            DateTime resultDate;
            if (DateTime.TryParse(Taken, out resultDate))
                result.Taken = resultDate;
            else
            {
                if (errors.Length > 0)
                    errors += "; ";
                errors += "Could not parse the taken event time.";
            }
        

            result.Value = Value;
            result.Note = Note;
            result.Location = Location;

            return (IReading)result.ToType();
        }
    }
}
