using System;
using NUnit.Framework;
using Shared.Responses;

namespace UnitTests.Responses
{
    [TestFixture]
    public class ResponseFactoryTests
    {
        [Test]
        public void CreateSuccessResponse_Default_ReturnsSuccessWithDefaultMessage()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();

            // Act
            var response = factory.CreateSuccessResponse();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.HasSuccess);
            Assert.That(response.Message, Is.EqualTo("Sucesso"));
        }

        [Test]
        public void CreateFailureResponse_Default_ReturnsFailureWithDefaultMessage()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();

            // Act
            var response = factory.CreateFailureResponse();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(response.HasSuccess);
            Assert.That(response.Message, Is.EqualTo("Falha"));
        }

        [Test]
        public void CreateSuccessSingleResponse_WithItem_ReturnsSuccessAndItem()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var expected = 42;

            // Act
            var result = factory.CreateSuccessSingleResponse(expected);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Sucesso"));
            Assert.That(result.Item, Is.EqualTo(expected));
        }

        [Test]
        public void CreateSuccessSingleResponse_WithMessage_ReturnsSuccessWithProvidedMessageAndDefaultItem()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var message = "custom message";

            // Act
            var result = factory.CreateSuccessSingleResponse<string?>(message);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.IsNull(result.Item);
        }

        [Test]
        public void CreateFailureSingleResponse_Default_ReturnsFailureWithDefaultMessage()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();

            // Act
            var result = factory.CreateFailureSingleResponse<int>();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Falha"));
            Assert.That(result.Item, Is.EqualTo(default(int)));
        }

        [Test]
        public void CreateFailureSingleResponse_WithMessageAndException_ReturnsFailureAndExceptionPreserved()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var message = "failure message";
            var ex = new InvalidOperationException("boom");

            // Act
            var result = factory.CreateFailureSingleResponse<int>(message, ex);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.That(result.Exception, Is.SameAs(ex));
            Assert.That(result.Item, Is.EqualTo(default(int)));
        }

        [Test]
        public void CreateFailureSingleResponse_WithMessage_ReturnsFailureWithMessage()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var message = "only message";

            // Act
            var result = factory.CreateFailureSingleResponse<string?>(message);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.IsNull(result.Exception);
            Assert.IsNull(result.Item);
        }

        [Test]
        public void CreateFailureSingleResponse_WithException_ReturnsFailureWithExceptionMessage()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var ex = new ArgumentException("invalid arg");

            // Act
            var result = factory.CreateFailureSingleResponse<string?>(ex);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(ex.Message));
            Assert.That(result.Exception, Is.SameAs(ex));
            Assert.IsNull(result.Item);
        }

        [Test]
        public void CreateSuccessDataResponse_WithItems_ReturnsSuccessAndItemsPreserved()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var items = new System.Collections.Generic.List<int> { 1, 2, 3 };

            // Act
            var result = factory.CreateSuccessDataResponse(items);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Sucesso"));
            Assert.That(result.Itens, Is.SameAs(items));
        }

        [Test]
        public void CreateSuccessDataResponse_WithMessageOnly_ReturnsSuccessAndNullItems()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var message = "custom success";

            // Act
            var result = factory.CreateSuccessDataResponse<int>(message);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.IsNull(result.Itens);
        }


        [Test]
        public void CreateSuccessDataResponse_WithItemsAndMessage_ReturnsSuccessWithProvidedMessageAndItems()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var items = new System.Collections.Generic.List<int> { 7, 8, 9 };
            var message = "items and message";

            // Act
            var result = factory.CreateSuccessDataResponse(items, message);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.That(result.Itens, Is.SameAs(items));
        }

        [Test]
        public void CreateFailureDataResponse_Default_ReturnsFailureWithDefaultMessageAndNullItems()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();

            // Act
            var result = factory.CreateFailureDataResponse<int>();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Falha"));
            Assert.IsNull(result.Itens);
        }

        [Test]
        public void CreateFailureDataResponse_WithMessageAndException_ReturnsFailureAndExceptionPreserved()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var message = "failure with ex";
            var ex = new InvalidOperationException("fail");

            // Act
            var result = factory.CreateFailureDataResponse<int>(message, ex);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.That(result.Exception, Is.SameAs(ex));
            Assert.IsNull(result.Itens);
        }

        [Test]
        public void CreateFailureDataResponse_WithMessage_ReturnsFailureWithMessage()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var message = "only message";

            // Act
            var result = factory.CreateFailureDataResponse<string?>(message);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.IsNull(result.Exception);
            Assert.IsNull(result.Itens);
        }

        [Test]
        public void CreateFailureDataResponse_WithException_ReturnsFailureWithExceptionMessage()
        {
            // Arrange
            var factory = ResponseFactory.CreateInstance();
            var ex = new ArgumentException("invalid arg for data");

            // Act
            var result = factory.CreateFailureDataResponse<string?>(ex);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(ex.Message));
            Assert.That(result.Exception, Is.SameAs(ex));
            Assert.IsNull(result.Itens);
        }

    }
}
