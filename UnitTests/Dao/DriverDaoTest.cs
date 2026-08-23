using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dao;
using Dao.Impl;
using Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shared.Responses;

namespace UnitTests
{
    [TestFixture]
    public class DriverDaoTests
    {
        private DbContextOptions<ApiF1DB> _options = null!;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<ApiF1DB>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Test]
        public async Task DriverDao_Constructor_AssignsDbAndMethodsWork()
        {
            // Arrange
            await using var context = new ApiF1DB(_options);
            var dao = new DriverDao(context);
            var drivers = new List<Driver>
            {
                new Driver { DriverKey = 1, FirstName = "A" },
                new Driver { DriverKey = 2, FirstName = "B" }
            };

            // Act
            var insertResponse = await dao.InsertDrivers(drivers);

            // Assert
            Assert.IsTrue(insertResponse.HasSuccess);
            // AddRangeAsync adds to the change tracker; verify the items are present in local view
            Assert.That(context.Drivers.Local.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task InsertDrivers_DbDriversNull_ReturnsFailureResponse()
        {
            // Arrange
            await using var context = new ApiF1DB(_options);
            // Force Drivers to null to cause a NullReferenceException inside the DAO
            context.Drivers = null!;
            var dao = new DriverDao(context);

            // Act
            var response = await dao.InsertDrivers(new List<Driver> { new Driver { DriverKey = 1 } });

            // Assert
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
        }

        [Test]
        public async Task DeleteDriver_WhenDriverExists_MarksDeletedAndReturnsSuccess()
        {
            // Arrange
            await using var context = new ApiF1DB(_options);
            var driver = new Driver { DriverKey = 10, FirstName = "ToDelete" };
            context.Drivers.Add(driver);

            var dao = new DriverDao(context);

            // Act
            var response = await dao.DeleteDriver(driver);

            // Assert
            Assert.IsTrue(response.HasSuccess);
            var state = context.Entry(driver).State;
            Assert.IsTrue(state == EntityState.Deleted || state == EntityState.Detached);
        }

        [Test]
        public async Task DeleteDriver_DbDriversNull_ReturnsFailureResponse()
        {
            // Arrange
            await using var context = new ApiF1DB(_options);
            context.Drivers = null!;
            var dao = new DriverDao(context);
            var driver = new Driver { DriverKey = 20 };

            // Act
            var response = await dao.DeleteDriver(driver);

            // Assert
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
        }

        [Test]
        public async Task GetDriverById_WhenExists_ReturnsSuccessSingleResponseWithItem()
        {
            // Arrange
            await using var context = new ApiF1DB(_options);
            var driver = new Driver { DriverKey = 5, FirstName = "Found" };
            context.Drivers.Add(driver);

            var dao = new DriverDao(context);

            // Act
            var response = await dao.GetDriverById(driver.DriverKey);

            // Assert
            Assert.IsTrue(response.HasSuccess);
            Assert.IsNotNull(response.Item);
            Assert.That(response.Item!.DriverKey, Is.EqualTo(driver.DriverKey));
        }

        [Test]
        public async Task GetDriverById_NotFound_ReturnsSuccessWithNullItem()
        {
            // Arrange
            await using var context = new ApiF1DB(_options);
            var dao = new DriverDao(context);

            // Act
            var response = await dao.GetDriverById(99999);

            // Assert
            Assert.IsTrue(response.HasSuccess);
            Assert.IsNull(response.Item);
        }

        [Test]
        public async Task GetDriverById_DbDriversNull_ReturnsFailureSingleResponse()
        {
            // Arrange
            await using var context = new ApiF1DB(_options);
            context.Drivers = null!;
            var dao = new DriverDao(context);

            // Act
            var response = await dao.GetDriverById(1);

            // Assert
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
        }

        [Test]
        public async Task UpdateDriver_WhenCalled_MarksModifiedAndReturnsSuccess()
        {
            // Arrange
            await using var context = new ApiF1DB(_options);
            var driver = new Driver { DriverKey = 7, FirstName = "Before" };
            context.Drivers.Add(driver);

            driver.FirstName = "After";
            var dao = new DriverDao(context);

            // Act
            var response = await dao.UpdateDriver(driver);

            // Assert
            Assert.IsTrue(response.HasSuccess);
            var state = context.Entry(driver).State;
            Assert.That(state, Is.EqualTo(EntityState.Modified));
        }

        [Test]
        public async Task UpdateDriver_DbDriversNull_ReturnsFailureResponse()
        {
            // Arrange
            await using var context = new ApiF1DB(_options);
            context.Drivers = null!;
            var dao = new DriverDao(context);
            var driver = new Driver { DriverKey = 8 };

            // Act
            var response = await dao.UpdateDriver(driver);

            // Assert
            Assert.IsFalse(response.HasSuccess);
            Assert.IsNotNull(response.Exception);
        }
    }
}
