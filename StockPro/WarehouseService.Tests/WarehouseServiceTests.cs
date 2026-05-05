using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace WarehouseService.Tests
{
    [TestClass]
    public class WarehouseServiceTests
    {
        private Mock<IWarehouseRepository>? _mockRepo;
        private Mock<IHttpClientFactory>? _mockHttpClientFactory;
        private Mock<IHttpContextAccessor>? _mockHttpContextAccessor;
        private Mock<IConfiguration>? _mockConfig;
        private WarehouseDbContext? _context;
        private WarehouseServiceImpl? _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<IWarehouseRepository>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockConfig = new Mock<IConfiguration>();

            var options = new DbContextOptionsBuilder<WarehouseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WarehouseDbContext(options);
            _service = new WarehouseServiceImpl(
                _mockRepo.Object, 
                _context, 
                _mockHttpClientFactory.Object, 
                _mockHttpContextAccessor.Object, 
                _mockConfig.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public async Task CreateWarehouseAsync_ShouldReturnSavedWarehouse()
        {
            // Arrange
            var warehouse = new Warehouse { Name = "Main", Location = "City", Address = "Street", Phone = "123" };
            _mockRepo!.Setup(r => r.SaveWarehouseAsync(warehouse)).ReturnsAsync(warehouse);

            // Act
            var result = await _service!.CreateWarehouseAsync(warehouse);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(warehouse.Name, result.Name);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnWarehouse_WhenExists()
        {
            // Arrange
            var whId = 1;
            var warehouse = new Warehouse { WarehouseId = whId, Name = "Main", IsActive = true, Location = "L", Address = "A", Phone = "P" };
            _mockRepo!.Setup(r => r.FindByWarehouseIdAsync(whId)).ReturnsAsync(warehouse);

            // Act
            var result = await _service!.GetByIdAsync(whId);

            // Assert
            Assert.AreEqual(whId, result.WarehouseId);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldThrow_WhenNotExists()
        {
            // Arrange
            _mockRepo!.Setup(r => r.FindByWarehouseIdAsync(99)).ReturnsAsync((Warehouse?)null);

            // Act & Assert
            try
            {
                await _service!.GetByIdAsync(99);
                Assert.Fail("KeyNotFoundException expected");
            }
            catch (KeyNotFoundException)
            {
                // Success
            }
        }

        [TestMethod]
        public async Task DeactivateWarehouseAsync_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var whId = 1;
            var warehouse = new Warehouse { WarehouseId = whId, Name = "Main", IsActive = true, Location = "L", Address = "A", Phone = "P" };
            _mockRepo!.Setup(r => r.FindByWarehouseIdAsync(whId)).ReturnsAsync(warehouse);

            // Act
            await _service!.DeactivateWarehouseAsync(whId);

            // Assert
            Assert.IsFalse(warehouse.IsActive);
            _mockRepo.Verify(r => r.UpdateWarehouseAsync(warehouse), Times.Once);
        }
    }
}
