using Moq;
using Microsoft.Extensions.Configuration;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Services;
using Xunit;

namespace NexusERP.Tests
{
    public class AuthServiceTests
    {
        private IConfiguration BuildFakeConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string?> {
                {"Jwt:Key", "ThisIsASuperSecretCryptographicKeyThatIsAtLeast32BytesLong!"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task Register_UsernameAlreadyExists_ThrowsAppException()
        {
            var mockRepo = new Mock<IUserRepository>();
            var config = BuildFakeConfiguration();
            var service = new AuthService(mockRepo.Object, config);

            mockRepo.Setup(r => r.GetUserByUsername("admin")).ReturnsAsync(new User());

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.Register("Admin User", "admin", "password123", 1, 99));

            Assert.Contains("already exists", ex.Message);
            mockRepo.Verify(r => r.CreateUser(It.IsAny<User>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Register_ValidData_HashesPasswordAndSavesUser()
        {
            var mockRepo = new Mock<IUserRepository>();
            var config = BuildFakeConfiguration();
            var service = new AuthService(mockRepo.Object, config);

            mockRepo.Setup(r => r.GetUserByUsername("newuser")).ReturnsAsync((User?)null);

            User? captureUser = null;

            mockRepo.Setup(r => r.CreateUser(It.IsAny<User>(), 99))
                .Callback<User, int>((u, id) => captureUser = u)
                .Returns(Task.CompletedTask);

            await service.Register("New User", "newuser", "plaintextPassword", 2, 99);

            Assert.NotNull(captureUser);
            Assert.Equal("newuser", captureUser.Username);
            Assert.Equal(2, captureUser.RoleId);

            Assert.NotEqual("plaintextPassword", captureUser.PasswordHash);

            Assert.True(BCrypt.Net.BCrypt.Verify("plaintextPassword", captureUser.PasswordHash));

            mockRepo.Verify(r => r.CreateUser(It.IsAny<User>(), 99), Times.Once);
        }



        [Fact]
        public async Task Login_UserNotFound_ThrowsAppException()
        {
            var mockRepo = new Mock<IUserRepository>();
            var config = BuildFakeConfiguration();
            var service = new AuthService(mockRepo.Object, config);

            mockRepo.Setup(r => r.GetUserByUsername("ghostuser")).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.Login("ghostuser", "anypassword"));

            Assert.Contains("Invalid username or", ex.Message);
        }

        [Fact]
        public async Task Login_InvalidPassword_ThrowsAppException()
        {
            var mockRepo = new Mock<IUserRepository>();
            var config = BuildFakeConfiguration();
            var service = new AuthService(mockRepo.Object, config);

            var realHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123");
            var dbUser = new User { Username = "validuser", PasswordHash = realHash };

            mockRepo.Setup(r => r.GetUserByUsername("validuser")).ReturnsAsync(dbUser);

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.Login("validuser", "WrongPassword!!"));

            Assert.Contains("Invalid username or password", ex.Message);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsJwtToken()
        {
            var mockRepo = new Mock<IUserRepository>();
            var config = BuildFakeConfiguration();
            var service = new AuthService(mockRepo.Object, config);

            var realHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123");
            var dbUser = new User
            {
                UserId = 1,
                FullName = "System Admin",
                Username = "validuser",
                PasswordHash = realHash,
                Role = new Role { Name = "Admin", Permissions = new List<string> { "Users.Manage" } }
            };

            mockRepo.Setup(r => r.GetUserByUsername("validuser")).ReturnsAsync(dbUser);

            var token = await service.Login("validuser", "CorrectPassword123");

            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.StartsWith("ey", token);
        }
    }
}
