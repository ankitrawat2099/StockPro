using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace PurchaseOrderService.Tests
{
    [TestClass]
    public class PurchaseOrderServiceTests
    {
        private Mock<IPurchaseRepository>? _mockRepo;
        private Mock<IHttpClientFactory>? _mockHttpClientFactory;
        private Mock<IHttpContextAccessor>? _mockHttpContextAccessor;
        private PurchaseDbContext? _context;
        private PurchaseServiceImpl? _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<IPurchaseRepository>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var options = new DbContextOptionsBuilder<PurchaseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PurchaseDbContext(options);
            _service = new PurchaseServiceImpl(_mockRepo.Object, _mockHttpClientFactory.Object, _context, _mockHttpContextAccessor.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public async Task SubmitForApproval_ShouldChangeStatusToPending_WhenDraft()
        {
            // Arrange
            var po = new PurchaseOrder { Status = "DRAFT", SupplierId = 1, WarehouseId = 1, CreatedById = Guid.NewGuid() };
            _context!.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            // Act
            await _service!.SubmitForApproval(po.PoId);

            // Assert
            var updatedPo = await _context.PurchaseOrders.FindAsync(po.PoId);
            Assert.AreEqual("PENDING", updatedPo!.Status);
        }

        [TestMethod]
        public async Task SubmitForApproval_ShouldThrow_WhenNotDraft()
        {
            // Arrange
            var po = new PurchaseOrder { Status = "APPROVED", SupplierId = 1, WarehouseId = 1, CreatedById = Guid.NewGuid() };
            _context!.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            // Act & Assert
            try
            {
                await _service!.SubmitForApproval(po.PoId);
                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                if (ex is AssertFailedException) throw;
                Assert.AreEqual("PO must be in DRAFT state to be submitted", ex.Message);
            }
        }

        [TestMethod]
        public async Task ApprovePO_ShouldChangeStatusToApproved_WhenPending()
        {
            // Arrange
            var po = new PurchaseOrder { Status = "PENDING", SupplierId = 1, WarehouseId = 1, CreatedById = Guid.NewGuid() };
            _context!.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            // Act
            await _service!.ApprovePO(po.PoId);

            // Assert
            var updatedPo = await _context.PurchaseOrders.FindAsync(po.PoId);
            Assert.AreEqual("APPROVED", updatedPo!.Status);
        }

        [TestMethod]
        public async Task CancelPO_ShouldChangeStatusToCancelled_WhenNotReceived()
        {
            // Arrange
            var po = new PurchaseOrder { Status = "DRAFT", SupplierId = 1, WarehouseId = 1, CreatedById = Guid.NewGuid() };
            _context!.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            // Act
            await _service!.CancelPO(po.PoId);

            // Assert
            var updatedPo = await _context.PurchaseOrders.FindAsync(po.PoId);
            Assert.AreEqual("CANCELLED", updatedPo!.Status);
        }
    }
}
