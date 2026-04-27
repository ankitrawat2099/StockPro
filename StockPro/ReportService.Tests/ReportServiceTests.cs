using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace ReportService.Tests
{
    [TestClass]
    public class ReportServiceTests
    {
        private Mock<IReportRepository>? _mockRepo;
        private Mock<IHttpClientFactory>? _mockHttpClientFactory;
        private Mock<IConfiguration>? _mockConfig;
        private ReportServiceImpl? _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<IReportRepository>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockConfig = new Mock<IConfiguration>();
            _service = new ReportServiceImpl(_mockRepo.Object, _mockHttpClientFactory.Object, _mockConfig.Object);
        }

        [TestMethod]
        public async Task GetTotalStockValue_ShouldReturnSumOfValues()
        {
            // Arrange
            var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            var today = DateOnly.FromDateTime(istNow);
            
            var snapshots = new List<InventorySnapshot>
            {
                new InventorySnapshot { StockValue = 100 },
                new InventorySnapshot { StockValue = 200 }
            };

            _mockRepo!.Setup(r => r.FindBySnapshotDate(today)).ReturnsAsync(snapshots);

            // Act
            var result = await _service!.GetTotalStockValue();

            // Assert
            Assert.AreEqual(300, result);
        }

        [TestMethod]
        public async Task GetStockValueByWarehouse_ShouldCallRepository()
        {
            // Arrange
            var whId = 1;
            _mockRepo!.Setup(r => r.SumStockValueByWarehouse(whId)).ReturnsAsync(500.0);

            // Act
            var result = await _service!.GetStockValueByWarehouse(whId);

            // Assert
            Assert.AreEqual(500.0, result);
        }
    }
}
