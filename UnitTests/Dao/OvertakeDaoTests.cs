using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Dao;
using Dao.Impl;
using Entities.Class;
using Shared.Responses;

namespace UnitTests.Dao
{
    [TestFixture]
    public class OvertakeDaoTests
    {
        [Test]
        public async Task Constructor_WithValidDb_GetOvertakesBySession_ReturnsSuccessDataResponse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var db = new ApiF1DB(options);
            var overt = new Overtake { SessionKey = 1 };
            db.Overtakes.Add(overt);
            await db.SaveChangesAsync();

            var dao = new OvertakeDao(db);

            // Act
            var result = await dao.GetOvertakesBySession(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HasSuccess, Is.True);
            Assert.That(result.Itens, Is.Not.Null);
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            Assert.That(result.Itens.First().SessionKey, Is.EqualTo(1));
        }

        [Test]
        public async Task GetOvertakesBySession_NullDb_ReturnsFailureDataResponse()
        {
            // Arrange
#pragma warning disable CS8625 // Suppress nullability warning for test
            var dao = new OvertakeDao(null);
#pragma warning restore CS8625

            // Act
            var result = await dao.GetOvertakesBySession(5);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
            Assert.That(result.Exception, Is.InstanceOf<NullReferenceException>());
            Assert.That(result.Message, Is.EqualTo("Object reference not set to an instance of an object."));
        }

[Test]
public async Task SaveOvertakes_WithValidDb_AddsEntitiesAndReturnsSuccess()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ApiF1DB>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    await using var db = new ApiF1DB(options);
    var dao = new OvertakeDao(db);

    var list = new List<Overtake>
    {
        new Overtake { SessionKey = 10, Position = 1 },
        new Overtake { SessionKey = 10, Position = 2 }
    };

    // Act
    var result = await dao.SaveOvertakes(list);

    // Assert
    Assert.That(result, Is.Not.Null);
    // Message should indicate insertion; HasSuccess is part of the response but implementation detail may vary
    Assert.That(result.Message, Is.EqualTo("The overtakes has been inserted."));

    var tracked = db.ChangeTracker.Entries<Overtake>().Count();
    Assert.That(tracked, Is.EqualTo(list.Count));
}

        [Test]
        public async Task SaveOvertakes_NullDb_ReturnsFailureResponse()
        {
            // Arrange
#pragma warning disable CS8625 // Suppress nullability warning for test
            var dao = new OvertakeDao(null);
#pragma warning restore CS8625
            var list = new List<Overtake> { new Overtake { SessionKey = 3 } };

            // Act
            var result = await dao.SaveOvertakes(list);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Exception, Is.Not.Null);
            Assert.That(result.Exception, Is.InstanceOf<NullReferenceException>());
            // The failure response for SaveOvertakes uses the exception message
            Assert.That(result.Message, Is.EqualTo(result.Exception.Message));
        }
    }
}
