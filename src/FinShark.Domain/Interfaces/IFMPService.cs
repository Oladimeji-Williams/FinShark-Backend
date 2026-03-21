using FinShark.Domain.Entities;
using FinShark.Domain.ValueObjects;

namespace FinShark.Domain.Interfaces;

public interface IFMPService
{
    Task<Stock?> GetStockQuoteAsync(string symbol, CancellationToken cancellationToken = default);
    Task<FmpStockProfile> GetFullStockProfileAsync(string symbol, CancellationToken cancellationToken = default);
}
