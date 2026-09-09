using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Converters
{
    public class GapToLeaderConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return string.Empty;
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetDouble().ToString();
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    return reader.GetString();
                }
                return string.Empty;
            }

            string text = "";
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.Null)
                {
                    text += "0.0,";

                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    text += reader.GetDouble().ToString() + ";";
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();
                    if (double.TryParse(s, out var d))
                        text += d.ToString() + ",";
                    else
                        text += "0.0,";
                }
                else
                {
                    text += "0.0,";
                }
            }

            return text;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var v in values)
            {
                if (double.TryParse(v, out var d))
                {
                    writer.WriteNumberValue(d);
                }
                else
                {
                    writer.WriteNumberValue(0.0);
                }
            }
            writer.WriteEndArray();
        }
    }
}
