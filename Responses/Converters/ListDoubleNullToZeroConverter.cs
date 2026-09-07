using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Converters
{
    public class ListDoubleNullToZeroConverter : JsonConverter<List<double>>
    {
        public override List<double> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new List<double>();
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                // Handle a single number (non-array) gracefully
                if (reader.TokenType == JsonTokenType.Number)
                {
                    return new List<double> { reader.GetDouble() };
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();
                    if (double.TryParse(s, out var d))
                        return new List<double> { d };
                }

                // Fallback to empty list for unexpected token types
                return new List<double>();
            }

            var list = new List<double>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.Null)
                {
                    list.Add(0.0);
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    list.Add(reader.GetDouble());
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();
                    if (double.TryParse(s, out var d))
                        list.Add(d);
                    else
                        list.Add(0.0);
                }
                else
                {
                    // For any other token, treat as zero to preserve list shape
                    list.Add(0.0);
                }
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<double> value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var v in value)
            {
                writer.WriteNumberValue(v);
            }
            writer.WriteEndArray();
        }
    }
}
