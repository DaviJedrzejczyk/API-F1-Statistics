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
            _service = new StintService(_clientMock.Object, _unityMock.Object);
        }

        [Test]
        public async Task GetStintsBySessionKey_ReturnsDbWhenDbHasItems()
        {
            int sessionKey = 1;
            var stints = new List<Stint> { new Stint { SessionKey = sessionKey } };

            _unityMock.Setup(x => x.StintDao.GetStintsBySessionKey(sessionKey)).ReturnsAsync(new DataResponse<Stint> { Itens = stints, HasSuccess = true });

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
            _clientMock.Setup(x => x.GetAllStintsBySession(sessionKey)).ReturnsAsync(new DataResponse<Stint> { HasSuccess = false, Message = "API error" });

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

            _unityMock.Setup(x => x.StintDao.GetStintsBySessionKey(sessionKey)).ReturnsAsync(new DataResponse<Stint> { Itens = new List<Stint>(), HasSuccess = true });
            _clientMock.Setup(x => x.GetAllStintsBySession(sessionKey)).ReturnsAsync(new DataResponse<Stint> { Itens = stints, HasSuccess = true });
            _unityMock.Setup(x => x.StintDao.SaveStints(stints)).ReturnsAsync(new Response { HasSuccess = true });
            _unityMock.Setup(x => x.Commit()).ReturnsAsync(new Response { HasSuccess = true });

            var response = await _service.GetStintsBySessionKey(sessionKey);

            Assert.Multiple(() =>
            {
                Assert.That(response.HasSuccess, Is.True);
                Assert.That(response.Itens, Is.Not.Null);
                Assert.That(response.Itens.Count, Is.EqualTo(1));
                Assert.That(response.Message, Is.EqualTo("Stints retrieved from API and saved to database successfully."));
            });
        }

        [Test]
        public async Task GetStintsBySessionKey_ReturnsFailureWhenSaveFails()
        {
            int sessionKey = 1;
            var stints = new List<Stint> { new Stint { SessionKey = sessionKey } };

            _unityMock.Setup(x => x.StintDao.GetStintsBySessionKey(sessionKey)).ReturnsAsync(new DataResponse<Stint> { Itens = new List<Stint>(), HasSuccess = true });
            _clientMock.Setup(x => x.GetAllStintsBySession(sessionKey)).ReturnsAsync(new DataResponse<Stint> { Itens = stints, HasSuccess = true });
            _unityMock.Setup(x => x.StintDao.SaveStints(stints)).ReturnsAsync(new Response { HasSuccess = false, Message = "Save failed" });

            var response = await _service.GetStintsBySessionKey(sessionKey);

            Assert.Multiple(() =>
            {
                Assert.That(response.HasSuccess, Is.False);
                Assert.That(response.Message, Is.EqualTo("Save failed"));
            });
        }
    }
}
