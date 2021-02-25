using System;
using System.Device.I2c;
using System.Linq;
using AquaMonitor.Web.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace AquaMonitor.Web.Devices
{
    /// <summary>
    /// Humidity and Temperature Sensor Htu21D
    /// </summary>

    public class Htu21D : IDisposable
    {
        private const float TEMPERATURE_COEFF_MUL = 175.72f;
        private const float TEMPERATURE_COEFF_ADD = -46.85f;
        private const float HUMIDITY_COEFF_MUL = 125;
        private const float HUMIDITY_COEFF_ADD = -6;
        private const float HTU21_TEMPERATURE_COEFFICIENT = -0.15f;
        private const float HTU21_CONSTANT_A = 8.1332f;
        private const float HTU21_CONSTANT_B = 1762.39f;
        private const float HTU21_CONSTANT_C = 235.66f;

        private readonly ILogger logger;
        private static byte? address = null;
        private static int? bus = null;

        private I2cDevice i2CDevice
        {
            get
            {
                var settings = new I2cConnectionSettings(bus.Value, address.Value);
                return I2cDevice.Create(settings);
            }
        }
        private DateTime lastTemp = DateTime.MinValue;
        private DateTime lastHumid = DateTime.MinValue;
        /// <summary>
        /// Enables Humidity Correction
        /// </summary>
        public bool EnableErrorCorrection { get; set; }
        #region prop

        private Resolution resolution;

        /// <summary>
        /// Htu21D Resolution
        /// </summary>
        public Resolution Resolution 
        {
            get => resolution;
            set
            {
                SetResolution(value);
                resolution = value;
            }
        }

        private double temperature;

        /// <summary>
        /// Htu21D Temperature
        /// </summary>
        public Temperature Temperature
        {
            get
            {
                ReadTemp();
                lastTemp = DateTime.Now;
                return Temperature.FromDegreesCelsius(temperature);
            }
        }

        private double humidity;

        /// <summary>
        /// Htu21D Relative Humidity (%)
        /// </summary>
        public double Humidity
        {
            get
            {
                ReadHumidity();
                lastHumid = DateTime.Now;
                return humidity;
            }
        }

        /// <summary>
        /// Humidity compensated for the temperature
        /// </summary>
        public double CompensatedHumidity
        {
            get => (humidity + (25 - temperature) * HTU21_TEMPERATURE_COEFFICIENT);
        }

        private bool lastReadSuccess;

        /// <summary>
        /// Returns true or false if the last read was successful
        /// </summary>
        public bool IsLastReadSuccessful
        {
            get => lastReadSuccess;
        }

        /// <summary>
        /// Gets the computed dew point
        /// </summary>
        public double DewPoint 
        { 
            get
            {
                if(DateTime.Now.Subtract(lastTemp).TotalMinutes > 5 || DateTime.Now.Subtract(lastHumid).TotalMinutes > 5)
                {
                    ReadHumidity();
                    ReadTemp();
                }

                // Missing power of 10
                var partialPressure = Math.Pow(10, HTU21_CONSTANT_A - HTU21_CONSTANT_B / (temperature + HTU21_CONSTANT_C));

                var dewPoint = -HTU21_CONSTANT_B / (Math.Log10(humidity * partialPressure / 100) - HTU21_CONSTANT_A) - HTU21_CONSTANT_C;

                return (float)dewPoint;
            }
        }

        private bool heater;

        /// <summary>
        /// Htu21D Heater
        /// </summary>
        public bool Heater
        {
            get => heater;
            set
            {
                SetHeater(value);
                heater = value;
            }
        }

        #endregion

        /// <summary>
        /// Creates a Htu21D temperature device
        /// </summary>
        /// <param name="busId"></param>
        /// <param name="address"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static Htu21D CreateDevice(int busId, I2cAddress address = I2cAddress.AddrLow, Resolution resolution = Resolution.High)
        {
            var logger = ApplicationLogging.CreateLogger<Htu21D>();
            try
            {
                Htu21D.bus = busId;
                Htu21D.address = (byte) address;
                var settings = new I2cConnectionSettings(busId, (byte) address);
                var dev = I2cDevice.Create(settings);
                logger.LogDebug("Device for temperature created successfully.");
                dev.Dispose();
                return new Htu21D(resolution);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create HTU21D");
            }
            return null;
        }

        /// <summary>
        /// Creates a new instance of the Htu21D
        /// </summary>
        /// <param name="resolution">Htu21D Read Resolution</param>
        public Htu21D(Resolution resolution = Resolution.High)
        {
            this.logger = ApplicationLogging.CreateLogger<Htu21D>();
            EnableErrorCorrection = true;
            
            Resolution = resolution;            

            logger.LogDebug("Reset temp sensor to base.");
            Reset();
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Htu21D Soft Reset
        /// </summary>
        public void Reset()
        {
            Write(Register.HTU_RESET);
        }

        /// <summary>
        /// Set Htu21D Resolution
        /// </summary>
        /// <param name="newResolution">Resolution is the quality of the temp and humidity data gathering</param>
        private void SetResolution(Resolution newResolution)
        {            
            WriteBits(Register.HTU_WRITE, newResolution == Resolution.High ? ResolutionBits.Humidity12BitTemp14Bit : newResolution == Resolution.Medium ? ResolutionBits.Humidity10BitTemp13Bit : ResolutionBits.Humidity8BitTemp12Bit, null);            
        }


        /// <summary>
        /// Set Htu21D Heater
        /// </summary>
        /// <param name="isOn">Heater on when value is true</param>
        private void SetHeater(bool isOn)
        {
            if (isOn)
            {                
                WriteBits(Register.HTU_WRITE, null, HeaterBits.OnChipHeaterOn);
            }
            else
            {
                WriteBits(Register.HTU_WRITE, null, HeaterBits.OnChipHeaterOff);
            }
        }

        /// <summary>
        /// Read Humidity
        /// </summary>
        private void ReadHumidity()
        {
            try
            {
                var humidityData = WriteRead(Register.HTU_HUM, 3);

                var rawHumidityReading = humidityData[0] << 8 | humidityData[1];
                /*
                var humidityRatio = rawHumidityReading / (float)65536;
                double humidity = -6 + (125 * humidityRatio);            
                */
                var localHumidity = (float)rawHumidityReading / 65536 * (float)HUMIDITY_COEFF_MUL + HUMIDITY_COEFF_ADD;

                var crcData = humidityData.Take(2).ToArray();
                var crc8 = humidityData[2];
                bool tCrc = CheckCrc8(crcData, crc8);
                if (tCrc == false)
                {
                    lastReadSuccess = false;
                    Console.WriteLine("Failed humid CRC");
                    return;
                }
                lastReadSuccess = true;
                if (EnableErrorCorrection) // fix HTU21Ds calibration inaccuracy estimates http://www.kandrsmith.org/RJS/Misc/Hygrometers/many_surface.png
                {
                    if (localHumidity >= 88 && temperature >= 10)
                        localHumidity += 6;
                    else if (localHumidity >= 88 && temperature >= 5)
                        localHumidity += 3;
                    else if (localHumidity >= 78 && temperature >= 10)
                        localHumidity += 4;
                    else if (localHumidity >= 78 && temperature >= 5)
                        localHumidity += 2;
                    else if(localHumidity >= 58 && temperature >= 10)
                        localHumidity += 2;
                    else if (localHumidity >= 58 && temperature >= 5)
                        localHumidity += 1;
                    else if (localHumidity <= 25 && temperature >= 15)
                        localHumidity += 2;
                    else if (localHumidity <= 25 && temperature >= 10)
                        localHumidity += 1;
                    else if (localHumidity <= 16 && temperature >= 10)
                        localHumidity += 4;
                    else if (localHumidity <= 16 && temperature >= 5)
                        localHumidity += 2;
                }
                this.humidity = localHumidity;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                lastReadSuccess = false;
            }
        }

        /// <summary>
        /// Read Humidity
        /// </summary>
        private void ReadTemp()
        {
            try
            {
                var tempData = WriteRead(Register.HTU_TEM, 3);

                var rawTempReading = tempData[0] << 8 | tempData[1];

                var localTemperature = (float)rawTempReading / 65536 * (float)TEMPERATURE_COEFF_MUL + TEMPERATURE_COEFF_ADD;

                var crcData = tempData.Take(2).ToArray();
                var crc8 = tempData[2];
                bool tCrc = CheckCrc8(crcData, crc8);
                if (tCrc == false)
                {
                    lastReadSuccess = false;
                    Console.WriteLine("Failed temp CRC");
                    return;
                }
                lastReadSuccess = true;
                this.temperature = localTemperature;            
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                lastReadSuccess = false;
            }
}      

        /// <summary>
        /// 8-bit CRC Checksum Calculation
        /// </summary>
        /// <param name="data">Raw Data</param>
        /// <param name="crc8">Raw CRC8</param>
        /// <returns>Checksum is true or false</returns>
        private bool CheckCrc8(byte[] data, byte crc8)
        {

            byte polynom = 0x31; // x^8 + x^5 + x^4 + 1
            byte crc = 0x00;            

            for(int i = 0; i <= 1; i ++)
            {
                crc ^= data[i];
                for(int j = 8; j > 0;j--)
                {
                    if((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ polynom);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }
            return crc == crc8;
        }

        /// <summary>
        /// Reads the register and returns the byte read
        /// </summary>
        /// <param name="register"></param>
        /// <param name="byteLength"></param>
        /// <returns></returns>
        private byte[] ReadBits(Register register, int byteLength = 1)
        {
            logger.LogDebug("Writing to read -> the bits ...");
            byte msb = (byte)register;
            Span<byte> writeBuff = stackalloc byte[]
            {
                msb
            };
            logger.LogDebug($"Write to device after bit read ... [{register.ToString()}]");
            var i2C = i2CDevice;
            i2C.Write(writeBuff); // request the userdata            
            i2C.Dispose();
            logger.LogDebug("Reading the result ...");
            System.Threading.Thread.Sleep(10);
            if (byteLength == 0)
                System.Threading.Thread.Sleep(10);
            if (byteLength == 0)
            {
                logger.LogWarning("Failed to read sensor no data");
                return new byte[] {};
            }
            if (byteLength == 1)
            {
                logger.LogDebug("Reading single byte");
                i2C = i2CDevice;
                byte contents = i2C.ReadByte();
                i2C.Dispose();
                return new[] { contents };
            }
            logger.LogDebug("Reading bytes");
            Span<byte> readBuff = stackalloc byte[byteLength];
            i2C = i2CDevice;
            i2C.Read(readBuff);
            i2C.Dispose();
            return readBuff.ToArray();
        }        

        /// <summary>
        /// Writes bits to the user register
        /// </summary>
        /// <param name="register"></param>
        /// <param name="resolutionValue"></param>
        /// <param name="heaterValue"></param>
        private void WriteBits(Register register, ResolutionBits? resolutionValue, HeaterBits? heaterValue)
        {
            var readBits = ReadBits(Register.HTU_READ).First();
            // flip bits as needed
            if (resolutionValue.HasValue)
            {
                switch (resolutionValue.Value)
                {
                    case ResolutionBits.Humidity12BitTemp14Bit:
                        readBits.BitOff(7);
                        readBits.BitOff(0);
                        break;
                    case ResolutionBits.Humidity10BitTemp13Bit:
                        readBits.BitOn(7);
                        readBits.BitOff(0);
                        break;
                    case ResolutionBits.Humidity11BitTemp11Bit:
                        readBits.BitOn(7);
                        readBits.BitOn(0);
                        break;
                    default:
                        readBits.BitOff(7);
                        readBits.BitOn(0);
                        break;
                }
            }
            if (heaterValue.HasValue)
            {
                switch (heaterValue.Value)
                {
                    case HeaterBits.OnChipHeaterOn:
                        readBits.BitOn(2);
                        break;
                    default:
                        readBits.BitOff(2);
                        break;
                }
            }
            logger.LogDebug("Writing Bits to a register for resolution and heater");
            Write(register, readBits);
            // wait SCL free            
            System.Threading.Thread.Sleep(15);
        }

        /// <summary>
        /// Writes then reads the result
        /// </summary>
        /// <param name="register"></param>
        /// <param name="readLength"></param>
        /// <returns></returns>
        private byte[] WriteRead(Register register, int readLength)
        {
            logger.LogDebug($"Write read device to get temp or humidity ... [{register.ToString()}]");
            Span<byte> writeBuff = stackalloc byte[]
            {
                (byte)register
            };
            Span<byte> readBuff = stackalloc byte[readLength];
            var i2C = i2CDevice;
            i2C.WriteRead(writeBuff, readBuff);
            i2C.Dispose();
            return readBuff.ToArray();
        }
        
        /// <summary>
        /// Writes out
        /// </summary>
        /// <param name="register"></param>
        /// <param name="data"></param>
        private void Write(Register register, byte? data = null)
        {
            logger.LogDebug($"Write to device ... [{register.ToString()}]");
            byte msb = (byte)register;

            Span<byte> writeBuff = stackalloc byte[]
            {
                msb
            };
            if (data != null)
                writeBuff = stackalloc byte[] { msb, data.Value }; // add the data field

            var i2C = i2CDevice;
            i2C.Write(writeBuff);
            i2C.Dispose();
            // wait SCL free            
            System.Threading.Thread.Sleep(20);
        }

        /// <summary>
        /// Dispose the static device instance
        /// </summary>
        public static void DisposeI2C()
        {
        }
    }
}