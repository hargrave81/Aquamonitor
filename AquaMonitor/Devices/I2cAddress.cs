using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaMonitor.Web.Devices
{
    /// <summary>
    /// SHT3x I2C Address
    /// </summary>
    public enum I2cAddress : byte
    {
        /// <summary>
        /// ADDR (pin2) connected to logic low (Default)
        /// </summary>
        AddrLow = 0x40,

        /// <summary>
        /// ADDR (pin2) connected to logic high
        /// </summary>
        AddrHigh = 0x41
    }
}
