using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

namespace StockMovementService.Tests
{
    [TestClass]
    public class StockMovementServiceTests
    {
        private Mock<IMovementRepository>? _mockRepo;
        private Mock<IHttpContextAccessor>? _mockHttpContextAccessor;
        private MovementServiceImpl? _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<IMovementRepository>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _service = new MovementServiceImpl(_mockRepo.Object, _mockHttpContextAccessor.Object);
        }

        [TestMethod]
        public async Task RecordMovementAsync_ShouldAddMovement_WhenValidRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext { User = principal };
            _mockHttpContextAccessor!.Setup(a => a.HttpContext).Returns(httpContext);

            var dto = new CreateMovementDto
            {
                ProductId = Guid.NewGuid(),
                WarehouseId = 1,
                MovementType = "STOCK_IN",
                Quantity = 10,
                BalanceAfter = 100
            };

            // Act
            await _service!.RecordMovementAsync(dto);

            // Assert
            _mockRepo!.Verify(r => r.AddAsync(It.IsAny<StockMovement>()), Times.Once);
        }

        [TestMethod]
        public async Task RecordMovementAsync_ShouldThrow_WhenQuantityIsZero()
        {
            // Arrange
            var dto = new CreateMovementDto { Quantity = 0 };

            // Act & Assert
            try
            {
                await _service!.RecordMovementAsync(dto);
                Assert.Fail("ArgumentException expected");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Quantity must be greater than zero", ex.Message);
            }
        }

        [TestMethod]
        public async Task RecordMovementAsync_ShouldThrow_WhenUserNotAuthenticated()
        {
            // Arrange
            _mockHttpContextAccessor!.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
            var dto = new CreateMovementDto { Quantity = 10, MovementType = "STOCK_IN" };

            // Act & Assert
            try
            {
                await _service!.RecordMovementAsync(dto);
                Assert.Fail("UnauthorizedAccessException expected");
            }
            catch (UnauthorizedAccessException)
            {
                // Success
            }
        }
    }
}
