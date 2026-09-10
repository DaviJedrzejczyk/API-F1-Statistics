using Dao.Impl;
using Dao;
using Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace UnitTests.Dao
{
    [TestFixture]
    public class StintDaoTests
    {
        private ApiF1DB CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(dbName).Options;
            return new ApiF1DB(options);
        }

        [Test]
        public async Task GetStintsBySessionKey_ReturnsItems()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var stint = new Stint { SessionKey = 1, MeetingKey = 1, DriverNumber = 7 };
            await context.Stints.AddAsync(stint);
            await context.SaveChangesAsync();

            var dao = new StintDao(context);

            var response = await dao.GetStintsBySessionKey(1);

            Assert.That(response.HasSuccess, Is.True);
            Assert.That(response.Itens, Is.Not.Null);
            Assert.That(response.Itens.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task SaveStints_AddsToContextChangeTracker()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString());
            var dao = new StintDao(context);

            var stints = new List<Stint>
            {
                new Stint { SessionKey = 2, MeetingKey = 1, DriverNumber = 10 }
            };

            var response = await dao.SaveStints(stints);

            Assert.That(response.HasSuccess, Is.True);

            // Persist the changes and ensure items were added
            await context.SaveChangesAsync();
            var count = await context.Stints.CountAsync(s => s.SessionKey == 2);
            Assert.That(count, Is.EqualTo(1));
        }
    }
}
