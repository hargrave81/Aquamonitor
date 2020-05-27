using System;
using System.Collections.Generic;
using System.Text;

namespace AquaMonitor.Data.Models
{

    /// <summary>
    /// Extended settings to avoid DB Updates
    /// </summary>
    public interface IExtendedSettings
    {
        /// <summary>
        /// Temperature offset
        /// </summary>
        double? TempOffset { get; set; }
    }

    /// <summary>
    /// Extended settings to avoid DB updates
    /// </summary>
    class ExtendedSettings : IExtendedSettings
    {
        /// <summary>
        /// Temperature offset
        /// </summary>
        public double? TempOffset { get; set; }
    }
}
