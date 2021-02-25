using System;
using System.Collections.Generic;
using System.Linq;
using AquaMonitor.Data.Context;
using Microsoft.Extensions.Configuration;

namespace AquaMonitor.Data.Models
{

    /// <summary>
    /// Global Data
    /// </summary>
    public class GlobalData : IGlobalState
    {
        private int systemOnline;
        private double tempF = -999d;
        /// <summary>
        /// Current humidity level
        /// </summary>
        public double Humidity { get; set; }
        /// <summary>
        /// Current Temperature in Celsius
        /// </summary>
        public double TemperatureC { get; set; }
        /// <summary>
        /// Current Temperature in Fahrenheit
        /// </summary>
        public double TemperatureF
        {
            get { if (tempF.Equals(-999d)) return 0; return tempF; }
            set
            {
                tempF = value;
                if (systemOnline == 0) // user has never set the system to false
                {
                    SystemOnline = true;
                    systemOnline = 1;
                }
            }
        }
        /// <summary>
        /// Temperature Pin
        /// </summary>
        public int TempPin { get; set; }


        /// <summary>
        /// Temp Type (11 or 22)
        /// </summary>
        public int TempType { get; set; }

        /// <summary>
        /// Water level sensors
        /// </summary>
        public IEnumerable<WaterLevel> WaterLevels { get; set; }
        /// <summary>
        /// Relays
        /// </summary>
        public IEnumerable<PowerRelay> Relays { get; set; }

        /// <summary>
        /// Admin password to make changes to system
        /// </summary>
        public string AdminPassword { get; set; }


        /// <summary>
        /// Zipcode for weather API
        /// </summary>
        public string Zipcode { get; set; }

        /// <summary>
        /// Country for weather API
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// API Key for weather API
        /// </summary>
        public string APIKey { get; set; }

        /// <summary>
        /// Outside temperature 
        /// </summary>
        public double? OutsideTempF { get; set; }

        /// <summary>
        /// Outside temperature celcius
        /// </summary>
        public double? OutsideTempC { get; set; }

        /// <summary>
        /// Outside humidity
        /// </summary>
        public double? OutsideHumidity { get; set; }

        /// <summary>
        /// Wind speeds
        /// </summary>
        public double? WindSpeed { get; set; }

        /// <summary>
        /// Sunrise
        /// </summary>
        public DateTime? Sunrise { get; set; }

        /// <summary>
        /// Sunset
        /// </summary>
        public DateTime? Sunset { get; set; }

        /// <summary>
        /// Cloud coverage
        /// </summary>
        public double? CloudCoverage { get; set; }

        /// <summary>
        /// Rain
        /// </summary>
        public bool? Rain { get; set; }

        /// <summary>
        /// Used to draw a weather Icon
        /// </summary>
        public string WeatherIcon { get; set; }

        /// <summary>
        /// Last recorded water temperature
        /// </summary>
        public float? WaterTemp { get; set; }

        #region Extra settings
        /// <summary>
        /// Gets or sets extended settings
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public IExtendedSettings More { get; set; }

        /// <summary>
        /// Extended settings for serialization
        /// </summary>
        public string SettingA { get => More != null ? System.Text.Json.JsonSerializer.Serialize(More) : "{}";
            set => More = string.IsNullOrEmpty(value) ? new ExtendedSettings() : System.Text.Json.JsonSerializer.Deserialize<ExtendedSettings>(value);
        }

        /// <summary>
        /// Extended settings for serialization future use
        /// </summary>
        public string SettingB { get; set; }

        /// <summary>
        /// Extended settings for serialization future use
        /// </summary>
        public string SettingC { get; set; }
        #endregion

        /// <summary>
        /// DbContext
        /// </summary>
        private readonly AquaServiceDbContext dbContext;

        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Database is loaded and ready
        /// </summary>
        public bool SettingsLoaded { get; private set; }

        /// <summary>
        /// CTor
        /// </summary>
        public GlobalData(IConfiguration configuration, AquaServiceDbContext dbContext)
        {
            var gdx = new GlobalData();
            configuration.Bind("Default", gdx);
            this.configuration = configuration;
            this.dbContext = dbContext;
            this.TempPin = gdx.TempPin;
            this.TempType = gdx.TempType;
            this.DataCollectionRate = gdx.DataCollectionRate;
            this.AdminPassword = gdx.AdminPassword;
            this.APIKey = gdx.APIKey;
            this.Zipcode = gdx.Zipcode;
            this.Country = gdx.Country;
            this.More = gdx.More;
        }

        public GlobalData(IAppSetting settings, IEnumerable<WaterLevel> waterLevels, IEnumerable<PowerRelay> relays)
        {
            this.Relays = relays;
            this.WaterLevels = waterLevels;
            this.TempPin = settings.TempPin;
            this.TempType = settings.TempType;
            this.SettingsLoaded = true;
        }

        public void Load()
        {
            this.More = new ExtendedSettings();
            if(DateTime.Now.Hour <=6 || DateTime.Now.Hour > 21)
                WeatherIcon = "01n";
            else
                WeatherIcon = "01d";
            if (dbContext.GetSetting() == null)
            {
                try
                {
                    GlobalData gd = new GlobalData();
                    configuration.Bind("Default", gd);
                    dbContext.AddRelays(gd.Relays);
                    dbContext.AddWaterLevels(gd.WaterLevels);
                    dbContext.SaveSettings(new AppSetting() { TempType = gd.TempType, TempPin =  gd.TempPin, DataCollectionRate = 60, AdminPassword = "Fishy"});
                }
                catch
                {
                    // ignored
                }
            }
            this.WaterLevels = dbContext.GetWaterLevels().ToList();
            this.Relays = dbContext.GetRelays().ToList();            
            if (dbContext.GetSetting() != null)
            {
                IAppSetting setting = null;
                try
                {
                    setting = dbContext.GetSetting();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("no such column"))
                    {
                        // lets try to re-migrate
                        dbContext.Migrate();
                        try
                        {
                            setting = dbContext.GetSetting();
                        }
                        catch (Exception ex2)
                        {
                            if (ex2.Message.Contains("no such column"))
                            {
                                // we have to drop and recreate the database
                                throw new Exception("You must delete the database file or manually migrate your settings to your appsettings.json - " + ex2.Message);
                            }
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                this.TempPin = setting.TempPin;
                this.TempType = setting.TempType;
                this.DataCollectionRate = setting.DataCollectionRate;
                this.AdminPassword = setting.AdminPassword;
                this.APIKey = setting.APIKey;
                this.Zipcode = setting.Zipcode;
                this.Country = setting.Country;
                this.More = setting.More;
            }
            this.SettingsLoaded = true; // the application can finish starting
        }

        /// <summary>
        /// True or false if the System Operations service is online
        /// </summary>
        public bool SystemOnline { get { return systemOnline == 1; } set { systemOnline = (value == false ? -1 : 1); } }

        /// <summary>
        /// Rate at which data is collected
        /// </summary>
        public int DataCollectionRate { get; set; }

        private GlobalData() { }

        /// <summary>
        /// Creates an instance based on Json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static GlobalData Create(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<GlobalData>(json);
        }

        /// <summary>
        /// Gets the Relay based on relay letter
        /// </summary>
        /// <param name="relay"></param>
        /// <returns></returns>
        public PowerRelay GetRelay(RelayLocation relay)
        {
            if (Relays == null || Relays.Count() == 0)
                throw new Exception("No relays were defined");

            if (!Relays.Any(t => t.Letter == relay))
            {
                throw new Exception("No relays matched your relay");
            }
            return Relays.FirstOrDefault(t => t.Letter == relay);
        }
    }
}
