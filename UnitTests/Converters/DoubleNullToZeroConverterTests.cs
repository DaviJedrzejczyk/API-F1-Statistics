using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using Shared.Converters;

namespace UnitTests.Converters
{
    [TestFixture]
    public class DoubleNullToZeroConverterTests
    {
        [Test]
        public void Read_NullToken_ReturnsZero()
        {
            // Arrange
            var json = "null";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            Assert.IsTrue(reader.Read(), "Reader should have a token to read");
            var converter = new DoubleNullToZeroConverter();

            // Act
            double result = converter.Read(ref reader, typeof(double), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.EqualTo(0.0));
        }

        [Test]
        public void Read_NumberToken_ReturnsValue()
        {
            // Arrange
            var value = 3.1415;
            var json = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            Assert.IsTrue(reader.Read(), "Reader should have a token to read");
            var converter = new DoubleNullToZeroConverter();

            // Act
            double result = converter.Read(ref reader, typeof(double), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void Write_WritesNumberValue()
        {
            // Arrange
            var value = -42.75;
            var buffer = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);
            var converter = new DoubleNullToZeroConverter();

            // Act
            converter.Write(writer, value, new JsonSerializerOptions());
            writer.Flush();
            var written = Encoding.UTF8.GetString(buffer.WrittenSpan);

            // Assert
            // The writer should have written the numeric representation of the value
            // Parse the produced JSON to verify it is a number with the same value
            using var doc = JsonDocument.Parse(written);
            Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Number));
            Assert.That(doc.RootElement.GetDouble(), Is.EqualTo(value));
        }
    }
}
