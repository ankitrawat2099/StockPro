using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SupplierService.Tests
{
    [TestClass]
    public class SupplierServiceTests
    {
        private Mock<ISupplierRepository>? _mockRepo;
        private SupplierDbContext? _context;
        private SupplierServiceImpl? _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<ISupplierRepository>();

            var options = new DbContextOptionsBuilder<SupplierDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SupplierDbContext(options);
            _service = new SupplierServiceImpl(_mockRepo.Object, _context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public async Task CreateSupplier_ShouldReturnSavedSupplier()
        {
            // Arrange
            var dto = new CreateSupplierDto { 
                Name = "Supp 1", Email = "s1@s.com", Phone = "111", TaxId = "T1", PaymentTerms = "Net 30",
                Address = "Addr", City = "City", Country = "Country", ContactPerson = "CP"
            };
            _mockRepo!.Setup(r => r.FindByTaxId(dto.TaxId)).ReturnsAsync((Supplier?)null);

            // Act
            var result = await _service!.CreateSupplier(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(dto.Name, result.Name);
        }

        [TestMethod]
        public async Task GetById_ShouldReturnSupplier_WhenExists()
        {
            // Arrange
            var id = 1;
            var supplier = new Supplier { 
                SupplierId = id, Name = "S1", IsActive = true, Email = "e", Phone = "p", 
                City = "c", Country = "c", ContactPerson = "cp", Address = "a",
                TaxId = "T1", PaymentTerms = "P1" 
            };
            _mockRepo!.Setup(r => r.FindBySupplierId(id)).ReturnsAsync(supplier);

            // Act
            var result = await _service!.GetById(id);

            // Assert
            Assert.AreEqual(id, result.SupplierId);
        }

        [TestMethod]
        public async Task UpdateRating_ShouldUpdateCorrectly()
        {
            // Arrange
            var id = 1;
            var supplier = new Supplier { 
                SupplierId = id, Name = "S1", Rating = 4.0, IsActive = true, Email = "e", Phone = "p", 
                City = "c", Country = "c", ContactPerson = "cp", Address = "a",
                TaxId = "T2", PaymentTerms = "P2"
            };
            _mockRepo!.Setup(r => r.FindBySupplierId(id)).ReturnsAsync(supplier);
            _context!.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            // Act
            await _service!.UpdateRating(id, 2.0);

            // Assert
            Assert.AreEqual(3.0, supplier.Rating);
        }
    }
}
