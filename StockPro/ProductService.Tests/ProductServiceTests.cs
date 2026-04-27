using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProductService.Services;
using ProductService.Repositories;
using ProductService.Data;
using ProductService.Entities;
using ProductService.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace ProductService.Tests
{
    [TestClass]
    public class ProductServiceTests
    {
        private Mock<IProductRepository>? _mockRepo;
        private Mock<IHttpClientFactory>? _mockHttpClientFactory;
        private Mock<IConfiguration>? _mockConfig;
        private ProductDbContext? _context;
        private ProductServiceImpl? _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<IProductRepository>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockConfig = new Mock<IConfiguration>();

            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ProductDbContext(options);
            _service = new ProductServiceImpl(_mockRepo.Object, _context, _mockHttpClientFactory.Object, _mockConfig.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public async Task CreateProductAsync_ShouldCreateProduct_WhenValidRequest()
        {
            // Arrange
            var request = new ProductRequest
            {
                Sku = "SKU001",
                Name = "Test Product",
                Description = "Description",
                Category = "Category",
                Brand = "Brand",
                UnitOfMeasure = "Unit",
                CostPrice = 10.0,
                SellingPrice = 15.0,
                ReorderLevel = 5,
                MaxStockLevel = 100,
                LeadTimeDays = 2,
                ImageUrl = "http://image.com",
                Barcode = "123456789"
            };

            // Act
            var result = await _service!.CreateProductAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(request.Name, result.Name);
            Assert.AreEqual(request.Sku, result.Sku);
            
            var productInDb = await _context!.Products.FirstOrDefaultAsync(p => p.Sku == request.Sku);
            Assert.IsNotNull(productInDb);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product
            {
                ProductId = productId,
                Sku = "SKU002",
                Name = "Product 2",
                Description = "Desc 2",
                Category = "Cat 2",
                Brand = "Brand 2",
                UnitOfMeasure = "Unit 2",
                Barcode = "987654321",
                ImageUrl = "img",
                IsActive = true
            };

            _mockRepo!.Setup(r => r.FindByProductIdAsync(productId)).ReturnsAsync(product);

            // Act
            var result = await _service!.GetByIdAsync(productId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(product.Name, result.Name);
            Assert.AreEqual(product.Sku, result.Sku);
        }

        [TestMethod]
        public async Task DeactivateProductAsync_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product
            {
                ProductId = productId,
                Sku = "SKU003",
                Name = "Product 3",
                IsActive = true,
                Description = "d", Category = "c", Brand = "b", UnitOfMeasure = "u", Barcode = "b", ImageUrl = "i"
            };

            _mockRepo!.Setup(r => r.FindByProductIdAsync(productId)).ReturnsAsync(product);
            _context!.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            await _service!.DeactivateProductAsync(productId);

            // Assert
            var productInDb = await _context.Products.FindAsync(productId);
            Assert.IsNotNull(productInDb);
            Assert.IsFalse(productInDb.IsActive);
        }
    }
}
