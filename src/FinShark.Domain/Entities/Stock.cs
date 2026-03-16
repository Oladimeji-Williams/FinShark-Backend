namespace FinShark.Domain.Entities;

using FinShark.Domain.ValueObjects;

public class Stock : BaseEntity
{
    public string Symbol { get; private set; } = null!;
    public string CompanyName { get; private set; } = null!;
    public decimal CurrentPrice { get; private set; }
    public decimal Purchase { get; private set; }
    public decimal LastDiv { get; private set; }
    public IndustryType Industry { get; set; } = IndustryType.Other;
    public decimal MarketCap { get; set; }
    public List<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PortfolioItem> PortfolioItems { get; set; } = new List<PortfolioItem>();

    private Stock() { } // EF Core needs parameterless constructor

    public Stock(string symbol, string companyName, decimal currentPrice, decimal purchase = 0, decimal lastDiv = 0)
    {
        if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol cannot be empty", nameof(symbol));
        if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("Company name cannot be empty", nameof(companyName));
        if (currentPrice <= 0) throw new ArgumentException("Current price must be greater than zero", nameof(currentPrice));

        Symbol = symbol;
        CompanyName = companyName;
        CurrentPrice = currentPrice;
        Purchase = purchase;
        LastDiv = lastDiv;
    }

    public void Update(string? symbol = null, string? companyName = null, decimal? currentPrice = null, 
        IndustryType? industry = null, decimal? marketCap = null)
    {
        if (!string.IsNullOrWhiteSpace(symbol)) Symbol = symbol;
        if (!string.IsNullOrWhiteSpace(companyName)) CompanyName = companyName;
        if (currentPrice.HasValue && currentPrice.Value > 0) CurrentPrice = currentPrice.Value;
        if (industry.HasValue) Industry = industry.Value;
        if (marketCap.HasValue && marketCap.Value >= 0) MarketCap = marketCap.Value;
    }
}
