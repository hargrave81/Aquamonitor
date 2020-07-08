namespace AquaMonitor.Web.Devices
{
    /// <summary>
    /// User Bits Flags
    /// </summary>
    public enum ResolutionBits : byte
    {
        /// <summary>
        /// High
        /// </summary>
        Humidity12BitTemp14Bit = 0b0000_0000,
        /// <summary>
        /// Low
        /// </summary>
        Humidity8BitTemp12Bit = 0b0000_0001,
        /// <summary>
        /// Medium
        /// </summary>
        Humidity10BitTemp13Bit = 0b1000_0000,
        /// <summary>
        /// Unused
        /// </summary>
        Humidity11BitTemp11Bit = 0b1000_0001,        
    }

    /// <summary>
    /// Bits for heater
    /// </summary>
    public enum HeaterBits : byte
    {
        /// <summary>
        /// On flag
        /// </summary>
        OnChipHeaterOn = 0b100,
        /// <summary>
        /// Off flag
        /// </summary>
        OnChipHeaterOff = 0b000,
    }

    /// <summary>
    /// Allows bit manipulation
    /// </summary>
    public static class ByteShop
    {
        /// <summary>
        /// Returns the Bit State
        /// </summary>
        /// <param name="src"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool BitState(this byte src, int position)
        {
            return (src & (1 << position)) != 0;
        }

        /// <summary>
        /// Turns a bit on a given position
        /// </summary>
        /// <param name="src"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static byte BitOn(this byte src, int position)
        {            
            int mask = 1 << position;
            return (byte)(src | mask);
        }

        /// <summary>
        /// Turns a bit off at a given position
        /// </summary>
        /// <param name="src"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static byte BitOff(this byte src, int position)
        {
            int mask = 1 << position;
            return (byte)(src & ~mask);
        }
    }
}
