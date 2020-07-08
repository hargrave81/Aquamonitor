using System;
using System.ComponentModel.DataAnnotations;

namespace AquaMonitor.Data.Models
{
    public class Reading : IReading
    {
        /// <summary>
        /// primary Key
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Value recorded
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// Type of reading
        /// </summary>
        public ReadingType Type { get; set; }
        /// <summary>
        /// Scale reading is captured at
        /// </summary>
        public ReadingScale Scale { get; set; }
        /// <summary>
        /// Date and time reading was taken
        /// </summary>
        public DateTime Taken { get; set; }
        /// <summary>
        /// Location reading was recorded from
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Ideal reading minimum
        /// </summary>
        public double IdealMin { get; protected set; }
        /// <summary>
        /// Ideal reading maximum
        /// </summary>
        public double IdealMax { get; protected set; }
        /// <summary>
        /// Notes on reading
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// CTor
        /// </summary>
        public Reading()
        {

        }

        public static string ReadingColor(ReadingType type)
        {
            return type switch
            {
                ReadingType.PH => "#5fa8a1",
                ReadingType.Nitrate => "#a11726",
                ReadingType.Nitrite => "#7f207a",
                ReadingType.FishFeed => "#9e8e5e",
                ReadingType.Alkalinity => "#fbde16",
                ReadingType.Ammonia => "#0c7950",
                ReadingType.DisolvedOxygen => "#41a1e7",
                ReadingType.TotalHardness => "#934d35",
                ReadingType.WaterTemp => "#71b1d7",
                _ => "#000"
            };
        }


        public string ScaleString
        {
            get
            {
                return Scale switch
                {
                    ReadingScale.PPM => "ppm",
                    ReadingScale.Numeric => "",
                    ReadingScale.Celcius => "°c",
                    ReadingScale.Chunks => "Chnks",
                    ReadingScale.Farenheit => "°F",
                    ReadingScale.Grams => "g",
                    ReadingScale.Unknown => "",
                    _ => ""
                };
            }
        }

        /// <summary>
        /// Converts reading to reading type
        /// </summary>
        /// <returns></returns>
        public IReading ToType()
        {
            return Type switch
            {
                ReadingType.Nitrate => new NitrateReading(this),
                ReadingType.DisolvedOxygen => new DisolvedOxygenReading(this),
                ReadingType.Ammonia => new AmmoniaReading(this),
                ReadingType.PH => new PhReading(this),
                ReadingType.Alkalinity => new AlkalinityReading(this),
                ReadingType.FishFeed => new FishFeedReading(this),
                ReadingType.Nitrite => new NitriteReading(this),
                ReadingType.TotalHardness => new TotalHardnessReading(this),
                ReadingType.WaterTemp => new WaterTempReading(this),
                _ => this
            };
        }

        public void UpdateCreatedToLocal(DateTimeOffset timeZone)
        {            
            this.Taken = this.Taken.Add(timeZone.Offset);
        }
    }

    public class PhReading : Reading
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public PhReading()
        {
            IdealMin = 6.0;
            IdealMax = 7.0;
            Scale = ReadingScale.Numeric;
            Type = ReadingType.PH;
        }

        /// <summary>
        /// CTOR from reading
        /// </summary>
        /// <param name="reading"></param>
        public PhReading(IReading reading) : this()
        {
            this.Value = reading.Value;
            this.Location = reading.Location;
            this.Taken = reading.Taken;
            this.Note = reading.Note;
            if(reading.Scale != ReadingScale.Unknown)
                this.Scale = reading.Scale; // custom scale
        }
    }

    public class NitrateReading : Reading
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public NitrateReading()
        {
            IdealMin = 10;
            IdealMax = 150;
            Scale = ReadingScale.PPM;
            Type = ReadingType.Nitrate;
        }

        /// <summary>
        /// CTOR from reading
        /// </summary>
        /// <param name="reading"></param>
        public NitrateReading(IReading reading) : this()
        {
            this.Value = reading.Value;
            this.Location = reading.Location;
            this.Taken = reading.Taken;
            this.Note = reading.Note;
            if(reading.Scale != ReadingScale.Unknown)
                this.Scale = reading.Scale; // custom scale
        }
    }

    public class NitriteReading : Reading
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public NitriteReading()
        {
            IdealMin = 0;
            IdealMax = .25;
            Scale = ReadingScale.PPM;
            Type = ReadingType.Nitrite;
        }

        /// <summary>
        /// CTOR from reading
        /// </summary>
        /// <param name="reading"></param>
        public NitriteReading(IReading reading) : this()
        {
            this.Value = reading.Value;
            this.Location = reading.Location;
            this.Taken = reading.Taken;
            this.Note = reading.Note;
            if(reading.Scale != ReadingScale.Unknown)
                this.Scale = reading.Scale; // custom scale
        }
    }

    public class AmmoniaReading : Reading
    {

        /// <summary>
        /// CTOR
        /// </summary>
        public AmmoniaReading()
        {
            IdealMin = 0;
            IdealMax = .5;
            Scale = ReadingScale.PPM;
            Type = ReadingType.Ammonia;
        }

        /// <summary>
        /// CTOR from reading
        /// </summary>
        /// <param name="reading"></param>
        public AmmoniaReading(IReading reading) : this()
        {
            this.Value = reading.Value;
            this.Location = reading.Location;
            this.Taken = reading.Taken;
            this.Note = reading.Note;
            if(reading.Scale != ReadingScale.Unknown)
                this.Scale = reading.Scale; // custom scale
        }
    }


    public class DisolvedOxygenReading : Reading
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public DisolvedOxygenReading()
        {
            IdealMin = 5;
            IdealMax = 50;
            Scale = ReadingScale.PPM;
            Type = ReadingType.DisolvedOxygen;
        }

        /// <summary>
        /// CTOR from reading
        /// </summary>
        /// <param name="reading"></param>
        public DisolvedOxygenReading(IReading reading) : this()
        {
            this.Value = reading.Value;
            this.Location = reading.Location;
            this.Taken = reading.Taken;
            this.Note = reading.Note;
            if(reading.Scale != ReadingScale.Unknown)
                this.Scale = reading.Scale; // custom scale
        }
    }


    
    public class WaterTempReading : Reading
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public WaterTempReading()
        {
            IdealMin = 65;
            IdealMax = 88;
            Scale = ReadingScale.Farenheit;
            Type = ReadingType.WaterTemp;
        }

        /// <summary>
        /// CTOR from reading
        /// </summary>
        /// <param name="reading"></param>
        public WaterTempReading(IReading reading) : this()
        {
            this.Value = reading.Value;
            this.Location = reading.Location;
            this.Taken = reading.Taken;
            this.Note = reading.Note;
            if(reading.Scale != ReadingScale.Unknown)
                this.Scale = reading.Scale; // custom scale
        }
    }

    public class TotalHardnessReading : Reading
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public TotalHardnessReading()
        {
            IdealMin = 50;
            IdealMax = 100;
            Scale = ReadingScale.PPM;
            Type = ReadingType.TotalHardness;
        }

        /// <summary>
        /// CTOR from reading
        /// </summary>
        /// <param name="reading"></param>
        public TotalHardnessReading(IReading reading) : this()
        {
            this.Value = reading.Value;
            this.Location = reading.Location;
            this.Taken = reading.Taken;
            this.Note = reading.Note;
            if(reading.Scale != ReadingScale.Unknown)
                this.Scale = reading.Scale; // custom scale
        }
    }

    public class AlkalinityReading : Reading
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public AlkalinityReading()
        {
            IdealMin = 100;
            IdealMax = 500;
            Scale = ReadingScale.PPM;
            Type = ReadingType.Alkalinity;
        }

        /// <summary>
        /// CTOR from reading
        /// </summary>
        /// <param name="reading"></param>
        public AlkalinityReading(IReading reading) : this()
        {
            this.Value = reading.Value;
            this.Location = reading.Location;
            this.Taken = reading.Taken;
            this.Note = reading.Note;
            if(reading.Scale != ReadingScale.Unknown)
                this.Scale = reading.Scale; // custom scale
        }
    }

    public class FishFeedReading : Reading
    {
        /// <summary>
        /// CTOR
        /// </summary>
        public FishFeedReading()
        {
            IdealMin = 1;
            IdealMax = 100;
            Scale = ReadingScale.Grams;
            Type = ReadingType.FishFeed;
        }

        /// <summary>
        /// CTOR from reading
        /// </summary>
        /// <param name="reading"></param>
        public FishFeedReading(IReading reading) : this()
        {
            this.Value = reading.Value;
            this.Location = reading.Location;
            this.Taken = reading.Taken;
            this.Note = reading.Note;
            if(reading.Scale != ReadingScale.Unknown)
                this.Scale = reading.Scale; // custom scale
        }
    }
}
