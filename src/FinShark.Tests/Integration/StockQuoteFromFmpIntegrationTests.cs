using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FinShark.Domain.Entities;
using FinShark.Domain.Exceptions;
using FinShark.Domain.Interfaces;
using FinShark.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FinShark.Tests.Integration;

[Collection("Integration")]
public sealed class StockQuoteFromFmpIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public StockQuoteFromFmpIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing IFMPService registration and replace with mock.
                var descriptor = services.SingleOrDefault(sd => sd.ServiceType == typeof(IFMPService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var mockFmpService = new Mock<IFMPService>(MockBehavior.Strict);
                mockFmpService.Setup(s => s.GetStockQuoteAsync("AAPL", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Stock("AAPL", "Apple Inc.", 150.25m) { Id = 999 });

                mockFmpService.Setup(s => s.GetFullStockProfileAsync("AAPL", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new FinShark.Domain.ValueObjects.FmpStockProfile
                    {
                        Symbol = "AAPL",
                        Price = 150.25m,
                        Beta = 1.2m,
                        VolAvg = 80000000,
                        MktCap = 2400000000000,
                        LastDiv = 0.9m,
                        Range = "130-170",
                        Changes = 1.2m,
                        CompanyName = "Apple Inc.",
                        Currency = "USD",
                        Cik = "0000320193",
                        Isin = "US0378331005",
                        Cusip = "037833100",
                        Exchange = "NASDAQ",
                        ExchangeShortName = "NASDAQ",
                        Website = "https://www.apple.com",
                        Description = "Apple Inc. designs, manufactures, and markets smartphones, personal computers, tablets, wearables, and accessories worldwide.",
                        Ceo = "Tim Cook",
                        Sector = "Technology",
                        Country = "USA",
                        FullTimeEmployees = "154000",
                        Phone = "1-800-MY-APPLE",
                        Address = "One Apple Park Way",
                        City = "Cupertino",
                        State = "CA",
                        Zip = "95014",
                        DcfDiff = 10.55m,
                        Dcf = 160m,
                        Image = "https://logo.clearbit.com/apple.com",
                        IpoDate = "1980-12-12",
                        DefaultImage = false,
                        IsEtf = false,
                        IsActivelyTrading = true,
                        IsAdr = false,
                        IsFund = false
                    });

                services.AddScoped(_ => mockFmpService.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetStockQuoteFromFmp_ReturnsMappedStockResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/stocks/quote/AAPL");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var parsed = JObject.Parse(body);

        Assert.True(parsed["success"]?.Value<bool>());
        Assert.NotNull(parsed["data"]);
        var data = parsed["data"]!;
        Assert.Equal("AAPL", data["symbol"]?.ToString());
        Assert.Equal("Apple Inc.", data["companyName"]?.ToString());
        Assert.Equal(150.25m, data["currentPrice"]!.Value<decimal>());
    }

    [Fact]
    public async Task GetFullStockQuoteFromFmp_ReturnsFullProfileDtoWithRequiredFields()
    {
        // Act
        var response = await _client.GetAsync("/api/stocks/quote/full/AAPL");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var parsed = JObject.Parse(body);

        Assert.True(parsed["success"]?.Value<bool>());
        Assert.NotNull(parsed["data"]);
        var data = parsed["data"]!;

        Assert.Equal("AAPL", data["symbol"]?.ToString());
        Assert.Equal("Apple Inc.", data["companyName"]?.ToString());
        Assert.Equal(150.25m, data["price"]!.Value<decimal>());
    }

    [Fact]
    public async Task AddStockBySymbol_Fmp403_ReturnsFmpStatusCodeAndSuggestion()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(sd => sd.ServiceType == typeof(IFMPService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var mockFmpService = new Mock<IFMPService>(MockBehavior.Strict);
                mockFmpService.Setup(s => s.GetStockQuoteAsync("MA", It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new FMPServiceException("Access to FMP is forbidden. Verify your API key and account permissions.", 403, "Try a different symbol or verify your FMP quota."));

                services.AddScoped(_ => mockFmpService.Object);
            });
        });

        var client = factory.CreateClient();

        var registerContent = new StringContent(JsonConvert.SerializeObject(new { UserName = "fmp403user", Email = "fmp403user@example.com", Password = "Password123!" }), Encoding.UTF8, "application/json");
        var registerResponse = await client.PostAsync("/api/auth/register", registerContent);
        registerResponse.EnsureSuccessStatusCode();

        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<FinShark.Domain.Entities.ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("fmp403user@example.com");
            Assert.NotNull(user);
            user!.EmailConfirmed = true;
            Assert.True((await userManager.UpdateAsync(user)).Succeeded);
        }

        var loginContent = new StringContent(JsonConvert.SerializeObject(new { Email = "fmp403user@example.com", Password = "Password123!" }), Encoding.UTF8, "application/json");
        var loginResponse = await client.PostAsync("/api/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        var loginJson = JObject.Parse(loginBody);
        var token = loginJson["data"]?["token"]?.Value<string>();
        Assert.False(string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!);

        var portfolioResponse = await client.PostAsync("/api/portfolio/symbol/MA", new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, portfolioResponse.StatusCode);

        var portfolioBody = await portfolioResponse.Content.ReadAsStringAsync();
        var parsed = JObject.Parse(portfolioBody);

        Assert.False(parsed["success"]?.Value<bool>());
        Assert.Equal(403, parsed["fmpStatusCode"]?.Value<int>());
        Assert.Equal("Try a different symbol or verify your FMP quota.", parsed["suggestion"]?.ToString());
    }
}
