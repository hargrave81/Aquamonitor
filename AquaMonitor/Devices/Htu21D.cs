using Iot.Device.Ft4222;
using Iot.Device.Sht3x;
using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Threading.Tasks;
using Iot.Units;

namespace AquaMonitor.Web.Devices
{
    /// <summary>
    /// Humidity and Temperature Sensor Htu21D
    /// </summary>

    public class Htu21D : IDisposable
    {

        private const int HTU21_TEMPERATURE_CONVERSION_TIME_T_14b_RH_12b = 50;
        private const int HTU21_TEMPERATURE_CONVERSION_TIME_T_13b_RH_10b = 25;
        private const int HTU21_TEMPERATURE_CONVERSION_TIME_T_12b_RH_8b = 13;
        private const int HTU21_TEMPERATURE_CONVERSION_TIME_T_11b_RH_11b = 7;
        private const int HTU21_HUMIDITY_CONVERSION_TIME_T_14b_RH_12b = 16;
        private const int HTU21_HUMIDITY_CONVERSION_TIME_T_13b_RH_10b = 5;
        private const int HTU21_HUMIDITY_CONVERSION_TIME_T_12b_RH_8b = 3;
        private const int HTU21_HUMIDITY_CONVERSION_TIME_T_11b_RH_11b = 8;
        private const float TEMPERATURE_COEFF_MUL = 175.72f;
        private const float TEMPERATURE_COEFF_ADD = -46.85f;
        private const float HUMIDITY_COEFF_MUL = 125;
        private const float HUMIDITY_COEFF_ADD = -6;
        private const float HTU21_TEMPERATURE_COEFFICIENT = -0.15f;
        private const float HTU21_CONSTANT_A = 8.1332f;
        private const float HTU21_CONSTANT_B = 1762.39f;
        private const float HTU21_CONSTANT_C = 235.66f;

        private static I2cDevice _staticDevice;
        private I2cDevice _i2cDevice;
        private DateTime lastTemp = DateTime.MinValue;
        private DateTime lastHumid = DateTime.MinValue;
        public bool EnableErrorCorrection { get; set; }
        #region prop

        private Resolution _resolution;

        /// <summary>
        /// Htu21D Resolution
        /// </summary>
        public Resolution Resolution 
        {
            get => _resolution;
            set
            {
                SetResolution(value);
                _resolution = value;
            }
        }

        private double _temperature;

        /// <summary>
        /// Htu21D Temperature
        /// </summary>
        public Temperature Temperature
        {
            get
            {
                ReadTemp();
                lastTemp = DateTime.Now;
                return Temperature.FromCelsius(_temperature);
            }
        }

        private double _humidity;

        /// <summary>
        /// Htu21D Relative Humidity (%)
        /// </summary>
        public double Humidity
        {
            get
            {
                ReadHumidity();
                lastHumid = DateTime.Now;
                return _humidity;
            }
        }

        /// <summary>
        /// Humidity compensated for the temperature
        /// </summary>
        public double CompensatedHumidity
        {
            get => (_humidity + (25 - _temperature) * HTU21_TEMPERATURE_COEFFICIENT);
        }

        private bool _lastReadSuccess;

        /// <summary>
        /// Returns true or false if the last read was successful
        /// </summary>
        public bool IsLastReadSuccessful
        {
            get => _lastReadSuccess;
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
                double partial_pressure;
                double dew_point;

                // Missing power of 10
                partial_pressure = Math.Pow(10, HTU21_CONSTANT_A - HTU21_CONSTANT_B / (_temperature + HTU21_CONSTANT_C));

                dew_point = -HTU21_CONSTANT_B / (Math.Log10(_humidity * partial_pressure / 100) - HTU21_CONSTANT_A) - HTU21_CONSTANT_C;

                return (float)dew_point;
            }
        }

        private bool _heater;

        /// <summary>
        /// Htu21D Heater
        /// </summary>
        public bool Heater
        {
            get => _heater;
            set
            {
                SetHeater(value);
                _heater = value;
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
            I2cConnectionSettings settings = new I2cConnectionSettings(busId, (byte)address);
            if(_staticDevice == null)
                _staticDevice = I2cDevice.Create(settings);
            return new Htu21D(_staticDevice, resolution);
        }

        /// <summary>
        /// Creates a new instance of the Htu21D
        /// </summary>
        /// <param name="i2CDevice">The I2C device used for communication.</param>
        /// <param name="resolution">Htu21D Read Resolution</param>
        public Htu21D(I2cDevice i2CDevice, Resolution resolution = Resolution.High)
        {
            EnableErrorCorrection = true;

            _i2cDevice = i2CDevice;
            
            Resolution = resolution;            

            Reset();
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
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
        /// <param name="resolution">Resolution is the quality of the temp and humidity data gathering</param>
        private void SetResolution(Resolution resolution)
        {            
            WriteBits(Register.HTU_WRITE, resolution == Resolution.High ? ResolutionBits.Humidity12BitTemp14Bit : resolution == Resolution.Medium ? ResolutionBits.Humidity10BitTemp13Bit : ResolutionBits.Humidity8BitTemp12Bit, null);            
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
                var humidity = (float)rawHumidityReading / 65536 * (float)HUMIDITY_COEFF_MUL + HUMIDITY_COEFF_ADD;

                var crcData = humidityData.Take(2).ToArray();
                var crc8 = humidityData[2];
                bool tCrc = CheckCrc8(crcData, crc8);
                if (tCrc == false)
                {
                    _lastReadSuccess = false;
                    Console.WriteLine("Failed humid CRC");
                    return;
                }
                _lastReadSuccess = true;
                if (EnableErrorCorrection) // fix HTU21Ds calibration inaccuracy estimates http://www.kandrsmith.org/RJS/Misc/Hygrometers/many_surface.png
                {
                    if (humidity >= 88 && _temperature >= 10)
                        humidity += 6;
                    else if (humidity >= 88 && _temperature >= 5)
                        humidity += 3;
                    else if (humidity >= 78 && _temperature >= 10)
                        humidity += 4;
                    else if (humidity >= 78 && _temperature >= 5)
                        humidity += 2;
                    else if(humidity >= 58 && _temperature >= 10)
                        humidity += 2;
                    else if (humidity >= 58 && _temperature >= 5)
                        humidity += 1;
                    else if (humidity <= 25 && _temperature >= 15)
                        humidity += 2;
                    else if (humidity <= 25 && _temperature >= 10)
                        humidity += 1;
                    else if (humidity <= 16 && _temperature >= 10)
                        humidity += 4;
                    else if (humidity <= 16 && _temperature >= 5)
                        humidity += 2;
                }
                _humidity = humidity;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                _lastReadSuccess = false;
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

                var temperature = (float)rawTempReading / 65536 * (float)TEMPERATURE_COEFF_MUL + TEMPERATURE_COEFF_ADD;

                var crcData = tempData.Take(2).ToArray();
                var crc8 = tempData[2];
                bool tCrc = CheckCrc8(crcData, crc8);
                if (tCrc == false)
                {
                    _lastReadSuccess = false;
                    Console.WriteLine("Failed temp CRC");
                    return;
                }
                _lastReadSuccess = true;
                _temperature = temperature;            
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                _lastReadSuccess = false;
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
            byte msb = (byte)register;
            Span<byte> writeBuff = stackalloc byte[]
            {
                msb
            };
            _i2cDevice.Write(writeBuff); // request the userdata            
            System.Threading.Thread.Sleep(10);
            if (byteLength == 1)
            {
                byte contents = _i2cDevice.ReadByte();
                return new byte[] { contents };
            }
            Span<byte> readBuff = stackalloc byte[byteLength];
            _i2cDevice.Read(readBuff);
            return readBuff.ToArray();
        }        

        /// <summary>
        /// Writes bits to the user register
        /// </summary>
        /// <param name="register"></param>
        /// <param name="resolution"></param>
        /// <param name="heater"></param>
        private void WriteBits(Register register, ResolutionBits? resolution, HeaterBits? heater)
        {
            var readBits = ReadBits(Register.HTU_READ).First();
            // flip bits as needed
            if (resolution.HasValue)
            {
                switch (resolution.Value)
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
            if (heater.HasValue)
            {
                switch (heater.Value)
                {
                    case HeaterBits.OnChipHeaterOn:
                        readBits.BitOn(2);
                        break;
                    default:
                        readBits.BitOff(2);
                        break;
                }
            }
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
            Span<byte> writeBuff = stackalloc byte[]
            {
                (byte)register
            };
            Span<byte> readBuff = stackalloc byte[readLength];
            _i2cDevice.WriteRead(writeBuff, readBuff);
            return readBuff.ToArray();
        }
        
        /// <summary>
        /// Writes out
        /// </summary>
        /// <param name="register"></param>
        /// <param name="data"></param>
        private void Write(Register register, byte? data = null)
        {
            byte msb = (byte)register;

            Span<byte> writeBuff = stackalloc byte[]
            {
                msb
            };
            if (data != null)
                writeBuff = stackalloc byte[] { msb, data.Value }; // add the data field

            _i2cDevice.Write(writeBuff);

            // wait SCL free            
            System.Threading.Thread.Sleep(20);
        }

        /// <summary>
        /// Dispose the static device instance
        /// </summary>
        public static void DisposeI2C()
        {
            if (_staticDevice != null)
                _staticDevice.Dispose();
        }
    }
}