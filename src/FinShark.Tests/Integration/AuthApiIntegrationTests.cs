using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FinShark.Domain.Entities;
using FinShark.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace FinShark.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("FINSHARK_USE_INMEMORY_DB", "true");

        builder.ConfigureServices(services =>
        {
            // Ensure in-memory DB is created for tests
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        });
    }
}

public sealed class AuthApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static StringContent ToJsonContent(object obj)
        => new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

    [Fact]
    public async Task AdminAssignRoleAndGetUsersFlow_Works()
    {
        // Arrange - create an admin user directly through DI
        var adminEmail = "admin@example.com";
        var adminPassword = "Admin123!";
        var userEmail = "user@example.com";
        var userPassword = "User123!";

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole("User"));
            }

            var admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(admin, adminPassword);
            Assert.True(result.Succeeded, "Admin user creation failed.");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Arrange - register the target user through API
        var registerResponse = await _client.PostAsync("/api/auth/register", ToJsonContent(new { Email = userEmail, Password = userPassword }));
        registerResponse.EnsureSuccessStatusCode();

        var registerResponseBody = await registerResponse.Content.ReadAsStringAsync();
        dynamic registerJson = JsonConvert.DeserializeObject(registerResponseBody)!;
        string targetUserId = registerJson.data.user.id;

        // Login as admin to obtain bearer token
        var loginResponse = await _client.PostAsync("/api/auth/login", ToJsonContent(new { Email = adminEmail, Password = adminPassword }));
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        dynamic loginJson = JsonConvert.DeserializeObject(loginBody)!;
        string token = loginJson.data.token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - assign Admin role to target user
        var assignResponse = await _client.PostAsync("/api/auth/admin/assign-role", ToJsonContent(new { UserId = targetUserId, Role = "Admin" }));
        assignResponse.EnsureSuccessStatusCode();

        // Act - get all users
        var getUsersResponse = await _client.GetAsync("/api/auth/admin/users");
        getUsersResponse.EnsureSuccessStatusCode();

        var getUsersBody = await getUsersResponse.Content.ReadAsStringAsync();
        dynamic getUsersJson = JsonConvert.DeserializeObject(getUsersBody)!;

        // Assert - target user appears and has roles
        Assert.NotNull(getUsersJson.data);
        var users = getUsersJson.data;
        bool hasTargetUser = false;
        bool hasAdminRole = false;

        foreach (var u in users)
        {
            if ((string)u.id == targetUserId)
            {
                hasTargetUser = true;
                foreach (var role in u.roles)
                {
                    if ((string)role == "Admin")
                    {
                        hasAdminRole = true;
                        break;
                    }
                }
            }
        }

        Assert.True(hasTargetUser, "Assigned user should be present in the returned users list.");
        Assert.True(hasAdminRole, "Assigned user should have Admin role.");
    }

    [Fact]
    public async Task EmailConfirmationFlow_Works()
    {
        var email = "confirmtest@example.com";
        var password = "Password123!";

        // Register user
        var registerResponse = await _client.PostAsync("/api/auth/register", ToJsonContent(new { Email = email, Password = password }));
        registerResponse.EnsureSuccessStatusCode();
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        dynamic registerJson = JsonConvert.DeserializeObject(registerBody)!;
        string userId = registerJson.data.user.id;

        // Login should fail before confirmation
        var loginResponse = await _client.PostAsync("/api/auth/login", ToJsonContent(new { Email = email, Password = password }));
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);

        // Resend confirmation and get token
        var resendResponse = await _client.PostAsync("/api/auth/resend-confirmation", ToJsonContent(new { Email = email }));
        resendResponse.EnsureSuccessStatusCode();
        var resendBody = await resendResponse.Content.ReadAsStringAsync();
        dynamic resendJson = JsonConvert.DeserializeObject(resendBody)!;
        string token = resendJson.data.token;

        // Confirm email
        var confirmResponse = await _client.GetAsync($"/api/auth/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}");
        confirmResponse.EnsureSuccessStatusCode();

        // Login should now succeed
        var loginResponseAfter = await _client.PostAsync("/api/auth/login", ToJsonContent(new { Email = email, Password = password }));
        loginResponseAfter.EnsureSuccessStatusCode();
    }
}

