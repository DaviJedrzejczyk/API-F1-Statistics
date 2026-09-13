using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using Shared.Converters;

namespace UnitTests.Converters
{
    [TestFixture]
    public class ListDoubleNullToZeroConverterTests
    {
        [Test]
        public void Read_NullToken_ReturnsEmptyList()
        {
            // Arrange
            var json = "null";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read(); // position on Null
            var sut = new ListDoubleNullToZeroConverter();

            // Act
            var result = sut.Read(ref reader, typeof(List<double>), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void Read_SingleNumber_ReturnsListWithNumber()
        {
            // Arrange
            var json = "123.5";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read(); // position on Number
            var sut = new ListDoubleNullToZeroConverter();

            // Act
            var result = sut.Read(ref reader, typeof(List<double>), new JsonSerializerOptions());

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(123.5));
        }

        [Test]
        public void Read_SingleStringNumber_ReturnsListWithParsedNumber()
        {
            // Arrange
            var json = "\"5\"";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read(); // position on String
            var sut = new ListDoubleNullToZeroConverter();

            // Act
            var result = sut.Read(ref reader, typeof(List<double>), new JsonSerializerOptions());

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(5.0));
        }

        [Test]
        public void Read_SingleStringNonNumber_ReturnsEmptyList()
        {
            // Arrange
            var json = "\"x\"";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read(); // position on String
            var sut = new ListDoubleNullToZeroConverter();

            // Act
            var result = sut.Read(ref reader, typeof(List<double>), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void Read_SingleUnexpectedToken_ReturnsEmptyList()
        {
            // Arrange
            var json = "true";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read(); // position on True
            var sut = new ListDoubleNullToZeroConverter();

            // Act
            var result = sut.Read(ref reader, typeof(List<double>), new JsonSerializerOptions());

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void Read_ArrayWithVariousTokens_ReturnsTransformedList()
        {
            // Arrange
            var json = "[null, 1.5, \"2\", \"x\", true]";
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read(); // position on StartArray
            var sut = new ListDoubleNullToZeroConverter();

            // Act
            var result = sut.Read(ref reader, typeof(List<double>), new JsonSerializerOptions());

            // Assert
            CollectionAssert.AreEqual(new List<double> { 0.0, 1.5, 2.0, 0.0, 0.0 }, result);
        }

        [Test]
        public void Write_NullValue_WritesNull()
        {
            // Arrange
            var sut = new ListDoubleNullToZeroConverter();
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms);

            // Act
#pragma warning disable CS8625 // Nullability: passing null to non-nullable parameter to exercise behavior
            sut.Write(writer, (List<double>?)null!, new JsonSerializerOptions());
#pragma warning restore CS8625
            writer.Flush();
            var output = Encoding.UTF8.GetString(ms.ToArray());

            // Assert
            Assert.That(output, Is.EqualTo("null"));
        }

        [Test]
        public void Write_List_WritesArrayOfNumbers()
        {
            // Arrange
            var sut = new ListDoubleNullToZeroConverter();
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms);
            var value = new List<double> { 1.1, 2.2, 3.3 };

            // Act
            sut.Write(writer, value, new JsonSerializerOptions());
            writer.Flush();
            var output = Encoding.UTF8.GetString(ms.ToArray());

            // Assert
            Assert.That(output, Is.EqualTo("[1.1,2.2,3.3]"));
        }
    }
}
