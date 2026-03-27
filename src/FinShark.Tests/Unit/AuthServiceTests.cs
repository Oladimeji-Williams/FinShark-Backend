using FinShark.Application.Auth.Services;
using FinShark.Application.Common;
using FinShark.Application.Dtos;
using FinShark.Domain.Entities;
using FinShark.Persistence.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinShark.Tests.Unit;

public class AuthServiceTests
{
    private static IAuthService CreateAuthService(
        Mock<UserManager<ApplicationUser>> userManagerMock,
        Mock<RoleManager<IdentityRole>> roleManagerMock)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-jwt-key-very-long-and-secure",
                ["Jwt:Issuer"] = "FinShark",
                ["Jwt:Audience"] = "FinSharkUsers",
                ["Jwt:ExpiryInMinutes"] = "60"
            })
            .Build();

        var loggerMock = new Mock<ILogger<AuthService>>();

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock.Setup(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var hostEnvironmentMock = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns(Microsoft.Extensions.Hosting.Environments.Development);

        var appUrlProviderMock = new Mock<FinShark.Application.Common.IAppUrlProvider>();
        appUrlProviderMock.Setup(p => p.GetClientUrl()).Returns("https://localhost:5001");

        return new AuthService(
            userManagerMock.Object,
            roleManagerMock.Object,
            configuration,
            hostEnvironmentMock.Object,
            appUrlProviderMock.Object,
            emailServiceMock.Object,
            loggerMock.Object);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        return userManagerMock;
    }

    private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
    {
        var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
        var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            roleStoreMock.Object,
            null!,
            null!,
            null!,
            null!);

        return roleManagerMock;
    }

    [Fact]
    public async Task AssignRoleAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        var roleManagerMock = CreateRoleManagerMock();

        userManagerMock
            .Setup(m => m.FindByIdAsync("invalid-user-id"))
            .ReturnsAsync((ApplicationUser?)null);

        var service = CreateAuthService(userManagerMock, roleManagerMock);
        var request = new AssignRoleRequestDto("invalid-user-id", "Admin");

        // Act
        var result = await service.AssignRoleAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task AssignRoleAsync_RoleExists_AddsRoleToUser()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-id", Email = "test@example.com", UserName = "test@example.com" };
        var userManagerMock = CreateUserManagerMock();
        var roleManagerMock = CreateRoleManagerMock();

        userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        roleManagerMock.Setup(m => m.RoleExistsAsync("Admin")).ReturnsAsync(true);
        userManagerMock.Setup(m => m.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
        userManagerMock.Setup(m => m.AddToRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        var service = CreateAuthService(userManagerMock, roleManagerMock);
        var request = new AssignRoleRequestDto(user.Id, "Admin");

        // Act
        var result = await service.AssignRoleAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Role assigned successfully", result.Message);
        Assert.NotNull(result.User);
        Assert.Contains("Admin", result.User!.Roles);
    }

    [Fact]
    public async Task AssignRoleAsync_RoleDoesNotExist_CreatesRoleAndAssigns()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-id-2", Email = "user2@example.com", UserName = "user2@example.com" };
        var userManagerMock = CreateUserManagerMock();
        var roleManagerMock = CreateRoleManagerMock();

        userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        roleManagerMock.Setup(m => m.RoleExistsAsync("Admin")).ReturnsAsync(false);
        roleManagerMock.Setup(m => m.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(m => m.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
        userManagerMock.Setup(m => m.AddToRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        var service = CreateAuthService(userManagerMock, roleManagerMock);
        var request = new AssignRoleRequestDto(user.Id, "Admin");

        // Act
        var result = await service.AssignRoleAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Role assigned successfully", result.Message);
        Assert.NotNull(result.User);
        Assert.Contains("Admin", result.User!.Roles);
        roleManagerMock.Verify(m => m.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Admin")), Times.Once);
    }
}
