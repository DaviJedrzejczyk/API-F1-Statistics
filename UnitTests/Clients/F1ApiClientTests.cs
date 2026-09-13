using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ExternalApi.Impls;

namespace UnitTests
{
    [TestFixture]
    public class F1ApiClientTests
    {
        [Test]
        public void Constructor_SetsBaseAddress()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act
            var client = new F1ApiClient(httpClient);

            // Assert
            Assert.That(httpClient.BaseAddress, Is.EqualTo(new Uri("https://api.openf1.org/v1/")));
        }

        [Test]
        public async Task Get_WhenResponseIsSuccess_ReturnsSuccessSingleResponse()
        {
            // Arrange
            var expectedContent = "{\"data\":true}";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedContent)
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new F1ApiClient(httpClient);

            // Act
            var result = await apiClient.Get("endpoint", "?p=1");

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Item, Is.EqualTo(expectedContent));
            Assert.That(result.Message, Is.EqualTo("Sucesso"));

            handlerMock.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task Get_WhenResponseIsFailure_ReturnsFailureSingleResponseWithReason()
        {
            // Arrange
            var reason = "Bad Request Reason";
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = reason
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new F1ApiClient(httpClient);

            // Act
            var result = await apiClient.Get("endpoint", "");

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Error to fetch data: " + reason));

            handlerMock.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task Get_WhenHttpClientThrows_ReturnsFailureSingleResponseWithException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var ex = new InvalidOperationException("boom");
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(ex)
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new F1ApiClient(httpClient);

            // Act
            var result = await apiClient.Get("e", "p");

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Error to fetch data: " + ex.Message));
            Assert.That(result.Exception, Is.SameAs(ex));

            handlerMock.Protected().Verify("SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
