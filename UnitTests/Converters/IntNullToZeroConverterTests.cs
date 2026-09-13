using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Moq;
using NUnit.Framework;
using Shared.Converters;

#nullable enable

namespace UnitTests.Converters
{
    [TestFixture]
    public class IntNullToZeroConverterTests
    {
        [Test]
        public void Read_NullToken_ReturnsZero()
        {
            // Arrange
            var json = "null";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            var converter = new IntNullToZeroConverter();
            // advance to the first token
            reader.Read();


            // Act
            var result = converter.Read(ref reader, typeof(int), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void Read_NumberToken_ReturnsIntValue()
        {
            // Arrange
            var json = "123";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            // advance to the first token
            reader.Read();

            var converter = new IntNullToZeroConverter();

            // Act
            var result = converter.Read(ref reader, typeof(int), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.EqualTo(123));
        }

        [Test]
        public void Write_WritesNumberValue()
        {
            // Arrange
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            var converter = new IntNullToZeroConverter();

            // Act
            converter.Write(writer, 42, new JsonSerializerOptions());
            writer.Flush();
            var written = Encoding.UTF8.GetString(stream.ToArray());

            // Assert
            Assert.That(written, Is.EqualTo("42"));
        }
    }
}
