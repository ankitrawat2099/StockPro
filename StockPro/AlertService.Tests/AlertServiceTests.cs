using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AlertService.Tests
{
    [TestClass]
    public class AlertServiceTests
    {
        private Mock<IAlertRepository>? _mockRepo;
        private Mock<IConfiguration>? _mockConfig;
        private AlertServiceImpl? _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<IAlertRepository>();
            _mockConfig = new Mock<IConfiguration>();
            _service = new AlertServiceImpl(_mockRepo.Object, _mockConfig.Object);
        }

        [TestMethod]
        public async Task SendAlertAsync_ShouldSetCreatedAtAndCallRepository()
        {
            // Arrange
            var alert = new Alert { RecipientId = 1, Title = "T", Message = "M", Severity = "INFO" };

            // Act
            await _service!.SendAlertAsync(alert);

            // Assert
            Assert.IsFalse(alert.IsRead);
            Assert.IsFalse(alert.IsAcknowledged);
            _mockRepo!.Verify(r => r.AddAsync(alert), Times.Once);
        }

        [TestMethod]
        public async Task SendLowStockAlertAsync_ShouldNotCreate_WhenAlreadyExists()
        {
            // Arrange
            var pId = Guid.NewGuid();
            var whId = 1;
            var existing = new List<Alert> { new Alert { RelatedWarehouseId = whId, IsAcknowledged = false } };
            _mockRepo!.Setup(r => r.FindByRelatedProductIdAsync(pId)).ReturnsAsync(existing);

            // Act
            await _service!.SendLowStockAlertAsync(pId, whId, 5, 1);

            // Assert
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Alert>()), Times.Never);
        }

        [TestMethod]
        public async Task AcknowledgeAsync_ShouldSetIsAcknowledgedToTrue()
        {
            // Arrange
            var id = 1;
            var alert = new Alert { AlertId = id, IsAcknowledged = false };
            _mockRepo!.Setup(r => r.FindUnacknowledgedAsync()).ReturnsAsync(new List<Alert> { alert });

            // Act
            await _service!.AcknowledgeAsync(id);

            // Assert
            Assert.IsTrue(alert.IsAcknowledged);
            _mockRepo.Verify(r => r.AddAsync(alert), Times.Once);
        }
    }
}
