using FinShark.Domain.Entities;
using FinShark.Domain.Exceptions;
using FinShark.Domain.Interfaces;
using FinShark.Domain.ValueObjects;
using FinShark.Application.Dtos;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

namespace FinShark.Infrastructure.FMP;

/// <summary>
/// Connects to Financial Modeling Prep API to retrieve stock quote/profile data.
/// </summary>
public sealed class FMPService : IFMPService
{
    private readonly HttpClient _client;
    private readonly FmpSettings _settings;
    private readonly ILogger<FMPService> _logger;

    public FMPService(
        IHttpClientFactory httpClientFactory,
        FmpSettings settings,
        ILogger<FMPService> logger)
    {
        if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            throw new InvalidOperationException("FMP BaseUrl is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("FMP ApiKey is not configured.");
        }

        _client = httpClientFactory.CreateClient("FMPClient");
        _client.BaseAddress = new Uri(_settings.BaseUrl);
        _client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 30);
    }

    public async Task<Stock?> GetStockQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol is required.", nameof(symbol));
        }

        var symbolTrim = symbol.Trim().ToUpperInvariant();
        _logger.LogInformation("Retrieving FMP stock profile for symbol: {Symbol}", symbolTrim);

        // Reuse full profile mapping to ensure all metadata (sector, market cap, last dividend) is included.
        var profile = await GetFullStockProfileAsync(symbolTrim, cancellationToken);
        var stock = FmpToStockMapper.MapToStock(profile);

        _logger.LogInformation("Mapped FMP quote for symbol {Symbol} to domain stock entity using full profile.", stock.Symbol);
        return stock;
    }

    public async Task<FmpStockProfile> GetFullStockProfileAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol is required.", nameof(symbol));
        }

        var symbolTrim = symbol.Trim().ToUpperInvariant();
        _logger.LogInformation("Retrieving full FMP stock profile for symbol: {Symbol}", symbolTrim);

        var requestUri = $"/stable/profile?symbol={WebUtility.UrlEncode(symbolTrim)}&apikey={WebUtility.UrlEncode(_settings.ApiKey)}";

        using var response = await _client.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var (message, suggestion) = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => ("FMP API key is invalid or unauthorized. Please check your FMP API key.", "Verify your FMP API key in configuration."),
                HttpStatusCode.Forbidden => ("Access to FMP is forbidden. Verify your API key and account permissions.", "Try a different symbol or verify your FMP quota."),
                HttpStatusCode.BadRequest => ("FMP request is invalid. Please verify the requested symbol.", "Ensure the symbol is valid and try again."),
                HttpStatusCode.NotFound => ($"Stock symbol '{symbolTrim}' was not found in FMP.", "Try a different symbol if this one does not exist."),
                HttpStatusCode.TooManyRequests => ("FMP rate limit exceeded. Try again later.", "Wait a moment and retry, or upgrade your FMP plan."),
                _ => ($"FMP service returned {(int)response.StatusCode} ({response.ReasonPhrase}).", "Try again later or check FMP service status.")
            };

            _logger.LogWarning("FMP API request failed with status {StatusCode} for symbol {Symbol}: {Message}", (int)response.StatusCode, symbolTrim, message);
            throw new FMPServiceException(message, (int)response.StatusCode, suggestion);
        }

        var content = await response.Content.ReadFromJsonAsync<List<FMPStock>>(cancellationToken: cancellationToken);
        if (content is null || content.Count == 0)
        {
            _logger.LogWarning("FMP API returned empty profile list for symbol {Symbol}", symbolTrim);
            throw new StockNotFoundException($"Stock symbol '{symbolTrim}' not found in FMP.");
        }

        var fmpStock = content[0];
        _logger.LogInformation("Retrieved full FMP stock profile for symbol {Symbol}", symbolTrim);

        return new FmpStockProfile
        {
            Symbol = fmpStock.Symbol,
            Price = Convert.ToDecimal(fmpStock.Price),
            Beta = Convert.ToDecimal(fmpStock.Beta),
            VolAvg = fmpStock.VolAvg,
            MktCap = fmpStock.MktCap,
            LastDiv = Convert.ToDecimal(fmpStock.LastDiv),
            Range = fmpStock.Range,
            Changes = Convert.ToDecimal(fmpStock.Changes),
            CompanyName = fmpStock.CompanyName,
            Currency = fmpStock.Currency,
            Cik = fmpStock.Cik,
            Isin = fmpStock.Isin,
            Cusip = fmpStock.Cusip,
            Exchange = fmpStock.Exchange,
            ExchangeShortName = fmpStock.ExchangeShortName,
            Sector = string.IsNullOrWhiteSpace(fmpStock.Sector) ? string.Empty : fmpStock.Sector,
            Website = fmpStock.Website,
            Description = fmpStock.Description,
            Ceo = fmpStock.Ceo,
            Country = fmpStock.Country,
            FullTimeEmployees = fmpStock.FullTimeEmployees,
            Phone = fmpStock.Phone,
            Address = fmpStock.Address,
            City = fmpStock.City,
            State = fmpStock.State,
            Zip = fmpStock.Zip,
            DcfDiff = Convert.ToDecimal(fmpStock.DcfDiff),
            Dcf = Convert.ToDecimal(fmpStock.Dcf),
            Image = fmpStock.Image,
            IpoDate = fmpStock.IpoDate,
            DefaultImage = fmpStock.DefaultImage,
            IsEtf = fmpStock.IsEtf,
            IsActivelyTrading = fmpStock.IsActivelyTrading,
            IsAdr = fmpStock.IsAdr,
            IsFund = fmpStock.IsFund,
        };
    }
}
