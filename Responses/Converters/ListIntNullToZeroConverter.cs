using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Converters
{
    public class ListIntNullToZeroConverter : JsonConverter<List<int>>
    {
        public override List<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new List<int>();
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                // Handle a single number (non-array) gracefully
                if (reader.TokenType == JsonTokenType.Number)
                {
                    return new List<int> { reader.GetInt32() };
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();
                    if (int.TryParse(s, out var d))
                        return new List<int> { d };
                }

                // Fallback to empty list for unexpected token types
                return new List<int>();
            }

            var list = new List<int>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.Null)
                {
                    list.Add(0);
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    list.Add(reader.GetInt32());
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();
                    if (int.TryParse(s, out var d))
                        list.Add(d);
                    else
                        list.Add(0);
                }
                else
                {
                    list.Add(0);
                }
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<int> value, JsonSerializerOptions options)
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
