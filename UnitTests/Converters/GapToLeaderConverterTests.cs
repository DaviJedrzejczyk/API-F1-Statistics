using System.Buffers;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace UnitTests.Converters
{
    [TestFixture]
    public class GapToLeaderConverterTests
    {
        private Shared.Converters.GapToLeaderConverter _converter = null!;

        [SetUp]
        public void SetUp()
        {
            _converter = new Shared.Converters.GapToLeaderConverter();
        }

        [Test]
        public void Read_NullToken_ReturnsEmptyString()
        {
            // Arrange
            var json = "null";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read();

            // Act
            var result = _converter.Read(ref reader, typeof(string), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Read_NumberToken_ReturnsNumberString()
        {
            // Arrange
            var json = "1.23";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read();

            // Act
            var result = _converter.Read(ref reader, typeof(string), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.EqualTo(1.23.ToString()));
        }

        [Test]
        public void Read_StringToken_ReturnsString()
        {
            // Arrange
            var json = "\"abc\"";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read();

            // Act
            var result = _converter.Read(ref reader, typeof(string), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.EqualTo("abc"));
        }

        [Test]
        public void Read_OtherToken_ReturnsEmptyString()
        {
            // Arrange
            var json = "true";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read();

            // Act
            var result = _converter.Read(ref reader, typeof(string), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Read_Array_MixedElements_ReturnsCombinedString()
        {
            // Arrange
            var json = "[null, 1.5, \"2.5\", \"x\", true]";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read(); // move to StartArray

            // Act
            var result = _converter.Read(ref reader, typeof(string), new JsonSerializerOptions());

            // Assert
            // Build expected string using current culture parsing/formatting where the code does.
            var expected = string.Empty;
            // null element is appended as literal "0.0,"
            expected += "0.0,";
            // numeric element 1.5 is formatted via GetDouble().ToString() (current culture)
            expected += (1.5).ToString() + ";";
            // string "2.5" is parsed with double.TryParse using current culture
            if (double.TryParse("2.5", out var parsed))
                expected += parsed.ToString() + ",";
            else
                expected += "0.0,";
            // unparsable string -> literal "0.0,"
            expected += "0.0,";
            // boolean true -> else -> "0.0,"
            expected += "0.0,";

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Write_NullValue_WritesNull()
        {
            // Arrange
            var buffer = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);

            // Act
            string? nullValue = null;
            _converter.Write(writer, nullValue!, new JsonSerializerOptions());
            writer.Flush();
            var json = Encoding.UTF8.GetString(buffer.WrittenSpan);

            // Assert
            Assert.That(json, Is.EqualTo("null"));
        }

        [Test]
        public void Write_EmptyString_WritesEmptyArray()
        {
            // Arrange
            var buffer = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);

            // Act
            _converter.Write(writer, string.Empty, new JsonSerializerOptions());
            writer.Flush();
            var json = Encoding.UTF8.GetString(buffer.WrittenSpan);

            // Assert
            using var doc = JsonDocument.Parse(json);
            Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(doc.RootElement.GetArrayLength(), Is.EqualTo(0));
        }

        [Test]
        public void Write_Values_WritesCorrectNumbers()
        {
            // Arrange
            var buffer = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);

            var value = "1,2.5,foo";

            // Act
            _converter.Write(writer, value, new JsonSerializerOptions());
            writer.Flush();
            var json = Encoding.UTF8.GetString(buffer.WrittenSpan);

            // Assert
            using var doc = JsonDocument.Parse(json);
            Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
            var arr = doc.RootElement.EnumerateArray();
            var elements = new System.Collections.Generic.List<double>();
            foreach (var el in arr)
                elements.Add(el.GetDouble());

            // Build expected values using the same parsing logic the converter uses (current culture)
            var expectedValues = new System.Collections.Generic.List<double>();
            if (double.TryParse("1", out var d1))
                expectedValues.Add(d1);
            else
                expectedValues.Add(0.0);

            if (double.TryParse("2.5", out var d2))
                expectedValues.Add(d2);
            else
                expectedValues.Add(0.0);

            if (double.TryParse("foo", out var d3))
                expectedValues.Add(d3);
            else
                expectedValues.Add(0.0);

            Assert.That(elements.Count, Is.EqualTo(expectedValues.Count));
            for (int i = 0; i < expectedValues.Count; i++)
                Assert.That(elements[i], Is.EqualTo(expectedValues[i]));
        }
    }
}
