using FinShark.Domain.Entities;
using FinShark.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinShark.Persistence.Seeding;

/// <summary>
/// Handles database seeding with dummy data
/// </summary>
public sealed class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(AppDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Seeds the database with sample stock data
    /// Only seeds if database is empty
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Check if stocks already exist
            if (_context.Stocks.Any())
            {
                _logger.LogInformation("Database already contains stocks. Skipping seeding.");
                return;
            }

            _logger.LogInformation("Starting database seeding with dummy data...");

            var stocks = new List<Stock>
            {
                new Stock("AAPL", "Apple Inc.", 150.25m, purchase: 120.00m, lastDiv: 0.88m) 
                { 
                    Industry = IndustryType.Technology,
                    MarketCap = 2500000000000m
                },
                new Stock("MSFT", "Microsoft Corporation", 320.50m, purchase: 280.00m, lastDiv: 0.62m)
                {
                    Industry = IndustryType.Technology,
                    MarketCap = 2350000000000m
                },
                new Stock("GOOGL", "Alphabet Inc.", 140.75m, purchase: 130.00m, lastDiv: 0m)
                {
                    Industry = IndustryType.Technology,
                    MarketCap = 1850000000000m
                },
                new Stock("AMZN", "Amazon.com Inc.", 165.30m, purchase: 150.25m, lastDiv: 0m)
                {
                    Industry = IndustryType.Retail,
                    MarketCap = 1700000000000m
                },
                new Stock("TSLA", "Tesla Inc.", 245.60m, purchase: 200.00m, lastDiv: 0m)
                {
                    Industry = IndustryType.Manufacturing,
                    MarketCap = 780000000000m
                },
                new Stock("META", "Meta Platforms Inc.", 485.75m, purchase: 450.00m, lastDiv: 0.54m)
                {
                    Industry = IndustryType.Technology,
                    MarketCap = 1250000000000m
                },
                new Stock("NVDA", "NVIDIA Corporation", 875.30m, purchase: 800.00m, lastDiv: 0.32m)
                {
                    Industry = IndustryType.Technology,
                    MarketCap = 2150000000000m
                },
                new Stock("JPM", "JPMorgan Chase & Co.", 195.45m, purchase: 180.00m, lastDiv: 0.98m)
                {
                    Industry = IndustryType.Finance,
                    MarketCap = 550000000000m
                },
                new Stock("V", "Visa Inc.", 250.80m, purchase: 230.00m, lastDiv: 0.42m)
                {
                    Industry = IndustryType.Finance,
                    MarketCap = 520000000000m
                },
                new Stock("DIS", "The Walt Disney Company", 92.15m, purchase: 85.00m, lastDiv: 0.15m)
                {
                    Industry = IndustryType.Entertainment,
                    MarketCap = 168000000000m
                }
            };

            // Attach a comment to first few stocks for demonstration
            if (stocks.Count > 0)
            {
                stocks[0].Comments.Add(new Comment { Title = "First pick", Content = "Great long-term investment" });
            }
            if (stocks.Count > 2)
            {
                stocks[2].Comments.Add(new Comment { Title = "Watch this", Content = "Volatile but promising" });
            }
            if (stocks.Count > 4)
            {
                stocks[4].Comments.Add(new Comment { Title = "High risk", Content = "Tesla has big ups and downs" });
            }

            await _context.Stocks.AddRangeAsync(stocks);
            var stocksSaved = await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} stocks into the database", stocksSaved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
