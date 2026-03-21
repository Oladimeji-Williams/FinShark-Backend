using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using FinShark.Domain.Entities;
using FinShark.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

[Collection("Integration")]
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
        var registerResponse = await _client.PostAsync("/api/auth/register", ToJsonContent(new { UserName = "targetuser", Email = userEmail, Password = userPassword }));
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
        var registerResponse = await _client.PostAsync("/api/auth/register", ToJsonContent(new { UserName = "confirmuser", Email = email, Password = password }));
        registerResponse.EnsureSuccessStatusCode();
        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        dynamic registerJson = JsonConvert.DeserializeObject(registerBody)!;
        string userId = registerJson.data.user.id;

        // Login should fail before confirmation
        var loginResponse = await _client.PostAsync("/api/auth/login", ToJsonContent(new { Email = email, Password = password }));
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);

        // Resend confirmation link (no token returned by design)
        var resendResponse = await _client.PostAsync("/api/auth/resend-confirmation", ToJsonContent(new { Email = email }));
        resendResponse.EnsureSuccessStatusCode();

        // Use UserManager to generate the expected token for confirmation
        string token;
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await userManager.GenerateEmailConfirmationTokenAsync(user!)));
        }

        // Confirm email
        var confirmResponse = await _client.GetAsync($"/api/auth/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}");
        confirmResponse.EnsureSuccessStatusCode();

        // Login should now succeed
        var loginResponseAfter = await _client.PostAsync("/api/auth/login", ToJsonContent(new { Email = email, Password = password }));
        loginResponseAfter.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PortfolioAddGetRemoveFlow_Works()
    {
        var email = "portfolio.user@example.com";
        var password = "Portfolio123!";
        var userName = "portfoliouser";

        // Register user
        var registerResponse = await _client.PostAsync("/api/auth/register", ToJsonContent(new { UserName = userName, Email = email, Password = password }));
        registerResponse.EnsureSuccessStatusCode();

        // Confirm user email directly through UserManager for test reliability.
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            Assert.NotNull(user);
            user!.EmailConfirmed = true;
            var updateResult = await userManager.UpdateAsync(user);
            Assert.True(updateResult.Succeeded, "Could not confirm test user email");
        }

        // Login to get bearer token
        var loginResponse = await _client.PostAsync("/api/auth/login", ToJsonContent(new { Email = email, Password = password }));
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        var loginJson = JObject.Parse(loginBody);
        var token = loginJson["data"]!["token"]!.Value<string>();
        Assert.False(string.IsNullOrWhiteSpace(token));

        var authClient = _factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a stock to add to portfolio
        var createStockResponse = await authClient.PostAsync("/api/stocks", ToJsonContent(new
        {
            Symbol = "TST1",
            CompanyName = "Test Stock 1",
            CurrentPrice = 12.34m,
            Sector = "Technology",
            MarketCap = 5000000m
        }));
        createStockResponse.EnsureSuccessStatusCode();

        var createStockBody = await createStockResponse.Content.ReadAsStringAsync();
        var createStockJson = JObject.Parse(createStockBody);
        var stockId = createStockJson["data"]!["id"]!.Value<int>();

        // Add the stock to portfolio
        var addResponse = await authClient.PostAsync($"/api/portfolio/{stockId}", null);
        addResponse.EnsureSuccessStatusCode();
        var addJson = JObject.Parse(await addResponse.Content.ReadAsStringAsync());
        Assert.True(addJson["data"]!.Value<bool>());

        // Get portfolio and verify the newly added stock is present
        var getPortfolioResponse = await authClient.GetAsync("/api/portfolio");
        getPortfolioResponse.EnsureSuccessStatusCode();
        var portfolioJson = JObject.Parse(await getPortfolioResponse.Content.ReadAsStringAsync());
        var portfolioData = (JArray)portfolioJson["data"]!;
        Assert.Single(portfolioData);
        Assert.Equal("TST1", portfolioData[0]["symbol"]!.Value<string>());

        // Add same stock again should return false (already in portfolio)
        var addDuplicateResponse = await authClient.PostAsync($"/api/portfolio/{stockId}", null);
        addDuplicateResponse.EnsureSuccessStatusCode();
        var addDuplicateJson = JObject.Parse(await addDuplicateResponse.Content.ReadAsStringAsync());
        Assert.False(addDuplicateJson["data"]!.Value<bool>());

        // Remove from portfolio
        var removeResponse = await authClient.DeleteAsync($"/api/portfolio/{stockId}");
        removeResponse.EnsureSuccessStatusCode();
        var removeJson = JObject.Parse(await removeResponse.Content.ReadAsStringAsync());
        Assert.True(removeJson["data"]!.Value<bool>());

        // Ensure portfolio is now empty
        var getPortfolioAfterRemoveResponse = await authClient.GetAsync("/api/portfolio");
        getPortfolioAfterRemoveResponse.EnsureSuccessStatusCode();
        var portfolioAfterJson = JObject.Parse(await getPortfolioAfterRemoveResponse.Content.ReadAsStringAsync());
        var portfolioAfterData = (JArray)portfolioAfterJson["data"]!;
        Assert.Empty(portfolioAfterData);
    }
}

