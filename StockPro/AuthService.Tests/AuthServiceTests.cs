using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AuthService.Services;
using AuthService.Repositories;
using AuthService.Data;
using AuthService.Entities;
using AuthService.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AuthService.Tests
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<IAuthRepository>? _mockRepo;
        private Mock<IConfiguration>? _mockConfig;
        private AuthDbContext? _context;
        private AuthServiceImpl? _service;

        [TestInitialize]
        public void Setup()
        {
            _mockRepo = new Mock<IAuthRepository>();
            _mockConfig = new Mock<IConfiguration>();

            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AuthDbContext(options);
            _service = new AuthServiceImpl(_mockRepo.Object, _context, _mockConfig.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public async Task RegisterAsync_ShouldCreateUser_WhenEmailIsUnique()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "Password123",
                Role = "STAFF",
                Phone = "1234567890",
                Department = "IT"
            };

            _mockRepo!.Setup(r => r.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);

            // Act
            var result = await _service!.RegisterAsync(request);

            // Assert
            Assert.AreEqual("User registered successfully", result);
            var userInDb = await _context!.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            Assert.IsNotNull(userInDb);
            Assert.AreEqual(request.FullName, userInDb.FullName);
        }

        [TestMethod]
        public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "exists@example.com"
            };

            _mockRepo!.Setup(r => r.ExistsByEmailAsync(request.Email)).ReturnsAsync(true);

            // Act & Assert
            try
            {
                await _service!.RegisterAsync(request);
                Assert.Fail("Exception expected");
            }
            catch (Exception ex)
            {
                if (ex is AssertFailedException) throw;
                Assert.AreEqual("User already exists", ex.Message);
            }
        }

        [TestMethod]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new AppUser
            {
                UserId = userId,
                FullName = "John Doe",
                Email = "john@example.com",
                Role = "ADMIN",
                IsActive = true,
                Phone = "1112223333",
                Department = "HR",
                PasswordHash = "hashed_password"
            };

            _mockRepo!.Setup(r => r.FindByUserIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _service!.GetUserByIdAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.FullName, result.FullName);
            Assert.AreEqual(user.Email, result.Email);
        }
    }
}
