using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaMonitor.Web.Devices
{
    /// <summary>
    /// SHT3x Register
    /// </summary>
    internal enum Register : ushort
    {
        HTU_TEM = 0xE3,
        HTU_HUM = 0xE5,
        HTU_TEM_NH = 0xF3,
        HTU_HUM_NH = 0xF5,
        HTU_RESET = 0xFE,
        HTU_WRITE = 0xE6,
        HTU_READ = 0xE7
    }
}
