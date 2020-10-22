using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AquaMonitor.Data
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                var input = reader.GetString();
                return TimeSpan.Parse(input);
            }
            catch {}
            var ts = TimeSpan.Zero;
            try
            {
                reader.Read();
                reader.Read();
                bool hasValue = reader.GetBoolean(); 
                if (hasValue)
                {
                    reader.Read();
                    reader.Read();
                    reader.Read();
                    var field = reader.GetString();
                    if (field == "Ticks")
                    {
                        reader.Read();
                        ts = new TimeSpan(reader.GetInt64());
                    }

                    
                }
            }
            catch { }

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                reader.Read();
            }
            reader.Read();
            return ts;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
