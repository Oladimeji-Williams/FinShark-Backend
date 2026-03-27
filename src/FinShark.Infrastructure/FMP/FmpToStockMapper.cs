using FinShark.Domain.Entities;
using FinShark.Domain.ValueObjects;

namespace FinShark.Infrastructure.FMP;

internal static class FmpToStockMapper
{
    public static Stock MapToStock(FmpStockApiModel source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (string.IsNullOrWhiteSpace(source.Symbol)) throw new ArgumentException("FMP response contains empty symbol.", nameof(source));
        if (string.IsNullOrWhiteSpace(source.CompanyName)) throw new ArgumentException("FMP response contains empty company name.", nameof(source));
        if (source.Price <= 0) throw new ArgumentException("FMP response must include a positive current price.", nameof(source));

        var stock = new Stock(
            symbol: source.Symbol.Trim().ToUpperInvariant(),
            companyName: source.CompanyName.Trim(),
            currentPrice: Convert.ToDecimal(source.Price),
            purchase: 0m,
            lastDiv: Convert.ToDecimal(source.LastDiv));

        if (SectorType.TryFrom(source.Sector, out var parsedSector))
        {
            stock.Update(sector: parsedSector);
        }

        if (source.MktCap > 0)
        {
            stock.Update(marketCap: Convert.ToDecimal(source.MktCap));
        }

        return stock;
    }

    public static Stock MapToStock(FmpStockProfile source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (string.IsNullOrWhiteSpace(source.Symbol)) throw new ArgumentException("FMP profile contains empty symbol.", nameof(source));
        if (string.IsNullOrWhiteSpace(source.CompanyName)) throw new ArgumentException("FMP profile contains empty company name.", nameof(source));
        if (source.Price <= 0) throw new ArgumentException("FMP profile must include a positive current price.", nameof(source));

        var stock = new Stock(
            symbol: source.Symbol.Trim().ToUpperInvariant(),
            companyName: source.CompanyName.Trim(),
            currentPrice: source.Price,
            purchase: 0m,
            lastDiv: source.LastDiv);

        if (SectorType.TryFrom(source.Sector, out var parsedSector))
        {
            stock.Update(sector: parsedSector);
        }

        if (source.MktCap > 0)
        {
            stock.Update(marketCap: source.MktCap);
        }

        return stock;
    }
}
