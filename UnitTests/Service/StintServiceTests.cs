using Dao.Interface;
using Dao;
using ExternalApi.Interfaces;
using Moq;
using Services.Impl;
using Shared.Responses;
using NUnit.Framework;
using Entities.Class;

namespace UnitTests.Service
{
    [TestFixture]
    public class StintServiceTests
    {
        private Mock<IUnityOfWork> _unityMock = null!;
        private Mock<IStintClient> _clientMock = null!;
        private StintService _service = null!;

        [SetUp]
        public void Setup()
        {
            _unityMock = new Mock<IUnityOfWork>();
            _clientMock = new Mock<IStintClient>();
            _unityMock.Setup(x => x.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::Dao.Cache.ImportWindow?)null);
            _unityMock.Setup(x => x.StageImportWindow(It.IsAny<global::Dao.Cache.ImportWindow>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var transaction = new Mock<IImportTransaction>();
            transaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unityMock.Setup(x => x.BeginImport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
            _service = new StintService(_clientMock.Object, _unityMock.Object);
        }

        [Test]
        public async Task GetStintsBySessionKey_ReturnsCompleteValidWindow()
        {
            int sessionKey = 1;
            var stints = new List<Stint> { new Stint { SessionKey = sessionKey } };

            _unityMock.Setup(x => x.GetImportWindow("stints", "1", It.IsAny<CancellationToken>())).ReturnsAsync(new global::Dao.Cache.ImportWindow { IsComplete = true, CompletedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1), ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(1), Payload = System.Text.Json.JsonSerializer.Serialize(stints) });

            var response = await _service.GetStintsBySessionKey(sessionKey);

            Assert.That(response.HasSuccess, Is.True);
            Assert.That(response.Itens, Is.Not.Null);
            Assert.That(response.Itens.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetStintsBySessionKey_ReturnsApiFailureWhenApiFails()
        {
            int sessionKey = 1;

            _unityMock.Setup(x => x.StintDao.GetStintsBySessionKey(sessionKey)).ReturnsAsync(new DataResponse<Stint> { Itens = new List<Stint>(), HasSuccess = true });
            _clientMock.Setup(x => x.GetAllStintsBySession(sessionKey, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Stint> { HasSuccess = false, Message = "API error" });

            var response = await _service.GetStintsBySessionKey(sessionKey);

            Assert.Multiple(() =>
            {
                Assert.That(response.HasSuccess, Is.False);
                Assert.That(response.Message, Is.EqualTo("API error"));
            });
        }

        [Test]
        public async Task GetStintsBySessionKey_RetrievesFromApiAndSaves()
        {
            int sessionKey = 1;
            var stints = new List<Stint> { new Stint { SessionKey = sessionKey } };

            _unityMock.Setup(x => x.StintDao.GetStintsBySessionKey(sessionKey, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Stint> { Itens = new List<Stint>(), HasSuccess = true });
            _clientMock.Setup(x => x.GetAllStintsBySession(sessionKey, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Stint> { Itens = stints, HasSuccess = true });
            _unityMock.Setup(x => x.StintDao.SaveStints(It.IsAny<List<Stint>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response { HasSuccess = true });
            _unityMock.Setup(x => x.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(new Response { HasSuccess = true });

            var response = await _service.GetStintsBySessionKey(sessionKey);

            Assert.Multiple(() =>
            {
                Assert.That(response.HasSuccess, Is.True);
                Assert.That(response.Itens, Is.Not.Null);
                Assert.That(response.Itens.Count, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task GetStintsBySessionKey_ReturnsFailureWhenSaveFails()
        {
            int sessionKey = 1;
            var stints = new List<Stint> { new Stint { SessionKey = sessionKey } };

            _unityMock.Setup(x => x.StintDao.GetStintsBySessionKey(sessionKey, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Stint> { Itens = new List<Stint>(), HasSuccess = true });
            _clientMock.Setup(x => x.GetAllStintsBySession(sessionKey, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Stint> { Itens = stints, HasSuccess = true });
            _unityMock.Setup(x => x.StintDao.SaveStints(It.IsAny<List<Stint>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response { HasSuccess = false, Message = "Save failed" });

            var response = await _service.GetStintsBySessionKey(sessionKey);

            Assert.Multiple(() =>
            {
                Assert.That(response.HasSuccess, Is.False);
                Assert.That(response.Message, Is.EqualTo("Save failed"));
            });
        }
    }
}
